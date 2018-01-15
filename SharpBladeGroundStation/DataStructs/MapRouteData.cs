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
        List<WayPointMarker> markers;


        public List<PointLatLng> Points
        {
            get { return points; }
            set { points = value; }
        }

        public List<WayPointMarker> Markers
        {
            get { return markers; }
            set { markers = value; }
        }
    }
}
