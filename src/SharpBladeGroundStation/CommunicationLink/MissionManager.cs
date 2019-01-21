using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using SharpBladeGroundStation.DataStructs;
using SharpBladeGroundStation.Map.Markers;
using System.MAVLink;


namespace SharpBladeGroundStation.CommunicationLink
{
	public class MissionManager
	{
		MapRouteData localMission;
		MapRouteData remoteMission;
		Vehicle target;
		
		List<MAVLinkPackage> sendPackages;
		List<MAVLinkPackage> receivePackages;

		bool isBusy;
		State state;

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

		public MissionManager(Vehicle v)
		{
			target = v;
			sendPackages = new List<MAVLinkPackage>();
			receivePackages = new List<MAVLinkPackage>();

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
				p.AddData(0f);  //p1
				p.AddData(0.5f);
				p.AddData(0f);
				p.AddData(float.NaN);
				p.AddData((float)localMission.Markers[i].Position.Lat);
				p.AddData((float)localMission.Markers[i].Position.Lng);
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
