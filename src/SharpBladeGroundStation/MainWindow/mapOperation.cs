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
using SharpBladeGroundStation.CommunicationLink;
using System.Threading;
using System.Windows.Controls;

namespace SharpBladeGroundStation
{
	public partial class MainWindow : Window
	{
		//GMap
		GMapMarker uavMarker;
		GMapMarker homeMarker;
		MapCenterPositionConfig mapCenterConfig = MapCenterPositionConfig.Free;

		PointLatLng positionWhenTouch;

		
		MapRouteData flightRoute;

		MissionManager missionManager;

		private void initGmap()
		{
            missionManager = new MissionManager(currentVehicle);
            
            missionManager.OnFinished += MissionManager_OnFinished;
            missionTreeView.ItemsSource = missionManager.MissionList;

            for (int i=0;i<mapComboBox.Items.Count;i++)
            {
                if(GCSConfig.MapName==(string)mapComboBox.Items[i])
                {
                    mapComboBox.SelectedIndex = i;
                    break;
                }
            }

            setMapProvider(GCSConfig.MapName);
            gmap.Position = new PointLatLng(34.242947, 108.916225);
			gmap.Zoom = 17;
			gmap.ShowCenter = false;
			
			uavMarker = new GMapMarker(gmap.Position);
			uavMarker.Shape = new UAVMarker();
			uavMarker.Offset = new Point(-15, -15);
			uavMarker.ZIndex = 100000;
			gmap.Markers.Add(uavMarker);

			homeMarker = new GMapMarker(new PointLatLng(0, 0));
			homeMarker.Shape = new HomeMarker();
			homeMarker.Offset = new Point(-10, -10);
			homeMarker.ZIndex = 100000-1;
			gmap.Markers.Add(homeMarker);

			gmap.MouseLeftButtonDown += Gmap_MouseLeftButtonDown;
			gmap.MouseLeftButtonUp += Gmap_MouseLeftButtonUp;
			gmap.OnMapZoomChanged += Gmap_OnMapZoomChanged;

			MapRouteData newroute;
			newroute = new MapRouteData(gmap, 1000, true, true);
			newroute.Color = Colors.White;
			newroute.LeftMouseButtonUp += Wp_MouseLeftButtonUp;
			newroute.RightMouseButtonDown += Wp_MouseRightButtonDown;
			newroute.MouseWheel += Wp_MouseWheel;
			missionManager.LocalMission = newroute;
			missionManager.MissionList.Add(new Mission(newroute) { ID = 0, Name = "航线1" ,Color=Colors.White});

			flightRoute = new MapRouteData(gmap);
			Gmap_OnMapZoomChanged();
			DispatcherTimer waitMap = new DispatcherTimer();
			waitMap.Interval = new TimeSpan(0, 0, 0, 0, 500);
			waitMap.Tick += WaitMap_Tick;
			waitMap.Start();
		}

        private void mapComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GCSconfig.MapName = (string)mapComboBox.SelectedItem;
            setMapProvider((string)mapComboBox.SelectedItem);
        }

        private void setMapProvider(string name)
        {
            string url = "";
            switch(name)
            {
                case "高德地图":
                    gmap.MapProvider = AMapHybirdProvider.Instance;
                    url = "www.autonavi.com";
                    break;
                case "谷歌中国地图":
                    gmap.MapProvider = GMap.NET.MapProviders.GoogleChinaHybridMapProvider.Instance;
                    url = "ditu.google.cn";
                    break;
                default:
                    GCSconfig.MapName = "谷歌中国地图";
                    gmap.MapProvider = GMap.NET.MapProviders.GoogleChinaHybridMapProvider.Instance;
                    url = "ditu.google.cn";
                    break;
            }
            System.Net.NetworkInformation.Ping ping = new System.Net.NetworkInformation.Ping();
            try
            {
                if (ping.Send(url).Status != System.Net.NetworkInformation.IPStatus.Success)
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

        }

		private void MissionManager_OnFinished()
		{
			this.Dispatcher.BeginInvoke(new ThreadStart(delegate { MessageBox.Show("操作成功！"); }));
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

		//add wp,ok.
		private void Gmap_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			RectLatLng area = gmap.SelectedArea;
			if (area.IsEmpty && gmap.Position == positionWhenTouch)
			{
				Point p = e.GetPosition(gmap);
				//GMapMarker m = new GMapMarker(gmap.FromLocalToLatLng((int)(p.X), (int)(p.Y)));
				//WayPointMarker wp = new WayPointMarker(missionManager.LocalMission, m, (missionManager.LocalMission.Markers.Count + 1).ToString());
				//missionManager.LocalMission.AddWaypoint(wp, m);
				((Mission)missionManager.MissionList[0]).AddWaypoint(
					new Waypoint(0) { Position = gmap.FromLocalToLatLng((int)(p.X), (int)(p.Y)), Altitude = 50,UseRelativeAlt=true});
				
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

		//move waypoint
		private void Wp_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			WayPointMarker wp = sender as WayPointMarker;			
			missionManager.LocalMission.RefreshWayPoint(wp);
			e.Handled = true;
		}

		//remove wp
		private void Wp_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
		{
			WayPointMarker wp = sender as WayPointMarker;
			int cnt = 1;
			foreach (GMapElement ge in missionManager.LocalMission.Markers)
			{
				if (ge == wp)
					continue;
				((WayPointMarker)ge).MarkerText = cnt.ToString();
				cnt++;
			}
			missionManager.LocalMission.RemoveWayPoint(wp);
			e.Handled = true;
		}

		//update wp
		private void Wp_MouseWheel(object sender, MouseWheelEventArgs e)
		{
			WayPointMarker wp = sender as WayPointMarker;
			if (e.Delta > 0)
				wp.Altitude += 0.5f;
			else
				wp.Altitude -= 0.5f;			
			missionManager.LocalMission.RefreshWayPoint(wp);
			e.Handled = true;
		}

		//ok
		private void updateFlightRoute(PointLatLng p)
		{
			flightRoute.AddWaypoint(p);
		}

		private void colorbtn_Click(object sender, RoutedEventArgs e)
		{
			System.Windows.Forms.ColorDialog cd = new System.Windows.Forms.ColorDialog();
			if(cd.ShowDialog()==System.Windows.Forms.DialogResult.OK)
			{
				System.Drawing.Color c = cd.Color;
				Mission m= (Mission)missionTreeView.SelectedItem;
				m.Color = Color.FromArgb(c.A, c.R, c.G, c.B);
			}
			e.Handled = false;
		}
	}
}
