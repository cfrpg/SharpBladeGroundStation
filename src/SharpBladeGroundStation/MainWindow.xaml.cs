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
using SharpBladeGroundStation.CommunicationLink;
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
        //SerialLink link;       

        Vehicle currentVehicle;
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

		//FlightState flightState;
		GPSData gpsData;

		HUDWindow hudWindow;
		//temps
		


		const string messageboxTitle = "SharpBladeGroundStation";
		//public FlightState FlightState
		//{
		//	get { return flightState; }
		//	set { flightState = value; }
		//}

		public GCSConfiguration GCSConfig
		{
			get { return GCSconfig; }
			set { GCSconfig = value; }
		}

        public Vehicle CurrentVehicle
        {
            get { return currentVehicle; }
            set { currentVehicle = value; }
        }

        public MainWindow()
        {
            InitializeComponent();
			initConfig();			
			initGmap();			
			portscanner = new AdvancedPortScanner(GCSconfig.BaudRate, 1000, 3);
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
            currentVehicle = new Vehicle(0);
            pfd.DataContext = currentVehicle.FlightState;
			gpsData = new GPSData();
			vdopText.DataContext = gpsData;
			hdopText.DataContext = gpsData;
			gpsStateText.DataContext = gpsData;
            battText.DataContext = currentVehicle.Battery;
			flightDataGrid.DataContext = currentVehicle.FlightState;

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
			catch(Exception ex)
			{
				MessageBox.Show(ex.ToString());
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
			if (currentVehicle.Link != null)
				return;
			currentVehicle.Link = e.Link;
            currentVehicle.Link.OnReceivePackage += Link_OnReceivePackage;
            currentVehicle.Link.OpenPort();
			Action a = () => { linkStateText.Text = currentVehicle.Link.Port.PortName + Environment.NewLine+ currentVehicle.Link.Protocol.ToString(); linkspd.DataContext = currentVehicle.Link; };
			linkStateText.Dispatcher.Invoke(a);
			
        }

		private void Link_OnReceivePackage(CommLink sender, EventArgs e)
        {
            while (currentVehicle.Link.ReceivedPackageQueue.Count != 0)
            {
                LinkPackage package = currentVehicle.Link.ReceivedPackageQueue.Dequeue();
				switch(sender.Protocol)
				{
					case LinkProtocol.MAVLink:
						analyzeMAVPackage(package);
						break;
					case LinkProtocol.ANOLink:
						analyzeANOPackage(package);
						break;
					case LinkProtocol.MAVLink2:
						analyzeMAVPackage(package);
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
            currentVehicle.Link.SendPackageQueue.Enqueue(p.Clone());
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
                currentVehicle.Link.SendPackageQueue.Enqueue(p);
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
			Stream s = new FileStream(path + "\\gcs.xml", FileMode.Create, FileAccess.Write, FileShare.None);	
			xs.Serialize(s, GCSConfig);
			s.Close();
			hudWindow.Close();
		}

		private void mainwindow_Loaded(object sender, RoutedEventArgs e)
		{
			hudWindow = new HUDWindow(this);
			hudWindow.Mainwin = this;
			hudWindow.Show();
			
		}
	}
}
