using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GMap.NET;

namespace SharpBladeGroundStation.DataStructs
{
    /// <summary>
    /// 降落的航点
    /// </summary>
    public class LandWaypoint:WaypointBase
    {
        public LandWaypoint(int i, PointLatLng pos, float alt) : base(i, pos, alt) { }
    }
}
