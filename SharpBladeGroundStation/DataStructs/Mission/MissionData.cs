using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpBladeGroundStation.DataStructs
{
    /// <summary>
    /// 储存任务数据的类
    /// </summary>
    public class MissionData
    {
        List<WaypointBase> waypoints;

        /// <summary>
        /// 获取第i个航点,或将航点插入到第i个航点之前
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public WaypointBase this[int id]
        {
            get
            {
                return waypoints[id];
            }
        }
    }
}
