using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GMap.NET;
using GMap.NET.WindowsPresentation;
using SharpBladeGroundStation.CommunicationLink;
using SharpBladeGroundStation.Map;
using SharpBladeGroundStation.Map.Markers;
using System.MAVLink;

namespace SharpBladeGroundStation.DataStructs
{
    /// <summary>
    /// 描述普通航点的类
    /// </summary>
    public class Waypoint:WaypointBase
    {
        float holdTime;
        float radius;
		bool changeSpeed;
		float speed;

        public float HoldTime
        {
            get { return holdTime; }
            set
            {
                holdTime = value;
                NotifyPropertyChanged("HoldTime");
            }
        }

        public float Radius
        {
            get { return radius; }
            set {
                radius = value;
                NotifyPropertyChanged("Radius");
            }
        }

		public bool ChangeSpeed
		{
			get { return changeSpeed; }
			set
			{
				changeSpeed = value;
				NotifyPropertyChanged("ChangeSpeed");
			}
		}

		public float Speed
		{
			get { return speed; }
			set
			{
				speed = value;
				NotifyPropertyChanged("Speed");
			}
		}

		public Waypoint(int i) : this(i,new PointLatLng(0, 0), 0) { }
       
        public Waypoint(int i,PointLatLng pos,float alt):base(i,pos,alt)
        {
            holdTime = 0;
            heading = float.NaN;
            radius = 0;
			speed = 10;
			changeSpeed = false;
        }
		                  
		protected override int rebuildID(int pos)
		{
			marker.MarkerText = pos.ToString();
			return base.rebuildID(pos);
		}

		public override void GenerateMissionItems(Vehicle v, List<LinkPackage> packageList)
		{
			MAVLinkPackage p = new MAVLinkPackage((byte)MAVLINK_MSG_ID.MISSION_ITEM, v.Link);
			var wp = PositionHelper.GCJ02ToWGS84(position);
			p.AddData(0f);  //p1
			p.AddData(0.5f);
			p.AddData(0f);
			p.AddData(float.NaN);
			p.AddData((float)wp.Lat);
			p.AddData((float)wp.Lng);
			p.AddData((float)altitude); //p7

			p.AddData((ushort)packageList.Count);//seq
			p.AddData((ushort)16);//cmd
			p.AddData((byte)v.ID);//sys
			p.AddData((byte)190);//comp
			if(useRelativeAlt)
				p.AddData((byte)3);//frame
			else
				p.AddData((byte)0);//frame
			if (id == 0)
				p.AddData((byte)1);//current
			else
				p.AddData((byte)0);//current
			p.AddData((byte)1);//auto
			p.Sequence = (byte)(packageList.Count);
			p.SetVerify();
			packageList.Add(p);

			if(changeSpeed)
			{
				p = new MAVLinkPackage((byte)MAVLINK_MSG_ID.MISSION_ITEM, v.Link);
				if(v.Type==MAVLink.MAV_TYPE.FIXED_WING)
					p.AddData(0f);  //p1
				else
					p.AddData(1f);  //p1
				p.AddData(speed);
				p.AddData(-1f);
				p.AddData(0);
				p.AddData(0);
				p.AddData(0);
				p.AddData(0); //p7

				p.AddData((ushort)packageList.Count);//seq
				p.AddData((ushort)178);//cmd
				p.AddData((byte)v.ID);//sys
				p.AddData((byte)190);//comp
				if (useRelativeAlt)
					p.AddData((byte)3);//frame
				else
					p.AddData((byte)0);//frame
				p.AddData((byte)0);//current
				p.AddData((byte)1);//auto
				p.Sequence = (byte)(packageList.Count);
				p.SetVerify();
			}
		}
	}
}
    