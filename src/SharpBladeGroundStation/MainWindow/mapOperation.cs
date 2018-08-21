using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using GMap.NET;
using GMap.NET.WindowsPresentation;
using SharpBladeGroundStation.Map;
using SharpBladeGroundStation.Map.Markers;
using SharpBladeGroundStation.Configuration;
using SharpBladeGroundStation.DataStructs;
using System.Threading;

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
			//gmap.Zoom = 3;
			gmap.MapProvider = AMapHybirdProvider.Instance;
			//gmap.MapProvider = GMap.NET.MapProviders.BingHybridMapProvider.Instance;

			gmap.Position = new PointLatLng(34.242947, 108.916225);
			gmap.Zoom = 17;
			gmap.ShowCenter = false;
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
			gmap.OnMapZoomChanged += Gmap_OnMapZoomChanged;

			newroute = new MapRouteData(gmap, 1000, true, true);
			newroute.LeftMouseButtonUp += Wp_MouseLeftButtonUp;
			newroute.RightMouseButtonDown += Wp_MouseRightButtonDown;
			newroute.MouseWheel += Wp_MouseWheel;
			flightRoute = new MapRouteData(gmap);
			Gmap_OnMapZoomChanged();
			DispatcherTimer waitMap = new DispatcherTimer();
			waitMap.Interval = new TimeSpan(0, 0, 0, 0, 500);
			waitMap.Tick += WaitMap_Tick;
			waitMap.Start();
		}

		private void WaitMap_Tick(object sender, EventArgs e)
		{
			DispatcherTimer dt = sender as DispatcherTimer;
			if (mapscale.Scale > 0.1f)
			{
				dt.Stop();
				dt = null;
				return;
			}
			Gmap_OnMapZoomChanged();

		}

		private void Gmap_OnMapZoomChanged()
		{
			mapscale.Scale = getMapScale();
		}

		private float getMapScale()
		{
			return (float)(PositionHelper.GetDistance(gmap.FromLocalToLatLng(0, 0), gmap.FromLocalToLatLng(100, 0)) / 100);
		}

		private void Gmap_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			RectLatLng area = gmap.SelectedArea;
			if (area.IsEmpty && gmap.Position == positionWhenTouch)
			{
				Point p = e.GetPosition(gmap);
				GMapMarker m = new GMapMarker(gmap.FromLocalToLatLng((int)(p.X), (int)(p.Y)));
				WayPointMarker wp = new WayPointMarker(newroute, m, (newroute.Markers.Count + 1).ToString(), string.Format("Lat {0}\nLon {1}\nAlt {2} m", m.Position.Lat, m.Position.Lng, 50));
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
			wp.LabelText = string.Format("Lat {0}\nLon {1}\nAlt {2} m", wp.Position.Lat, wp.Position.Lng, wp.Altitude);
			newroute.RefreshWayPoint(wp);
			e.Handled = true;
		}

		//remove wp,ok
		private void Wp_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
		{
			WayPointMarker wp = sender as WayPointMarker;
			int cnt = 1;
			foreach (GMapElement ge in newroute.Markers)
			{
				if (ge == wp)
					continue;
				((WayPointMarker)ge).MarkerText = cnt.ToString();
				cnt++;
			}
			newroute.RemoveWayPoint(wp);
			e.Handled = true;
		}

		private void Wp_MouseWheel(object sender, MouseWheelEventArgs e)
		{
			WayPointMarker wp = sender as WayPointMarker;
			if (e.Delta > 0)
				wp.Altitude += 0.5f;
			else
				wp.Altitude -= 0.5f;
			wp.LabelText = string.Format("Lat {0}\nLon {1}\nAlt {2} m", wp.Position.Lat, wp.Position.Lng, wp.Altitude);
			newroute.RefreshWayPoint(wp);
			e.Handled = true;

		}

		//ok
		private void updateFlightRoute(PointLatLng p)
		{
			flightRoute.AddWaypoint(p);
		}
	}
}
