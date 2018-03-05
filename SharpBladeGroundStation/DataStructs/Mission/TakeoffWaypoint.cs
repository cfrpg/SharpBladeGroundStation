using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GMap.NET;

namespace SharpBladeGroundStation.DataStructs
{
    /// <summary>
    /// 起飞航点的类
    /// </summary>
    public class TakeoffWaypoint:WaypointBase
    {
        public TakeoffWaypoint(int i, PointLatLng pos, float alt) : base(i, pos, alt) { }
    }
}
