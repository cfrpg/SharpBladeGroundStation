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

namespace SharpBladeGroundStation
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        SerialLink link;
        LinkPackage package;
        string msg = "";
		

        PortScanner portscanner;

		ObservableCollection<Vector3Data> sensorData;
		ObservableCollection<Vector3Data> pidData;

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

			
        }

		private void initGmap()
		{
			gmap.Zoom = 3;
			gmap.MapProvider = AMapSatProvider.Instance;
			GMaps.Instance.Mode = AccessMode.ServerOnly;
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
			Action a = () => { linkStateText.Text = link.Port.PortName + Environment.NewLine+ link.Protocol.ToString(); };
			linkStateText.Dispatcher.Invoke(a);
        }

		private void Link_OnReceivePackage(SerialLink sender, EventArgs e)
        {
            while (link.ReceivedPackageQueue.Count != 0)
            {
                package = link.ReceivedPackageQueue.Dequeue();
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

					break;
				case 0x04://GPSDATA

					break;
				case 0x05://POWER
					ushort v = package.NextUShort();
					ushort c = package.NextUShort();

					Action a05 = () => { battText.Text = string.Format("{0:F2}V {1:F2}A", (double)v / 100.0, (double)c / 100.0); };
					battText.Dispatcher.Invoke(a05);
					break;
				case 0x06://MOTO

					break;
				case 0x07://SENSER2

					break;
				case 0x0A://FLY MODEL

					break;
				case 0x0B://

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

		private void button_Click(object sender, RoutedEventArgs e)
		{
			
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

	}
}
