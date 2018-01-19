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
    public class MarkedMapRouteData : MapRouteData
    {
        List<GMapElement> markers;

        public MouseButtonEventHandler LeftMouseButtonUp;
        public MouseButtonEventHandler LeftMouseButtonDown;
        public MouseButtonEventHandler RightMouseButtonUp;
        public MouseButtonEventHandler RightMouseButtonDown;

        /// <summary>
        /// 获取路径点的标记的集合
        /// </summary>
        public List<GMapElement> Markers
        {
            get { return markers; }
            //set { markers = value; }
        }

        public MarkedMapRouteData(GMapControl m, int z) : base(m, z)
        {
            markers = new List<GMapElement>();
        }

        public void InsertWaypoint(int id, GMapElement shape, GMapMarker m)
        {
            m.Shape = shape;
            m.ZIndex = zindex + 1;
            markers.Insert(id, shape);
            map.Markers.Add(m);

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
}
