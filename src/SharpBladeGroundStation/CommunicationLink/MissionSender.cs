using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using SharpBladeGroundStation.DataStructs;
using SharpBladeGroundStation.Map.Markers;
using System.MAVLink;
using System.Diagnostics;

namespace SharpBladeGroundStation.CommunicationLink
{
	public class MissionSender
	{
		MissionSenderState state;
		List<MAVLinkPackage> packages;
		Vehicle target;
		ushort nextRequest;

		Thread background;

		public delegate void MissionSenderEvent();
		public event MissionSenderEvent OnFinished;

		public MissionSenderState State
		{
			get { return state; }
			set { state = value; }
		}

		public List<MAVLinkPackage> Packages
		{
			get { return packages; }
			set { packages = value; }
		}

		public Vehicle Target
		{
			get { return target; }
			set { target = value; }
		}

		public ushort NextRequest
		{
			get { return nextRequest; }
			set { nextRequest = value; }
		}

		public MissionSender(Vehicle t)
		{
			target = t;
			state = MissionSenderState.Idle;
			packages = new List<MAVLinkPackage>();
			background = new Thread(backgroundWorker);
			background.IsBackground = true;
			//background.Start();
		}

		void backgroundWorker()
		{
			bool flag = false;
			while (!flag)
			{
				switch (state)
				{
					case MissionSenderState.Idle:
						//flag = true;
						break;
					case MissionSenderState.Configing:
						//flag = true;
						break;
					case MissionSenderState.Sending:
						break;
					case MissionSenderState.Waiting:
						if (nextRequest < 32768)
						{
							Debug.WriteLine("[Mission Sender]:Send {0} of {1}", nextRequest, packages.Count);
							//target.Link.SendPackageQueue.Enqueue(packages[nextRequest]);
							target.Link.SendPackage(packages[nextRequest]);
							nextRequest = 32768;
						}
						else
						{
							if (nextRequest == 32769)
								state = MissionSenderState.Finished;
						}
						break;
					case MissionSenderState.Finished:
						//flag = true;
						OnFinished?.Invoke();
						break;
					default:
						break;
				}
			}
		}

		public void StartSendMission(MapRouteData data)
		{
			state = MissionSenderState.Configing;
			packages.Clear();
			for (int i = 0; i < data.Markers.Count; i++)
			{
				MAVLinkPackage p = new MAVLinkPackage((byte)MAVLINK_MSG_ID.MISSION_ITEM, target.Link);
				p.AddData(0f);  //p1
				p.AddData(0.5f);
				p.AddData(0f);
				p.AddData(float.NaN);
				p.AddData((float)data.Markers[i].Position.Lat);
				p.AddData((float)data.Markers[i].Position.Lng);
				p.AddData((float)data.Markers[i].Altitude); //p7

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
				packages.Add(p);
			}
			MAVLinkPackage p1 = new MAVLinkPackage((byte)MAVLINK_MSG_ID.MISSION_COUNT, target.Link);
			p1.System = 255;
			p1.Component = 0;			
			p1.AddData((ushort)packages.Count);
			p1.AddData((byte)target.ID);
			p1.AddData((byte)190);
			p1.SetVerify();

			nextRequest = 32768;
			target.Link.SendPackage(p1);
			state = MissionSenderState.Waiting;

		}
	}

	public enum MissionSenderState
	{
		/// <summary>
		/// 空闲
		/// </summary>
		Idle = 0,
		/// <summary>
		/// 先占坑，配置参数
		/// </summary>
		Configing,
		/// <summary>
		/// 正在一个发送阶段
		/// </summary>
		Sending,
		/// <summary>
		/// 正在一个等待阶段
		/// </summary>
		Waiting,
		/// <summary>
		/// 发送完毕
		/// </summary>
		Finished
	}
}
