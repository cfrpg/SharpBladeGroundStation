using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpBladeGroundStation.DataStructs;
using GMap.NET;

namespace SharpBladeGroundStation
{
	public class CourseInfo
	{
		WayPoint currentWayPoint;
		WayPoint prevWayPoint;
		WayPoint nextWayPoint;

		public WayPoint CurrentWayPoint
		{
			get { return currentWayPoint; }
			set { currentWayPoint = value; }
		}

		public WayPoint PrevWayPoint
		{
			get { return prevWayPoint; }
			set { prevWayPoint = value; }
		}

		public WayPoint NextWayPoint
		{
			get { return nextWayPoint; }
			set { nextWayPoint = value; }
		}

		public CourseInfo()
		{
			currentWayPoint = new WayPoint();
			prevWayPoint = new WayPoint();
			nextWayPoint = new WayPoint();
		}

		public void SetInitWayPoint(WayPoint cur,WayPoint next)
		{
			currentWayPoint = cur.Clone();
			nextWayPoint = next.Clone();
		}

		public void StepForwardWayPoint(WayPoint next)
		{
			prevWayPoint = currentWayPoint;
			currentWayPoint = nextWayPoint;
			nextWayPoint = next.Clone();
		}

		public string GetString(PointLatLng pos,float h)
		{
			string str = "";
			str += string.Format("上一航点:");
			if(prevWayPoint.ID!=-1)
			{
				str += string.Format("经:{0:000.0000000} 纬:{1:00.0000000} 高度:{2:N1}",prevWayPoint.Position.Lng,prevWayPoint.Position.Lat,prevWayPoint.Height);
			}
			str += Environment.NewLine;
			str += string.Format("当前位置:");			
			str+= string.Format("经:{0:000.0000000} 纬:{1:00.0000000} 高度:{2:N1}",pos.Lng,pos.Lat,h);

			str += Environment.NewLine;
			str += string.Format("下一航点:");
			if (nextWayPoint.ID != -1)
			{
				str += string.Format("经:{0:000.0000000} 纬:{1:00.0000000} 高度:{2:N1}", nextWayPoint.Position.Lng, nextWayPoint.Position.Lat, nextWayPoint.Height);
			}
			return str;
		}

		public void Clear()
		{
			prevWayPoint.ID = -1;
			currentWayPoint.ID = -1;
			nextWayPoint.ID = -1;
		}
			

	}
}
