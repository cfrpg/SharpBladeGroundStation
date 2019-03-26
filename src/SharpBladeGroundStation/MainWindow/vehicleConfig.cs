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
using System.IO.Ports;
using System.Threading;
using SharpBladeGroundStation.CommunicationLink.BootLoader;


namespace SharpBladeGroundStation
{
	public partial class MainWindow : Window
	{
		List<orntData> orntList;
		List<string> essentialParamList;
		private void initVehicleConfig()
		{			
			for (int i = 0; i <= 360; i += 45)
			{
				orntYawBox.Items.Add(i.ToString());
				orntPitchBox.Items.Add(i.ToString());
				orntRollBox.Items.Add(i.ToString());
			}
			orntList = new List<orntData>();
			orntList.Add(new orntData(0, 0, 0, 0));
			orntList.Add(new orntData(1, 0, 0, 45));
			orntList.Add(new orntData(2, 0, 0, 90));
			orntList.Add(new orntData(3, 0, 0, 135));
			orntList.Add(new orntData(4, 0, 0, 180));
			orntList.Add(new orntData(5, 0, 0, 225));
			orntList.Add(new orntData(6, 0, 0, 270));
			orntList.Add(new orntData(7, 0, 0, 315));
			orntList.Add(new orntData(8, 180, 0, 0));
			orntList.Add(new orntData(9, 180, 0, 45));
			orntList.Add(new orntData(10, 180, 0, 90));
			orntList.Add(new orntData(11, 180, 0, 135));
			orntList.Add(new orntData(12, 0, 180, 0));
			orntList.Add(new orntData(13, 180, 0, 225));
			orntList.Add(new orntData(14, 180, 0, 270));
			orntList.Add(new orntData(15, 180, 0, 315));
			orntList.Add(new orntData(16, 90, 0, 0));
			orntList.Add(new orntData(17, 90, 0, 45));
			orntList.Add(new orntData(18, 90, 0, 90));
			orntList.Add(new orntData(19, 90, 0, 135));
			orntList.Add(new orntData(20, 270, 0, 0));
			orntList.Add(new orntData(21, 270, 0, 45));
			orntList.Add(new orntData(22, 270, 0, 90));
			orntList.Add(new orntData(23, 270, 0, 135));
			orntList.Add(new orntData(24, 0, 90, 0));
			orntList.Add(new orntData(25, 0, 270, 0));
			orntList.Add(new orntData(26, 0, 180, 90));
			orntList.Add(new orntData(27, 0, 180, 270));
			orntList.Add(new orntData(28, 90, 90, 0));
			orntList.Add(new orntData(29, 180, 90, 0));
			orntList.Add(new orntData(30, 270, 90, 0));
			orntList.Add(new orntData(31, 90, 180, 0));
			orntList.Add(new orntData(32, 270, 180, 0));
			orntList.Add(new orntData(33, 90, 270, 0));
			orntList.Add(new orntData(34, 180, 270, 0));
			orntList.Add(new orntData(35, 270, 270, 0));
			orntList.Add(new orntData(36, 90, 180, 90));
			orntList.Add(new orntData(37, 90, 0, 270));
			orntList.Add(new orntData(38, 90, 68, 293));
			orntList.Add(new orntData(39, 0, 315, 0));
			orntList.Add(new orntData(40, 90, 315, 0));
		}

		private void orntButton_Click(object sender, RoutedEventArgs e)
		{
			if((string)orntButton.Content=="设置")
			{
				if(!linkAvilable)
				{
					showMessageBox("无可用连接！");
					return;
				}
				orntButton.Content = "保存";
				orntYawBox.IsEnabled = true;
				orntPitchBox.IsEnabled = true;
				orntRollBox.IsEnabled = true;
			}
			else
			{
				int pos;
				for (pos = 0; pos < orntList.Count; pos++)
				{
					if (orntList[pos].Roll.ToString() == (string)orntRollBox.SelectedValue)
					{
						if (orntList[pos].Pitch.ToString() == (string)orntPitchBox.SelectedValue)
						{
							if (orntList[pos].Yaw.ToString() == (string)orntYawBox.SelectedValue)
							{
								break;
							}
						}
					}
				}
				if(pos==orntList.Count)
				{
					showMessageBox("安装角度不合法！");
					return;
				}
				Parameter p = currentVehicle.ParameterDictionary["SENS_BOARD_ROT"];
				p.SetValue(pos);
				p.Unsave = true;
				Thread t = new Thread(new ParameterizedThreadStart(sendBoardRotation));
				t.Start(pos);
			}
		}

