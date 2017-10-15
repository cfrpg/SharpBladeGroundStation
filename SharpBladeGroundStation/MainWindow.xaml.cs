using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SharpBladeGroundStation.CommLink;
using SharpBladeGroundStation.Map;
using SharpBladeGroundStation.DataStructs;
using GMap.NET;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Threading;
using FlightDisplay;

namespace SharpBladeGroundStation
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        SerialLink link;       
        string msg = "";		

        PortScanner portscanner;

		//displayed data
		ObservableCollection<Vector3Data> sensorData;
		ObservableCollection<Vector3Data> pidData;
        ObservableCollection<Vector3Data> motorData;
		ObservableCollection<Vector3Data> rcData;
		ObservableCollection<Vector3Data> otherData;

        FlightState flightState;
		GPSData gpsData;

		public FlightState FlightState
		{
			get
			{
				return flightState;
			}

			set
			{
				flightState = value;
			}
		}

		public MainWindow()
        {
            InitializeComponent();
			//link = new SerialLink("COM3", LinkProtocol.MAVLink);
			//link.OnReceivePackage += Link_OnReceivePackage;
			initGmap();
            portscanner = new PortScanner(LinkProtocol.ANOLink, 115200, 20480, 1);
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
			
        }

		private void initGmap()
		{
			gmap.Zoom = 3;
			gmap.MapProvider = AMapSatProvider.Instance;
			GMaps.Instance.Mode = AccessMode.ServerAndCache;
			gmap.Position = new PointLatLng(34.242947, 108.916225);
			gmap.Zoom = 18;
			
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

		private void setSensorData(string name,double x,double y,double z,bool refresh)
		{
			bool flag = true;
			for(int i=0;i<sensorData.Count;i++)
			{
				if(sensorData[i].Name==name)
				{
					sensorData[i].X = x;
					sensorData[i].Y = y;
					sensorData[i].Z = z;
					flag = false;			
					
				}
			}
			if (flag)
			{
				Action a = () => { sensorData.Add(new Vector3Data(name, x, y, z)); };
				Dispatcher.BeginInvoke(a, DispatcherPriority.Background);
				//sensorData.Add(new Vector3Data(name, x, y, z));
			}
			if (refresh)
			{
				//Action action = () => { sensorDataList.Items.Refresh(); };
				//sensorDataList.Dispatcher.Invoke(action);
			}

		}

		private void setPidData(int id, double p, double i, double d, bool refresh)
		{
			bool flag = true;
			string name = transPidName(id);
			for (int j = 0; j < pidData.Count; j++)
			{
				if (pidData[j].Name == name)
				{
					pidData[j].X = p;
					pidData[j].Y = i;
					pidData[j].Z = d;
					flag = false;

				}
			}
			if (flag)
			{
				Action a = () => { pidData.Add(new Vector3Data(name, p, i, d)); };
				Dispatcher.BeginInvoke(a, DispatcherPriority.Background);
				//pidData.Add(new Vector3Data(name, p, i, d));
			}
			if (refresh)
			{
				//Action action = () => { pidDataList.Items.Refresh(); };
				//pidDataList.Dispatcher.Invoke(action);
			}
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
					break;
				case 0x02://SENSER
					short x = package.NextShort();
					short y = package.NextShort();
					short z = package.NextShort();
					setSensorData("ACCEL", x, y, z,false);
					x = package.NextShort();
					y = package.NextShort();
					z = package.NextShort();
					setSensorData("GYRO", x, y, z,false);
					x = package.NextShort();
					y = package.NextShort();
					z = package.NextShort();
					setSensorData("MAG", x, y, z,true);
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
							setPidData(id + i+1, P, I, D, i==2);
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
			MessageBox.Show("Only for developers.", "Orz");
		}

	}
}
