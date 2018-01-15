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

namespace SharpBladeGroundStation
{
    public partial class MainWindow : Window
    {
        //GMap
        GMapMarker uavMarker;
        MapCenterPositionConfig mapCenterConfig = MapCenterPositionConfig.Free;
        GMapRoute mapRoute;
        List<PointLatLng> waypointPosition;

        WayPointMarker wp;
        List<WayPointMarker> waypointMarkers;

        GMapRoute flightRoute;
        List<PointLatLng> flightRoutePoints;

        private void initGmap()
        {
            gmap.Zoom = 3;
            gmap.MapProvider = AMapHybirdProvider.Instance;
            //gmap.MapProvider = GMap.NET.MapProviders.BingHybridMapProvider.Instance;

            gmap.Position = new PointLatLng(34.242947, 108.916225);
            gmap.Zoom = 18;
            System.Net.NetworkInformation.Ping ping = new System.Net.NetworkInformation.Ping();
            try
            {
                if (ping.Send("www.autonavi.com").Status != System.Net.NetworkInformation.IPStatus.Success)
                {
                    GMaps.Instance.Mode = AccessMode.CacheOnly;
                }
                else
                {
                    GMaps.Instance.Mode = AccessMode.ServerAndCache;
                }
            }
            catch
            {
                GMaps.Instance.Mode = AccessMode.CacheOnly;
            }
            uavMarker = new GMapMarker(gmap.Position);
            uavMarker.Shape = new UAVMarker();
            uavMarker.Offset = new Point(-15, -15);
            uavMarker.ZIndex = 100000;
            waypointPosition = new List<PointLatLng>();
            mapRoute = new GMapRoute(waypointPosition);

            flightRoutePoints = new List<PointLatLng>();
            flightRoute = new GMapRoute(flightRoutePoints);

            gmap.Markers.Add(uavMarker);
            gmap.Markers.Add(mapRoute);
            gmap.Markers.Add(flightRoute);

            gmap.MouseLeftButtonDown += Gmap_MouseLeftButtonDown;
            gmap.MouseLeftButtonUp += Gmap_MouseLeftButtonUp;
            waypointMarkers = new List<WayPointMarker>();
        }

        private void Gmap_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            RectLatLng area = gmap.SelectedArea;
            if (area.IsEmpty && gmap.Position == positionWhenTouch)
            {
                Point p = e.GetPosition(gmap);
                GMapMarker m = new GMapMarker(gmap.FromLocalToLatLng((int)(p.X), (int)(p.Y)));
                wp = new WayPointMarker(this, m, (waypointMarkers.Count + 1).ToString(), string.Format("Waypoint {0}\nLat {1}\nLon {2}\n", waypointMarkers.Count + 1, m.Position.Lat, m.Position.Lng));
                m.Shape = wp;
                m.ZIndex = 1000;
                waypointMarkers.Add(wp);
                gmap.Markers.Add(m);
                reGeneRoute();

                wp.MouseRightButtonDown += Wp_MouseRightButtonDown;
                wp.MouseLeftButtonUp += Wp_MouseLeftButtonUp;
            }
            if (!area.IsEmpty)
            {
                if (MessageBox.Show("缓存选定区域地图？", "SharpBladeGroundStation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    if (MessageBox.Show("清除原有缓存？", "SharpBladeGroundStation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        gmap.Manager.PrimaryCache.DeleteOlderThan(DateTime.Now, null);
                    }
                    AccessMode m = GMaps.Instance.Mode;
                    GMaps.Instance.Mode = AccessMode.ServerAndCache;
                    for (int i = (int)gmap.Zoom; i <= gmap.MaxZoom; i++)
                    {
                        TilePrefetcher tp = new TilePrefetcher();
                        tp.Owner = this;
                        tp.ShowCompleteMessage = true;
                        tp.Start(area, i, gmap.MapProvider, 100);
                    }
                    GMaps.Instance.Mode = m;

                }
                gmap.SelectedArea = new RectLatLng();
            }
        }

        private void Gmap_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            positionWhenTouch = gmap.Position;
        }

        private void Wp_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            reGeneRoute();
            e.Handled = true;
        }

        private void Wp_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {

            WayPointMarker wp = sender as WayPointMarker;
            waypointMarkers.Remove(wp);
            gmap.Markers.Remove(wp.Marker);
            for (int i = 0; i < waypointMarkers.Count; i++)
            {
                waypointMarkers[i].MarkerText = (i + 1).ToString();
            }
            reGeneRoute();
            e.Handled = true;
        }
        private void reGeneRoute()
        {
            List<PointLatLng> points = new List<PointLatLng>();
            foreach (var v in waypointMarkers)
            {
                points.Add(v.Marker.Position);
            }
            gmap.Markers.Remove(mapRoute);
            mapRoute = new GMapRoute(points);
            setRouteColor();

            gmap.Markers.Add(mapRoute);
        }
        private void setRouteColor()
        {
            mapRoute.RegenerateShape(gmap);
            if (mapRoute.Shape != null)
            {
                ((System.Windows.Shapes.Path)mapRoute.Shape).Stroke = new SolidColorBrush(Colors.Red);
                ((System.Windows.Shapes.Path)mapRoute.Shape).StrokeThickness = 4;
                ((System.Windows.Shapes.Path)mapRoute.Shape).Opacity = 1;
                ((System.Windows.Shapes.Path)mapRoute.Shape).Effect = null;
            }
        }
        private void updateFlightRoute(PointLatLng p)
        {
            flightRoutePoints.Add(p);
            if (flightRoutePoints.Count > GCSconfig.MaxCoursePoint)
            {
                flightRoutePoints.RemoveAt(0);
            }
            gmap.Markers.Remove(flightRoute);
            flightRoute = new GMapRoute(flightRoutePoints);
            flightRoute.RegenerateShape(gmap);
            if (flightRoute.Shape != null)
            {
                ((System.Windows.Shapes.Path)flightRoute.Shape).Stroke = new SolidColorBrush(Colors.Red);
                ((System.Windows.Shapes.Path)flightRoute.Shape).StrokeThickness = 4;
                ((System.Windows.Shapes.Path)flightRoute.Shape).Opacity = 1;
                ((System.Windows.Shapes.Path)flightRoute.Shape).Effect = null;
            }
            flightRoute.ZIndex = 10000;
            gmap.Markers.Add(flightRoute);
            if (mapCenterConfig == MapCenterPositionConfig.FollowUAV)
            {
                gmap.Position = p;
            }
        }
    }
}