		private void requestParam()
		{
			essentialParamList = new List<string>();
			essentialParamList.Add("SENS_BOARD_ROT");
			Thread getParamThread = new Thread(getParamWorker);
			getParamThread.IsBackground = true;
			getParamThread.Start();
		}

		private void sendBoardRotation(object o)
		{
			int pos = (int)o;
			MAVLinkPackage pkg = new MAVLinkPackage((int)MAVLINK_MSG_ID.PARAM_SET, currentVehicle.Link);
			pkg.System = 255;
			pkg.Component = 1;
			pkg.AddData(pos);
			pkg.AddData((byte)currentVehicle.ID);
			pkg.AddData((byte)0);
			pkg.AddASCIIString("SENS_BOARD_ROT", 16);
			pkg.AddData((byte)MAVLink.MAV_PARAM_TYPE.INT32);
			pkg.SetVerify();
			currentVehicle.Link.SendPackage(pkg);
			DateTime time = DateTime.Now;
			int retry = 0;
			while (true)
			{
				Thread.Sleep(50);
				if (!currentVehicle.ParameterDictionary["SENS_BOARD_ROT"].Unsave)
				{
					break;
				}
				if (DateTime.Now.Subtract(time).TotalMilliseconds > 1000)
				{
					retry++;
					if (retry > 3)
						break;
					currentVehicle.Link.SendPackage(pkg);
					time = DateTime.Now;
				}
			}
			Action a;
			if (retry <= 3)
			{
				a = () =>
				{
					showMessageBox("设置成功！");

					orntButton.Content = "设置";
					orntYawBox.IsEnabled = false;
					orntPitchBox.IsEnabled = false;
					orntRollBox.IsEnabled = false;
				};
			}
			else
			{
				a = () =>
				  {
					  showMessageBox("设置失败！");
				  };
			}
			Dispatcher.BeginInvoke(a);
		}

		private void getParamWorker()
		{
			DateTime time;
			int retry;
			while (currentVehicle.ID == -1) ;
			for(int i=0;i<essentialParamList.Count;i++)
			{
				MAVLinkPackage p = new MAVLinkPackage((int)MAVLINK_MSG_ID.PARAM_REQUEST_READ, currentVehicle.Link);
				p.System = 255;
				p.Component = 1;
				p.AddData((short)-1);
				p.AddData((byte)currentVehicle.ID);
				p.AddData((byte)0);
				p.AddASCIIString(essentialParamList[i], 16);
				p.SetVerify();
				currentVehicle.Link.SendPackage(p);
				Debug.WriteLine("[Main]:Request param {0}", essentialParamList[i]);
				time = DateTime.Now;
				retry = 0;
				while(true)
				{
					Thread.Sleep(50);
					if(currentVehicle.ParameterDictionary.ContainsKey(essentialParamList[i]))
					{
						Debug.WriteLine("[Main]:Get param {0}", essentialParamList[i]);
						break;
					}
					if(DateTime.Now.Subtract(time).TotalMilliseconds>1000)
					{
						retry++;
						Debug.WriteLine("[Main]:Timeout,retry {0}", retry);
						currentVehicle.Link.SendPackage(p);
						time = DateTime.Now;
					}
				}
			}
			int rot=0;
			currentVehicle.ParameterDictionary["SENS_BOARD_ROT"].GetValue(ref rot);
			Action a = () =>
			{

				orntRollBox.SelectedIndex = orntList[rot].Roll / 45;
				orntPitchBox.SelectedIndex = orntList[rot].Pitch / 45;
				orntYawBox.SelectedIndex = orntList[rot].Yaw / 45;
			};
			Dispatcher.BeginInvoke(a);

		}

		struct orntData
		{
			public int id;
			public int Roll;
			public int Pitch;
			public int Yaw;
			public orntData(int i,int r,int p,int y)
			{
				id = i;
				Roll = r;
				Pitch = p;
				Yaw = y;
			}
		}
	}


}
