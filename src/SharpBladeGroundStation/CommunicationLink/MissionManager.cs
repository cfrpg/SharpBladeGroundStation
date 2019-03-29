using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Diagnostics;
using SharpBladeGroundStation.DataStructs;
using SharpBladeGroundStation.Map.Markers;
using System.MAVLink;
using SharpBladeGroundStation.Map;
using GMap.NET.WindowsPresentation;

namespace SharpBladeGroundStation.CommunicationLink
{
	public class MissionManager
	{
		Vehicle target;

        ObservableCollection<MissionItem> missionList;
		
		List<LinkPackage> sendPackages;
		List<LinkPackage> receivedPackage;

		bool isBusy;
		State state;
		ushort waypointCount;
		ushort currentWaypoint;

		public delegate void MissionSenderEvent();
		public event MissionSenderEvent OnFinished;	

		public bool IsBusy
		{
			get { return isBusy; }
		}

		public Vehicle Target
		{
			get { return target; }
			set { target = value; }
		}

        public ushort WaypointCount
        {
            get { return waypointCount; }
            set { waypointCount = value; }
        }

        public ObservableCollection<MissionItem> MissionList
        {
            get { return missionList; }
            set { missionList = value; }
        }

        public MissionManager(Vehicle v)
		{
			target = v;
			sendPackages = new List<LinkPackage>();
			receivedPackage = new List<LinkPackage>();
            missionList = new ObservableCollection<MissionItem>();

			isBusy = false;
			state = State.Idle;
		}

		public bool StartSendMission()
		{
			if (isBusy)
				return false;
			if(target.Link==null)			
				return false;
			
			isBusy = true;
			state = State.Sending;

			sendPackages.Clear();	

			missionList[0].GenerateMissionItems(target, sendPackages);

			MAVLinkPackage p1 = new MAVLinkPackage((byte)MAVLINK_MSG_ID.MISSION_COUNT, target.Link);
			p1.System = 255;
			p1.Component = 0;
			p1.AddData((ushort)sendPackages.Count);
			p1.AddData((byte)target.ID);
			p1.AddData((byte)0);
			p1.SetVerify();			
			target.Link.SendPackage(p1);

			return true;
		}

		public bool StartReceiveMission()
		{
			if (isBusy)
				return false;
			if (target.Link == null)
				return false;

			//isBusy = true;
			//state = State.Receiving;

			WaypointCount = 0;
			currentWaypoint = 0;
			receivedPackage.Clear();

			MAVLinkPackage p = new MAVLinkPackage((byte)MAVLINK_MSG_ID.MISSION_REQUEST_LIST, target.Link);
			p.AddData((byte)target.ID);//sys
			p.AddData((byte)190);//comp
			p.SetVerify();
			target.Link.SendPackage(p);	

			return true;
		}

		public void SendMissionRequest()
		{
			if (currentWaypoint == WaypointCount)
				return;
			MAVLinkPackage p = new MAVLinkPackage((byte)MAVLINK_MSG_ID.MISSION_REQUEST, target.Link);
			p.AddData(currentWaypoint);
			p.AddData((byte)target.ID);//sys
			p.AddData((byte)190);//comp			
			p.SetVerify();
			currentWaypoint++;
			Debug.WriteLine("[Mavlink]Request {0} of {1} waypoint.", currentWaypoint, waypointCount);
			target.Link.SendPackage(p);
		}

		public bool AddMissionItem(LinkPackage p1)
		{
			receivedPackage.Add(p1.Clone());
			if (receivedPackage.Count==WaypointCount)
			{
				return true;				
			}
			else
			{
				SendMissionRequest();
				return false;
			}
		}
		public void UnpackMission()
		{
			Waypoint currwp = null;
			MAVLinkPackage p = new MAVLinkPackage((byte)MAVLINK_MSG_ID.MISSION_ACK, target.Link);
			p.AddData((byte)target.ID);//sys
			p.AddData((byte)190);//comp
			p.AddData((byte)0);
			p.SetVerify();
			target.Link.SendPackage(p);
			((Mission)missionList[0]).Clear();
			for (int i = 0; i < waypointCount; i++)
			{
				float p1 = receivedPackage[i].NextSingle();
				float p2 = receivedPackage[i].NextSingle();
				float p3 = receivedPackage[i].NextSingle();
				float p4 = receivedPackage[i].NextSingle();
				float p5 = receivedPackage[i].NextSingle();
				float p6 = receivedPackage[i].NextSingle();
				float p7 = receivedPackage[i].NextSingle();
				ushort seq = receivedPackage[i].NextUShort();
				ushort cmd = receivedPackage[i].NextUShort();
				receivedPackage[i].NextShort();//skip tgt_sys & tgt_comp
				MAVLink.MAV_FRAME frame = (MAVLink.MAV_FRAME)receivedPackage[i].NextByte();

				if (cmd == 16)
				{
					currwp = new Waypoint(0);
					currwp.Position = PositionHelper.WGS84ToGCJ02(new GMap.NET.PointLatLng(p5, p6));
					currwp.Altitude = p7;
					if (frame == MAVLink.MAV_FRAME.GLOBAL_RELATIVE_ALT)
						currwp.UseRelativeAlt = true;
					else
						currwp.UseRelativeAlt = false;
					((Mission)missionList[0]).AddWaypoint(currwp);					
				}
				if(cmd==178)
				{
					if(currwp!=null)
					{
						currwp.ChangeSpeed = true;
						currwp.Speed = p2;
					}
				}
			}

			OnFinished?.Invoke();
			isBusy = false;
			state = State.Idle;
		}
		public void HandleMissionRequest(int seq)
		{
			if (state != State.Sending)
				return;
			if (seq >= sendPackages.Count)
				return;
			target.Link.SendPackage(sendPackages[seq]);
		}

		public void HandleMissionAck()
		{
			if (state == State.Sending)
				SetCurrentWaypoint(0);
			isBusy = false;
			state = State.Idle;
			OnFinished?.Invoke();
		}

		public void SetCurrentWaypoint(int id)
		{
			MAVLinkPackage p = new MAVLinkPackage((byte)MAVLINK_MSG_ID.MISSION_SET_CURRENT, target.Link);
			p.System = 255;
			p.Component = 0;
			p.AddData((ushort)id);
			p.AddData(target.ID);
			p.AddData((byte)0);
			p.SetVerify();
			target.Link.SendPackage(p);
		}
		
		enum State
		{
			Idle,
			Sending,
			Receiving
		}
	}
}
