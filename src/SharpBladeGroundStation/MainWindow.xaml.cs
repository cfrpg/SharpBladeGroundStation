using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Speech.Synthesis;
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
using System.MAVLink;

namespace SharpBladeGroundStation
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        SerialLink link;       
       
		GCSConfiguration GCSconfig;

        //PortScanner portscanner;
		AdvancedPortScanner portscanner;

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

		HUDWindow hudWindow;
		//temps
		


		const string messageboxTitle = "SharpBladeGroundStation";
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
			//portscanner = new PortScanner(LinkProtocol.MAVLink, 115200, 20480, 1);
			//portscanner.OnFindPort += Portscanner_OnFindPort;
			//portscanner.Start();
			portscanner = new AdvancedPortScanner(GCSconfig.BaudRate, 1000, 3);
			//portscanner = new AdvancedPortScanner(57600, 1000, 3);
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
			gcsCfgGroup.DataContext = GCSconfig;

		}
		

		
		private void Portscanner_OnFindPort(AdvancedPortScanner sender, PortScannerEventArgs e)
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

		
		private float rad2deg(float rad)
		{
			return (float)(rad / Math.PI * 180);
		}
		private float deg2rad(float deg)
		{
			return (float)(deg * Math.PI / 180);
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
			//SpeechSynthesizer ss = new SpeechSynthesizer();
			//ss.Rate = 0;
			//ss.Speak("鹅鹅鹅鹅鹅鹅鹅鹅鹅嗯，鹅鹅鹅鹅鹅鹅鹅鹅鹅嗯，启动失败。");
			//MessageBox.Show("黑科技启动失败", "Orz");
			//getJoysticks();
			var screens=System.Windows.Forms.Screen.AllScreens;
			var currscn=System.Windows.Forms.Screen.FromPoint(System.Windows.Forms.Cursor.Position);
			if (screens.Count() == 1)
			{
				SpeechSynthesizer ss = new SpeechSynthesizer();
				ss.Rate = 0;
				ss.Speak("鹅鹅鹅鹅鹅鹅鹅鹅鹅嗯，鹅鹅鹅鹅鹅鹅鹅鹅鹅嗯，启动失败。");
				MessageBox.Show("黑科技启动失败", "Orz");
				return;
			}
			int a = 0, b = 0;
			for(int i=0;i<screens.Count();i++)
			{
				if(screens[i].DeviceName==currscn.DeviceName)
				{
					a = (i + 1) % screens.Count();
					b = i;
					break;
				}
			}
			this.WindowState = WindowState.Normal;
			this.Top = screens[a].WorkingArea.Top;
			this.Left = screens[a].WorkingArea.Left;
			this.WindowState = WindowState.Maximized;
			hudWindow.WindowState = WindowState.Normal;
			hudWindow.Top = screens[b].WorkingArea.Top;
			hudWindow.Left = screens[b].WorkingArea.Left;
			hudWindow.WindowState = WindowState.Maximized;
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

		private void clearBtn_Click(object sender, RoutedEventArgs e)
		{
			if(MessageBox.Show("清空所有航线?",messageboxTitle,MessageBoxButton.YesNoCancel)==MessageBoxResult.Yes)
			{
				newroute.Clear();
			}
		}

		private void uploadBtn_Click(object sender, RoutedEventArgs e)
		{
			MessageBox.Show("这是没有实装的上传航线", "orz");
		}

		private void downloadBtn_Click(object sender, RoutedEventArgs e)
		{
			MessageBox.Show("这是没有实装的下载航线", "orz");
		}

		private void pathPlanBtn_Click(object sender, RoutedEventArgs e)
		{
			MessageBox.Show("这是没有实装的规划", "orz");
		}

		private void followBtn_Click(object sender, RoutedEventArgs e)
		{
			if (mapCenterConfig == MapCenterPositionConfig.Free)
			{
				mapCenterConfig = MapCenterPositionConfig.FollowUAV;
				followBtn.Background = new SolidColorBrush(Color.FromArgb(204, 255, 20, 20));
				
			}
			else
			{
				mapCenterConfig = MapCenterPositionConfig.Free;
				followBtn.Background = new SolidColorBrush(Color.FromArgb(204, 100, 100, 100));
			}
		}

		private void mainwindow_Closing(object sender, CancelEventArgs e)
		{
			string path = Environment.CurrentDirectory + "\\config";
			
			XmlSerializer xs = new XmlSerializer(typeof(GCSConfiguration));
			Stream s = new FileStream(path + "\\gcs.xml", FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);	
			xs.Serialize(s, GCSConfig);
			s.Close();
			hudWindow.Close();
			
		}

		private void mainwindow_Loaded(object sender, RoutedEventArgs e)
		{
			hudWindow = new HUDWindow();
			hudWindow.Mainwin = this;
			hudWindow.Show();
			
		}
	}
}
