using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GMap.NET;

namespace SharpBladeGroundStation.DataStructs
{
    public class Waypoint:WaypointBase
    {
        float holdTime;
        float heading;
        float radius;

        public float HoldTime
        {
            get { return holdTime; }
            set { holdTime = value; }
        }

        public float Heading
        {
            get { return heading; }
            set { heading = value; }
        }

        public float Radius
        {
            get { return radius; }
            set { radius = value; }
        }

        public Waypoint() : this(new PointLatLng(0, 0), 0) { }
       
        public Waypoint(PointLatLng pos,float alt):base(pos,alt)
        {
            holdTime = 0;
            heading = float.NaN;
            radius = 0;
        }
    }
}
