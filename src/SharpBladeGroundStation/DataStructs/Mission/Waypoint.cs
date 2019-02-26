using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GMap.NET;

namespace SharpBladeGroundStation.DataStructs
{
    /// <summary>
    /// 描述普通航点的类
    /// </summary>
    public class Waypoint:WaypointBase
    {
        float holdTime;
        float radius;

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

        public Waypoint(int i) : this(i,new PointLatLng(0, 0), 0) { }
       
        public Waypoint(int i,PointLatLng pos,float alt):base(i,pos,alt)
        {
            holdTime = 0;
            heading = float.NaN;
            radius = 0;
        }
    }
}
    