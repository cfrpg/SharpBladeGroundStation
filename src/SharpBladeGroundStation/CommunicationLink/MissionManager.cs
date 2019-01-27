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
		MapRouteData localMission;
		MapRouteData remoteMission;
		Vehicle target;
		
		List<MAVLinkPackage> sendPackages;
		List<LinkPackage> receivedPackage;

		bool isBusy;
		State state;
		ushort waypointCount;
		ushort currentWaypoint;

		public delegate void MissionSenderEvent();
		public event MissionSenderEvent OnFinished;
		public MapRouteData LocalMission
		{
			get { return localMission; }
			set { localMission = value; }
		}

		public MapRouteData RemoteMission
		{
			get { return remoteMission; }
			set { remoteMission = value; }
		}

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
			get
			{
				return waypointCount;
			}

			set
			{
				waypointCount = value;
			}
		}

		public MissionManager(Vehicle v)
		{
			target = v;
			sendPackages = new List<MAVLinkPackage>();
			receivedPackage = new List<LinkPackage>();

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
			for (int i = 0; i < localMission.Markers.Count; i++)
			{
				MAVLinkPackage p = new MAVLinkPackage((byte)MAVLINK_MSG_ID.MISSION_ITEM, target.Link);
                
                var wp= PositionHelper.GCJ02ToWGS84(localMission.Markers[i].Position);
				p.AddData(0f);  //p1
				p.AddData(0.5f);
				p.AddData(0f);
				p.AddData(float.NaN);
				p.AddData((float)wp.Lat);
				p.AddData((float)wp.Lng);
				p.AddData((float)localMission.Markers[i].Altitude); //p7

				p.AddData((ushort)i);//seq
				p.AddData((ushort)16);//cmd
				p.AddData((byte)target.ID);//sys
				p.AddData((byte)190);//comp

				p.AddData((byte)3);//frame
				if (i == 0)
					p.AddData((byte)1);//current
				else
					p.AddData((byte)0);//current
				p.AddData((byte)1);//auto			

				p.Sequence = (byte)i;
				p.SetVerify();
				sendPackages.Add(p);
			}
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
				//MAVLinkPackage p = new MAVLinkPackage((byte)MAVLINK_MSG_ID.MISSION_ACK, target.Link);
				//p.AddData((byte)target.ID);//sys
				//p.AddData((byte)190);//comp
				//p.AddData((byte)0);
				//p.SetVerify();
				//target.Link.SendPackage(p);
				//OnFinished?.Invoke();
				//isBusy = false;
				//state = State.Idle;
			}
			else
			{
				SendMissionRequest();
				return false;
			}
		}
		public void UnpackMission()
		{
			MAVLinkPackage p = new MAVLinkPackage((byte)MAVLINK_MSG_ID.MISSION_ACK, target.Link);
			p.AddData((byte)target.ID);//sys
			p.AddData((byte)190);//comp
			p.AddData((byte)0);
			p.SetVerify();
			target.Link.SendPackage(p);
			localMission.Clear();
			for(int i=0;i<waypointCount;i++)
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
				if(cmd==16)
				{				
					GMapMarker m = new GMapMarker(PositionHelper.WGS84ToGCJ02(new GMap.NET.PointLatLng(p5,p6)));
					WayPointMarker wp = new WayPointMarker(localMission, m, (localMission.Markers.Count + 1).ToString(), string.Format("Lat {0}\nLon {1}\nAlt {2} m", m.Position.Lat, m.Position.Lng, p7));
					localMission.AddWaypoint(wp, m, p7);
					
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
			isBusy = false;
			state = State.Idle;
			OnFinished?.Invoke();
		}

		enum State
		{
			Idle,
			Sending,
			Receiving
		}
	}
}
