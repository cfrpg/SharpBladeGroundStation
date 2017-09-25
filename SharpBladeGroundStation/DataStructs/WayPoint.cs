using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GMap.NET;

namespace SharpBladeGroundStation.DataStructs
{
	public class WayPoint
	{
		PointLatLng position;
		float height;
		float heading;
		int id;

		public PointLatLng Position
		{
			get { return position; }
			set { position = value; }
		}

		public float Height
		{
			get { return height; }
			set { height = value; }
		}

		public float Heading
		{
			get { return heading; }
			set { heading = value; }
		}

		public int ID
		{
			get { return id; }
			set { id = value; }
		}

		public WayPoint(int i,PointLatLng pos,float h,float hdg)
		{
			id = i;
			position = pos;
			height = h;
			heading = hdg;
		}

		public WayPoint(int i, double lat,double lon,float h,float hdg):this(i,new PointLatLng(lat,lon),h,hdg)
		{
			
		}

		public WayPoint():this(-1,0,0,0,0)
		{

		}

		public WayPoint Clone()
		{
			return new WayPoint(id, position, height, heading);
		}
	}
}
