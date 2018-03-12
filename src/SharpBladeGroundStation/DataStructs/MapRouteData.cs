using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using GMap.NET;
using GMap.NET.WindowsPresentation;
using SharpBladeGroundStation.Map;
using SharpBladeGroundStation.Map.Markers;
using SharpBladeGroundStation.Configuration;

namespace SharpBladeGroundStation.DataStructs
{
    /// <summary>
    /// 描述地图上路线的类
    /// </summary>
    public class MapRouteData
    {
        List<PointLatLng> points;
        List<GMapElement> markers;
        GMapControl map;
        GMapRoute route;
        bool wayPointMarkerEnabled;
        bool clickable;
        int zindex;
        int maxPointNumber;
		Color color;

        public MouseButtonEventHandler LeftMouseButtonUp;
        public MouseButtonEventHandler LeftMouseButtonDown;
        public MouseButtonEventHandler RightMouseButtonUp;
        public MouseButtonEventHandler RightMouseButtonDown;

        /// <summary>
        /// 获取路径包含的点的集合
        /// </summary>
        public List<PointLatLng> Points
        {
            get { return points; }
            //set { points = value; }
        }

        /// <summary>
        /// 获取路径点的标记的集合
        /// </summary>
        public List<GMapElement> Markers
        {
            get { return markers; }
            //set { markers = value; }
        }

        /// <summary>
        /// 获取路径所在的地图控件
        /// </summary>
        public GMapControl Map
        {
            get { return map; }
            //set { map = value; }
        }

        /// <summary>
        /// 获取路径对象
        /// </summary>
        public GMapRoute Route
        {
            get { return route; }
            //set { route = value; }
        }

        /// <summary>
        /// 获取或设置是否显示路径点的标记
        /// </summary>
        public bool WayPointMarkerEnabled
        {
            get { return wayPointMarkerEnabled; }
            set { wayPointMarkerEnabled = value; }
        }

        /// <summary>
        /// 是否相应鼠标事件
        /// </summary>
        public bool Clickable
        {
            get { return clickable; }
            //set { clickable = value; }
        }

        public int ZIndex
        {
            get { return zindex; }
            set { zindex = value; }
        }

		/// <summary>
		/// 路径中最大航点数量
		/// </summary>
        public int MaxPointNumber
        {
            get { return maxPointNumber; }
            set
            {
                maxPointNumber = value;
                if(points.Count>maxPointNumber)
                {
					RemoveRange(0, points.Count - maxPointNumber);                   
                }
            }
        }

		public Color Color
		{
			get { return color; }
			set
			{
				color = value;
				regeneRoute();
			}
		}

		/// <summary>
		/// 构造函数
		/// </summary>
		/// <param name="m">所在的地图控件</param>
		/// <param name="wpenabled">启用航点标记</param>
		/// <param name="ca">是否响应鼠标事件</param>
		/// <param name="z">深度</param>
		public MapRouteData(GMapControl m, int z, bool wpenabled,bool ca)
        {
            map = m;
            wayPointMarkerEnabled = wpenabled;
            clickable = ca;
            points = new List<PointLatLng>();
            markers = new List<GMapElement>();
            zindex = z;
            route = new GMapRoute(points);
            map.Markers.Add(route);
            maxPointNumber = int.MaxValue;
			color = Colors.Red;
        }
        /// <summary>
        /// 无航点,无点击事件的构造函数
        /// </summary>
        /// <param name="m">所在的地图控件</param>
        /// <param name="z">深度</param>
        public MapRouteData(GMapControl m, int z) : this(m, z, false, false) { }

        /// <summary>
        /// 无航点,无点击事件构造函数
        /// </summary>
        /// <param name="m">所在的地图控件</param>
        public MapRouteData(GMapControl m) : this(m, 10000) { }

        public void AddWaypoint(GMapElement shape,GMapMarker m)
        {
            InsertWaypoint(points.Count,  shape,m);
        }

        public void AddWaypoint(PointLatLng pos)
        {
            if (wayPointMarkerEnabled || clickable)
            {
                throw new Exception("This method cannnot add waypoint marker,use AddWaypoint(PointLatLng,UIElement,string,string) instead.");
            }
            points.Add(pos);
			if (points.Count > maxPointNumber)
			{
				RemoveRange(0, points.Count - maxPointNumber);
			}
			regeneRoute();
            
        }

        public void InsertWaypoint(int id, GMapElement shape,GMapMarker m)
        {
            if (WayPointMarkerEnabled)
            {
				m.Shape = shape;
                m.ZIndex = zindex + 1;
                markers.Insert(id,shape);
                map.Markers.Add(m);
                if (clickable)
                {
                    if (LeftMouseButtonDown != null)
                        shape.MouseLeftButtonDown += LeftMouseButtonDown;
                    if (LeftMouseButtonUp != null)
                        shape.MouseLeftButtonUp += LeftMouseButtonUp;
                    if (RightMouseButtonDown != null)
                        shape.MouseRightButtonDown += RightMouseButtonDown;
                    if (RightMouseButtonUp != null)
                        shape.MouseRightButtonUp += RightMouseButtonUp;
                }
            }
            points.Insert(id,m.Position);
			if (points.Count > maxPointNumber)
			{
				RemoveRange(0, points.Count - maxPointNumber);
			}
			regeneRoute();
        }
			
		public void RemoveWayPoint(GMapElement wp)
		{
			for (int i = 0; i < markers.Count; i++)
			{
				if (wp == markers[i])
				{
					points.RemoveAt(i);
					markers.RemoveAt(i);					
					break;
				}
			}
			map.Markers.Remove(wp.Marker);
			regeneRoute();
		}

		public void RemoveRange(int id,int num)
		{
			if (wayPointMarkerEnabled)
			{
				for (int i = 0; i < num; i++)
				{
					map.Markers.Remove(markers[i+id].Marker);
				}
			}
			markers.RemoveRange(id, num);
			points.RemoveRange(id, num);
			regeneRoute();
		}

		public void RefreshWayPoint(GMapElement wp)
		{
			for(int i=0;i<markers.Count;i++)
			{
				if(wp==markers[i])
				{
					points[i] = wp.Position;
					break;
				}
			}
			regeneRoute();
		}	

		public void Clear()
		{
			foreach(GMapElement e in markers)
			{
				map.Markers.Remove(e.Marker);
			}
			markers.Clear();
			points.Clear();
			regeneRoute();
		}
        private void regeneRoute()
        {
            map.Markers.Remove(route);
            route = new GMapRoute(points);
            route.RegenerateShape(map);
            if (route.Shape != null)
            {
                ((System.Windows.Shapes.Path)route.Shape).Stroke = new SolidColorBrush(color);
                ((System.Windows.Shapes.Path)route.Shape).StrokeThickness = 4;
                ((System.Windows.Shapes.Path)route.Shape).Opacity = 1;
                ((System.Windows.Shapes.Path)route.Shape).Effect = null;
            }
			map.Markers.Add(route);
        }
    }
}
