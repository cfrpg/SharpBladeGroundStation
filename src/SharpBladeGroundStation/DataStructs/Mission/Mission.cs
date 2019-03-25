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
			WaypointMarker wpm = new WaypointMarker(route, m, "");
			route.AddWaypoint(wpm, m);
			wp.Marker = wpm;
			AddWaypoint(99999, wp);
		}

		/// <summary>
		/// 在指定节点前分裂出一个紧挨的节点
		/// </summary>
		/// <param name="wp"></param>
		public void SplitWaypoint(WaypointBase wp)
		{
			GMapMarker m = new GMapMarker(new GMap.NET.PointLatLng(wp.Position.Lat - 0.0001, wp.Position.Lng - 0.0001));
			WaypointMarker wpm = new WaypointMarker(route, m, "");
			Waypoint newwp = new Waypoint(0, m.Position, wp.Altitude);
			route.AddWaypoint(wp.Marker, wpm, m, wp.Altitude);
			newwp.Marker = wpm;

			AddWaypoint(wp.ID, newwp);
			return;


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

		/// <summary>
		/// 更新航点的坐标
		/// </summary>
		/// <param name="wpm">要更新的航点对应的Marker</param>
		public void UpdateWaypointPosition(WaypointMarker wpm)
		{
			for(int i=0;i<childItems.Count;i++)
			{
				if(childItems[i] is WaypointBase)
				{
					if (((WaypointBase)childItems[i]).Marker==wpm)
					{
						((WaypointBase)childItems[i]).Position = wpm.Position;
						break;
					}
				}
			}
		}

		/// <summary>
		/// 更新航点的高度
		/// </summary>
		/// <param name="wpm">要更新的航点对应的Marker</param>
		public void UpdateWaypointAltitude(WaypointMarker wpm)
		{
			for (int i = 0; i < childItems.Count; i++)
			{
				if (childItems[i] is WaypointBase)
				{
					if (((WaypointBase)childItems[i]).Marker == wpm)
					{
						((WaypointBase)childItems[i]).Altitude = wpm.Altitude;
						break;
					}
				}
			}
		}

		public void RemoveWaypoint(WaypointMarker wpm)
		{
			for (int i = 0; i < childItems.Count; i++)
			{
				if (childItems[i] is WaypointBase)
				{
					if (((WaypointBase)childItems[i]).Marker == wpm)
					{
						RemoveWaypointAt(childItems[i].ID);
						break;
					}
				}
			}
			route.RemoveWaypoint(wpm);
		}

		public void RemoveWaypointAt(int id)
		{
			removeMissionItemAt(id);
			rebuildID(0);
		}
	}
}
