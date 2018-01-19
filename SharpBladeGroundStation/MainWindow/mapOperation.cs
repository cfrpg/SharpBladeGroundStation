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
using SharpBladeGroundStation.DataStructs;

namespace SharpBladeGroundStation
{
    public partial class MainWindow : Window
    {
        //GMap
        GMapMarker uavMarker;
        MapCenterPositionConfig mapCenterConfig = MapCenterPositionConfig.Free;  

        PointLatLng positionWhenTouch;

		MapRouteData newroute;
		MapRouteData flightRoute;

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
            gmap.Markers.Add(uavMarker);

            gmap.MouseLeftButtonDown += Gmap_MouseLeftButtonDown;
            gmap.MouseLeftButtonUp += Gmap_MouseLeftButtonUp;           

			newroute = new MapRouteData(gmap, 1000, true, true);
			newroute.LeftMouseButtonUp += Wp_MouseLeftButtonUp;
			newroute.RightMouseButtonDown += Wp_MouseRightButtonDown;
			flightRoute = new MapRouteData(gmap);			
		}

        private void Gmap_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            RectLatLng area = gmap.SelectedArea;
            if (area.IsEmpty && gmap.Position == positionWhenTouch)
            {	
				Point p = e.GetPosition(gmap);
				GMapMarker m = new GMapMarker(gmap.FromLocalToLatLng((int)(p.X), (int)(p.Y)));
				WayPointMarker wp = new WayPointMarker(newroute, m, (newroute.Markers.Count + 1).ToString(), string.Format("Waypoint {0}\nLat {1}\nLon {2}\n", newroute.Markers.Count + 1, m.Position.Lat, m.Position.Lng));
				newroute.AddWaypoint(wp, m);

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

		//move waypoint,almost ok
        private void Wp_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			WayPointMarker wp = sender as WayPointMarker;			
			newroute.RefreshWayPoint(wp);
			e.Handled = true;
		}

		//remove wp,ok
        private void Wp_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
			WayPointMarker wp = sender as WayPointMarker;
			int cnt = 1;
			foreach(GMapElement ge in newroute.Markers)
			{
				if (ge == wp)
					continue;
				((WayPointMarker)ge).MarkerText = cnt.ToString();
				cnt++;
			}
			newroute.RemoveWayPoint(wp);
			e.Handled = true;
        }
       
		//ok
        private void updateFlightRoute(PointLatLng p)
        {			
			flightRoute.AddWaypoint(p);
        }
    }
}
