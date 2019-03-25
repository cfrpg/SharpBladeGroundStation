using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using GMap.NET.WindowsPresentation;
using SharpBladeGroundStation.Map.Markers;

namespace SharpBladeGroundStation.DataStructs
{
    /// <summary>
    /// 描述飞行任务的类
    /// </summary>
    public class Mission:MissionItem
    {
        string name;
		Color color;
		MapRouteData route;

        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                NotifyPropertyChanged("Name");
            }
        }

		public Color Color
		{
			get { return color; }
			set
			{
				color = value;
				route.Color = color;				
				NotifyPropertyChanged("Color");
			}
		}

		public MapRouteData Route
		{
			get { return route; }
			set { route = value; }
		}

		public Mission(MapRouteData r)
		{
			route = r;
		}

		/// <summary>
		/// 在最后插入航点
		/// </summary>
		/// <param name="wp">要插入的航点</param>
		public void AddWaypoint(WaypointBase wp)
		{
			GMapMarker m = new GMapMarker(wp.Position);
			WayPointMarker wpm = new WayPointMarker(route, m, (childItems.Count + 1).ToString());
			route.AddWaypoint(wpm, m);
			AddWaypoint(childItems.Count, wp);
		}



		/// <summary>
		/// 在指定位置插入航点
		/// </summary>
		/// <param name="id">要插入的位置</param>
		/// <param name="wp">要插入的航点</param>
		public void AddWaypoint(int id,WaypointBase wp)
		{
			insertMissionItem(wp, id);
			rebuildID(0);
		}
	}
}
