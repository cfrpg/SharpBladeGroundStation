using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using FlightDisplay;
using GMap.NET;
using GMap.NET.WindowsPresentation;
using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using SharpBladeGroundStation.CommLink;
using SharpBladeGroundStation.DataStructs;
using SharpBladeGroundStation.Map;
using SharpBladeGroundStation.Map.Markers;
using SharpBladeGroundStation.Configuration;

namespace SharpBladeGroundStation
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        SerialLink link;       
        string msg = "";
		GCSConfiguration GCSconfig;

        PortScanner portscanner;


		//displayed data
		ObservableCollection<Vector3Data> sensorData;
		ObservableCollection<Vector3Data> pidData;
        ObservableCollection<Vector3Data> motorData;
		ObservableCollection<Vector3Data> rcData;
		ObservableCollection<Vector3Data> otherData;

		
		ObservableDataSource<Point>[] accelGraphData = new ObservableDataSource<Point>[3];
		ObservableDataSource<Point>[] gyroGraphData = new ObservableDataSource<Point>[3];
		ObservableDataSource<Point>[] attitudeGraphData = new ObservableDataSource<Point>[3];
		ObservableDataSource<Point> altitudeGraphData;
		Dictionary<int, UInt64> dataSkipCount;

		FlightState flightState;
		GPSData gpsData;

		//GMap
		GMapMarker currentMarker;
		GMapMarker uavMarker;
		MapCenterPositionConfig mapCenterConfig = MapCenterPositionConfig.Free;
		GMapRoute mapRoute;
		List<PointLatLng> waypointPosition;

		WayPointMarker wp;
		List<WayPointMarker> waypointMarkers;

		GMapRoute flightRoute;
		List<PointLatLng> flightRoutePoints;
		//temps
		PointLatLng positionWhenTouch;

		public FlightState FlightState
		{
			get { return flightState; }
			set { flightState = value; }
		}

		public GCSConfiguration GCSConfig
		{
			get { return GCSconfig; }
			set { GCSconfig = value; }
		}

		public MainWindow()
        {
            InitializeComponent();
			initConfig();
			//link = new SerialLink("COM3", LinkProtocol.MAVLink);
			//link.OnReceivePackage += Link_OnReceivePackage;
			initGmap();
            //portscanner = new PortScanner(LinkProtocol.ANOLink, 115200, 20480, 1);
			portscanner = new PortScanner(LinkProtocol.MAVLink, 115200, 20480, 1);
			portscanner.OnFindPort += Portscanner_OnFindPort;
			portscanner.Start();
			linkStateText.Text = "Connecting";

			sensorData = new ObservableCollection<Vector3Data>();
			sensorDataList.ItemsSource = sensorData;
			pidData = new ObservableCollection<Vector3Data>();
			pidDataList.ItemsSource = pidData;
            motorData = new ObservableCollection<Vector3Data>();
            motorDataList.ItemsSource = motorData;
			rcData = new ObservableCollection<Vector3Data>();
			rcDataList.ItemsSource = rcData;
            FlightState = new FlightState();
			pfd.DataContext = flightState;
			gpsData = new GPSData();
			vdopText.DataContext = gpsData;
			hdopText.DataContext = gpsData;
			gpsStateText.DataContext = gpsData;

			flightDataGrid.DataContext = FlightState;

			otherData = new ObservableCollection<Vector3Data>();
			otherDataList.ItemsSource = otherData;

			string[] xyz = { "X", "Y", "Z" };
			string[] ypr = { "Roll", "Pitch", "Yaw" };
			for(int i=0;i<3;i++)
			{
				accelGraphData[i] = new ObservableDataSource<Point>();
				accelPlotter.AddLineGraph(accelGraphData[i],"Accel "+xyz[i]);
				gyroGraphData[i] = new ObservableDataSource<Point>();
				gyroPlotter.AddLineGraph(gyroGraphData[i], "Gyro " + xyz[i]);
			}
			for(int i=0;i<3;i++)
			{
				attitudeGraphData[i] = new ObservableDataSource<Point>();
				attPlotter.AddLineGraph(attitudeGraphData[i], ypr[i]);
			}
			altitudeGraphData = new ObservableDataSource<Point>();
			altPlotter.AddLineGraph(altitudeGraphData, "Altitude");

			dataSkipCount = new Dictionary<int, ulong>();
			for(int i=0;i<255;i++)
			{
				dataSkipCount[i] = 0;
			}
		}
		private void initConfig()
		{
			string path = Environment.CurrentDirectory+"\\config";
			DirectoryInfo di = new DirectoryInfo(path);
			if (!di.Exists)
				di.Create();
			//FileInfo fi = new FileInfo(path + "\\gcs.cfg");
			XmlSerializer xs = new XmlSerializer(typeof(GCSConfiguration));
			Stream s = new FileStream(path + "\\gcs.xml", FileMode.OpenOrCreate,FileAccess.ReadWrite,FileShare.None);
			try
			{
				GCSconfig=(GCSConfiguration)xs.Deserialize(s);
				s.Close();
			}
			catch
			{
				s.Close();
				s= new FileStream(path + "\\gcs.xml", FileMode.Create, FileAccess.Write, FileShare.None);
				GCSconfig = GCSConfiguration.DefaultConfig();				
				xs.Serialize(s, GCSConfig);
				s.Close();
			}
			
					
		}
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
				if(ping.Send("www.autonavi.com").Status!=System.Net.NetworkInformation.IPStatus.Success)
				{
					GMaps.Instance.Mode = AccessMode.CacheOnly;
				}
				else
				{
					GMaps.Instance.Mode = AccessMode.ServerOnly;

				}
			}
			catch
			{
				GMaps.Instance.Mode = AccessMode.CacheOnly;
			}
			//gmap.Manager.Mode = AccessMode.CacheOnly;		
			
			//GMaps.Instance.Mode = AccessMode.CacheOnly;
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
			if(area.IsEmpty&&gmap.Position==positionWhenTouch)
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
			if(!area.IsEmpty)
			{
				if(MessageBox.Show("缓存选定区域地图？","SharpBladeGroundStation",MessageBoxButton.YesNo)==MessageBoxResult.Yes)
				{
					if(MessageBox.Show("清除原有缓存？", "SharpBladeGroundStation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
					{
						gmap.Manager.PrimaryCache.DeleteOlderThan(DateTime.Now, null);
					}
					AccessMode m = GMaps.Instance.Mode;
					GMaps.Instance.Mode = AccessMode.ServerAndCache;
					for(int i=(int)gmap.Zoom;i<=gmap.MaxZoom;i++)
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
			for(int i=0;i<waypointMarkers.Count;i++)
			{
				waypointMarkers[i].MarkerText = (i + 1).ToString();
			}
			reGeneRoute();
			e.Handled = true;
		}
		private void reGeneRoute()
		{
			List<PointLatLng> points=new List<PointLatLng>();
			foreach(var v in waypointMarkers)
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
			if(flightRoutePoints.Count>GCSconfig.MaxCoursePoint)
			{
				flightRoutePoints.RemoveAt(0);
			}
			gmap.Markers.Remove(flightRoute);
			flightRoute = new GMapRoute(flightRoutePoints);
			flightRoute.RegenerateShape(gmap);
			if(flightRoute.Shape!=null)
			{
				((System.Windows.Shapes.Path)flightRoute.Shape).Stroke = new SolidColorBrush(Colors.Red);
				((System.Windows.Shapes.Path)flightRoute.Shape).StrokeThickness = 4;
				((System.Windows.Shapes.Path)flightRoute.Shape).Opacity = 1;
				((System.Windows.Shapes.Path)flightRoute.Shape).Effect = null;
			}
			flightRoute.ZIndex = 10000;
			gmap.Markers.Add(flightRoute);
		}
		private void Portscanner_OnFindPort(PortScanner sender, PortScannerEventArgs e)
        {
			Debug.WriteLine("[main] find port {0}", e.Link.Port.PortName);
			portscanner.Stop();
			if (link != null)
				return;
			link = e.Link;
			link.OnReceivePackage += Link_OnReceivePackage;
			link.OpenPort();
			Action a = () => { linkStateText.Text = link.Port.PortName + Environment.NewLine+ link.Protocol.ToString(); linkspd.DataContext = link; };
			linkStateText.Dispatcher.Invoke(a);
			
        }

		private void Link_OnReceivePackage(SerialLink sender, EventArgs e)
        {
            while (link.ReceivedPackageQueue.Count != 0)
            {
                LinkPackage package = link.ReceivedPackageQueue.Dequeue();
				switch(sender.Protocol)
				{
					case LinkProtocol.MAVLink:
						analyzeMAVPackage(package);
						break;
					case LinkProtocol.ANOLink:
						analyzeANOPackage(package);
						break;
				}
            }
        }
		

		private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			leftcol.MaxWidth = Math.Min(400, (e.NewSize.Height-30)/669*300);
		}

        private void setVector3Data(string name,double x,double y,double z,ObservableCollection<Vector3Data> list)
        {
            bool flag = true;
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Name == name)
                {
                    list[i].X = x;
                    list[i].Y = y;
                    list[i].Z = z;
                    flag = false;

                }
            }
            if (flag)
            {
                Action a = () => { list.Add(new Vector3Data(name, x, y, z)); };
                Dispatcher.BeginInvoke(a, DispatcherPriority.Background);
                
            }
        }

		private void analyzeMAVPackage(LinkPackage p)
		{
			MAVLinkPackage package = (MAVLinkPackage)p;
			package.StartRead();
			UInt32 time = 0;
			UInt64 time64 = 0;
			UInt64 dt = (ulong)GCSconfig.PlotTimeInterval*1000;
			switch(package.Function)
			{
				case 1:		//SYS_STATUS

					break;

				case 24:    //GPS_RAW_INT 
					time64 = package.NextUInt64();
					gpsData.Latitude = package.NextInt32()*1.0 / 1e7;
					gpsData.Longitude = package.NextInt32()*1.0 / 1e7;
                    int talt = package.NextInt32();
					gpsData.Hdop = package.NextUShort();
					if (gpsData.Hdop > 10000)
						gpsData.Hdop = -1;
					gpsData.Vdop = package.NextUShort();
					if (gpsData.Vdop > 10000)
						gpsData.Vdop = -1;
                    gpsData.Vdop /= 100f;
                    gpsData.Hdop /= 100f;
					flightState.GroundSpeed = package.NextUShort()/100.0f;
					
					gpsData.SatelliteCount = package.NextUShort();
					GPSPositionState gpss= (GPSPositionState)package.NextByte();//sb文档害我debug一天!
					

					if ((ulong)time * 1000 - dataSkipCount[package.Function] > dt)
					{
						attitudeGraphData[0].AppendAsync(this.Dispatcher, new Point(time / 1000.0, flightState.Roll));
						attitudeGraphData[1].AppendAsync(this.Dispatcher, new Point(time / 1000.0, flightState.Pitch));
						attitudeGraphData[2].AppendAsync(this.Dispatcher, new Point(time / 1000.0, flightState.Yaw));

						dataSkipCount[package.Function] = (ulong)time * 1000;
					}
					if (gpsData.State == GPSPositionState.NoGPS && gpss != GPSPositionState.NoGPS)
					{
						flightRoutePoints.Clear();
						dataSkipCount[package.Function] = 0;
					}
					gpsData.State = gpss;
					if ((ulong)time - dataSkipCount[package.Function] > (ulong)GCSconfig.CourseTimeInterval*1000)
					{
						Action a241 = () => { updateFlightRoute(new PointLatLng(gpsData.Latitude, gpsData.Longitude)); };
						Dispatcher.BeginInvoke(a241);
					}
					gpsData.SatelliteCount = package.NextByte();
					Action a24 = () => { uavMarker.Position = PositionHelper.WGS84ToGCJ02( new PointLatLng(gpsData.Latitude, gpsData.Longitude)); };
					Dispatcher.BeginInvoke(a24);
					break;
				case 30:
					time = package.NextUInt32();
					flightState.Roll = -rad2deg(package.NextSingle());
					flightState.Pitch = rad2deg(package.NextSingle());
					flightState.Yaw = rad2deg(package.NextSingle());
					float h = flightState.Yaw;
					flightState.Heading = h < 0 ? 360 + h : h;
					Action a30 = () => { uavMarker.Shape.RenderTransform = new RotateTransform(flightState.Heading, 15, 15); };
					Dispatcher.BeginInvoke(a30);
					
					if ((ulong)time * 1000-dataSkipCount[package.Function]  > dt)
					{						
						attitudeGraphData[0].AppendAsync(this.Dispatcher, new Point(time / 1000.0, flightState.Roll));
						attitudeGraphData[1].AppendAsync(this.Dispatcher, new Point(time / 1000.0, flightState.Pitch));
						attitudeGraphData[2].AppendAsync(this.Dispatcher, new Point(time / 1000.0, flightState.Yaw));
						
						dataSkipCount[package.Function] = (ulong)time * 1000;
					}
					break;
				
				case 32:    //LOCAL_POSITION_NED
					time = package.NextUInt32();
					float vx = package.NextSingle();
					float vy = package.NextSingle();
					float vz = package.NextSingle();
					vx = package.NextSingle();
					vy = package.NextSingle();
					vz = package.NextSingle();
					flightState.ClimbRate = -vz;

					break;
				case 141:   //ALTITUDE 
					time64 = package.NextUInt64();
					flightState.Altitude = package.NextSingle();
					if (time64-dataSkipCount[package.Function] > dt)
					{
						altitudeGraphData.AppendAsync(this.Dispatcher, new Point(time64 / 1000000.0, flightState.Altitude));
						dataSkipCount[package.Function] = time64;
					}
					break;
				case 105:   //HIGHRES_IMU
					time64 = package.NextUInt64();
					float[] sd = { 0, 0, 0 };
					sd[0] = package.NextSingle();
					sd[1] = package.NextSingle();
					sd[2] = package.NextSingle();
					//setSensorData("ACCEL", sd[0], sd[1], sd[2], false);
					setVector3Data("ACCEL", sd[0], sd[1], sd[2], sensorData);
					if (time64 - dataSkipCount[package.Function] > dt)
					{
						for (int i = 0; i < 3; i++)
						{
							accelGraphData[i].AppendAsync(this.Dispatcher, new Point(time64 / 1000000.0, sd[i]));
						}
					}

					sd[0] = package.NextSingle();
					sd[1] = package.NextSingle();
					sd[2] = package.NextSingle();
					//setSensorData("GYRO", sd[0], sd[1], sd[2], false);
					setVector3Data("GYRO", sd[0], sd[1], sd[2], sensorData);
					if (time64-dataSkipCount[package.Function] > dt)
					{
						for (int i = 0; i < 3; i++)
						{
							gyroGraphData[i].AppendAsync(this.Dispatcher, new Point(time64 / 1000000.0, sd[i]));
						}
						dataSkipCount[package.Function] = time64;
					}

					sd[0] = package.NextSingle();
					sd[1] = package.NextSingle();
					sd[2] = package.NextSingle();
					//setSensorData("MAG", sd[0], sd[1], sd[2], true);
					setVector3Data("MAG", sd[0], sd[1], sd[2], sensorData);
					break;
				default:

					break;
			}

		}
		private float rad2deg(float rad)
		{
			return (float)(rad / Math.PI * 180);
		}
		private float deg2rad(float deg)
		{
			return (float)(deg * Math.PI / 180);
		}
		private void analyzeANOPackage(LinkPackage p)
		{
			ANOLinkPackage package = (ANOLinkPackage)p;
			package.StartRead();
			switch(package.Function)
			{
				case 0x00://VER
					
					break;
				case 0x01://STATUS
					flightState.Roll = package.NextShort()/100f;
					flightState.Pitch = package.NextShort() / 100f;
					flightState.Yaw = package.NextShort()/100f;
					flightState.Altitude = package.NextInt32()/100f;
					flightState.FlightModeText = getFlightModeText( package.NextByte());
					flightState.IsArmed = package.NextByte() == 1;
					attitudeGraphData[0].AppendAsync(this.Dispatcher, new Point(package.TimeStamp / 1000, flightState.Roll));
					attitudeGraphData[1].AppendAsync(this.Dispatcher, new Point(package.TimeStamp / 1000, flightState.Pitch));
					attitudeGraphData[2].AppendAsync(this.Dispatcher, new Point(package.TimeStamp / 1000, flightState.Yaw));
					altitudeGraphData.AppendAsync(this.Dispatcher, new Point(package.TimeStamp / 1000, flightState.Altitude / 100.0));
					break;
				case 0x02://SENSER
					short[] sd = { 0, 0, 0 };
					sd[0] = package.NextShort();
					sd[1] = package.NextShort();
					sd[2] = package.NextShort();
					//setSensorData("ACCEL", sd[0], sd[1], sd[2], false);
					setVector3Data("ACCEL", sd[0], sd[1], sd[2], sensorData);
					for (int i=0;i<3;i++)
					{
						accelGraphData[i].AppendAsync(this.Dispatcher, new Point(package.TimeStamp / 1000, sd[i]));
					}					

					sd[0] = package.NextShort();
					sd[1] = package.NextShort();
					sd[2] = package.NextShort();
					//setSensorData("GYRO", sd[0], sd[1], sd[2], false);
					setVector3Data("GYRO", sd[0], sd[1], sd[2], sensorData);
					for (int i = 0; i < 3; i++)
					{
						gyroGraphData[i].AppendAsync(this.Dispatcher, new Point(package.TimeStamp / 1000, sd[i]));
					}

					sd[0] = package.NextShort();
					sd[1] = package.NextShort();
					sd[2] = package.NextShort();
					//setSensorData("MAG", sd[0], sd[1], sd[2], true);
					setVector3Data("MAG", sd[0], sd[1], sd[2], sensorData);
					break;
				case 0x03://RCDATA
					for(int i=0;i<10;i++)
					{
						short rc = package.NextShort();
						setVector3Data(getRCChannelName(i), rc, rc, rc, rcData);
					}
					break;
				case 0x04://GPSDATA
					gpsData.State = (GPSPositionState)package.NextByte();
					gpsData.SatelliteCount = package.NextByte();
					gpsData.Longitude = package.NextInt32() / 10000000.0f;
					gpsData.Latitude = package.NextInt32() / 10000000.0f;
					gpsData.HomingAngle = package.NextShort() / 10.0f;

					break;
				case 0x05://POWER
					ushort v = package.NextUShort();
					ushort c = package.NextUShort();

					Action a05 = () => { battText.Text = string.Format("{0:F2}V {1:F2}A", (double)v / 100.0, (double)c / 100.0); };
					battText.Dispatcher.Invoke(a05);
					break;
				case 0x06://MOTO
                    for(int i=1;i<=8;i++)
                    {
                        short pwm = package.NextShort();
                        setVector3Data("PWM" + i.ToString(), pwm, pwm, pwm, motorData);
                    }
					break;
				case 0x07://SENSER2
					int altbar = package.NextInt32();
					setVector3Data("ALT_BAR", altbar, 0, 0, otherData);
					
					altbar = package.NextUShort();
					setVector3Data("ALT_CSB", altbar, 0, 0, otherData);
					break;
				case 0x0A://FLY MODEL

					break;
				case 0x0B://
					short sr = package.NextShort();
					short sp = package.NextShort();
					FlightState.ClimbRate = package.NextShort() / 100.0f;
					setVector3Data("角速度", sr, sp, 0, otherData);
					break;
				case 0x20://FP_NUMBER

					break;
				case 0x21://FP

					break;
				case 0xEF://CHECK

					break;
				default:
					if(package.Function>=0x10&&package.Function<=0x15)
					{
						int id = (package.Function - 0x10)*3;
						for(int i=0;i<3;i++)
						{
							short P = package.NextShort();
							short I = package.NextShort();
							short D = package.NextShort();							
							//setPidData(id + i+1, P, I, D, i==2);
							setVector3Data(transPidName(id + i + 1), P,I,D, sensorData);
						}
					}
					break;
			}
		}

		private string getRCChannelName(int id)
		{
			return "CH" + id.ToString();
		}
		

		private void button1_Click(object sender, RoutedEventArgs e)
		{
			ANOLinkPackage p = new ANOLinkPackage();
			p.Function = 0x02;
			p.AddData((byte)0x01);
			p.SetVerify();
			link.SendPackageQueue.Enqueue(p.Clone());
		}

		private void button2_Click(object sender, RoutedEventArgs e)
		{
			Vector3Data[] pids = new Vector3Data[18];
			for(int i=0;i<pidData.Count;i++)
			{
				int id = transPidName(pidData[i].Name);
				pids[id-1] = pidData[i].Clone();
			}
			for(int i=0;i<18;i++)
			{
				if (pids[i] == null)
					pids[i] = new Vector3Data(transPidName(i + 1));
			}
			for(int i=0;i<6;i++)
			{
				ANOLinkPackage p = new ANOLinkPackage();
				p.Function = (byte)i;
				p.Function += 0x10;
				for(int j=0;j<3;j++)
				{
					p.AddData((short)pids[i * 3 + j].X);
					p.AddData((short)pids[i * 3 + j].Y);
					p.AddData((short)pids[i * 3 + j].Z);
				}
				p.SetVerify();
				link.SendPackageQueue.Enqueue(p);
			}
		}

		private string transPidName(int id)
		{
			return "PID" + id.ToString();
		}

		private int transPidName(string name)
		{
			return int.Parse(name.Substring(3));
		}

		private string getFlightModeText(int id)
		{
			switch(id)
			{
				case 0:
					return "未知";
					
				case 1:
					return "姿态";
					
				case 2:
					return "定高";
					
				case 3:
					return "定点";
					
				case 11:
					return "航线";
					
				case 20:
					return "降落";
					
				case 21:
					return "返航";
					
				default:
					return "未知";
					
			}
		}

		private void button_Click(object sender, RoutedEventArgs e)
		{			
			//MessageBox.Show("Only for developers.", "Orz");
			
			
		}

		private void button3_Click(object sender, RoutedEventArgs e)
		{
			if (leftcol.Width.Value != 0)
			{
				leftcol.MinWidth = 0;
				leftcol.Width = new GridLength(0, GridUnitType.Star);
				button3.Content = "▶";
			}
			else
			{
				leftcol.MinWidth = 250;
				leftcol.Width = new GridLength(300, GridUnitType.Star);
				button3.Content = "◀";
			}
		}
	}
}
