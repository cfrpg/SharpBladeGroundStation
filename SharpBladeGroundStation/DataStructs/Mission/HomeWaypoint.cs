using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GMap.NET;

namespace SharpBladeGroundStation.DataStructs
{
    /// <summary>
    /// 起始点
    /// </summary>
    public class HomeWaypoint:WaypointBase
    {
        public HomeWaypoint(int i, PointLatLng pos, float alt):base(i,pos,alt) { }
    }
}
