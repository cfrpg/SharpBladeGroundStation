﻿using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using System.Threading;
using System.Diagnostics;
using System.IO;
using System.Timers;
using GMap.NET;
using SharpBladeGroundStation.CommunicationLink;
using SharpBladeGroundStation.DataStructs;
using SharpBladeGroundStation.Map;
using System.MAVLink;
using System.Windows.Input;
using Microsoft.Xna.Framework;
using Point = System.Windows.Point;
using Timer = System.Timers.Timer;

namespace SharpBladeGroundStation
{
	public partial class MainWindow : Window
	{
		Thread linkListener;
		AdvancedPortScanner portscanner;
		CommLogger logger;
		LogLink logLink;

		//MissionSender missionSender;
		bool linkAvilable;

		bool isReplay;

		Timer heartbeatListener;
		int heartbeatCounter;

		bool[] packageFlags;
 		public LogLink LogLink
		{
			get { return logLink; }
		}

		void initLinkListener()
		{
			heartbeatListener = new Timer(1000);
			heartbeatListener.Elapsed += HeartbeatListener_Elapsed;
			heartbeatListener.Start();
			packageFlags = new bool[255];
			Array.Clear(packageFlags, 0, packageFlags.Length);
			linkAvilable = false;
			portscanner = new AdvancedPortScanner(GCSconfig.BaudRate, 1000, 3);
			portscanner.OnFindPort += Portscanner_OnFindPort;
			portscanner.Start();
			linkStateText.Text = "Connecting";

			linkListener = new Thread(linkListenerWorker);
			linkListener.IsBackground = true;
			linkListener.Priority = ThreadPriority.AboveNormal;
			linkListener.Start();

			logLink = new LogLink();
			logPlayerCtrl.DataContext = logLink;
			logLink.Initialize();
			isReplay = false;
			heartbeatCounter = 0;
		}

		private void HeartbeatListener_Elapsed(object sender, ElapsedEventArgs e)
		{
			if (!linkAvilable)
				return;
			MAVLinkPackage pkg = new MAVLinkPackage((int)MAVLINK_MSG_ID.HEARTBEAT, currentVehicle.Link);
			pkg.System = 255;
			pkg.Component = 1;
			pkg.AddData((int)0);
			pkg.AddData((byte)6);
			pkg.AddData((byte)0);
			pkg.AddData((byte)1);
			pkg.AddData((byte)3);
			pkg.AddData(currentVehicle.LinkVersion);
			pkg.SetVerify();
			currentVehicle.Link.SendPackage(pkg);
			heartbeatCounter += 1;
			if(heartbeatCounter>5)
			{
				currentVehicle.SubsystemStatus.Telemetey = 4;
			}
			else if(heartbeatCounter>3)
			{
				if (currentVehicle.SubsystemStatus.Telemetey < 2)
					currentVehicle.SubsystemStatus.Telemetey = 2;
			}
			else
			{
				if (currentVehicle.SubsystemStatus.Telemetey <= 2)
					currentVehicle.SubsystemStatus.Telemetey = 0;
			}
			
		}

		void linkListenerWorker()
		{
			List<LinkPackage> pkgs = new List<LinkPackage>();
			LinkPackage package;
			while (true)
			{
				while (linkAvilable && currentVehicle.Link.ReceivedPackageQueue.TryDequeue(out package))
				{
					switch (currentVehicle.Link.Protocol)
					{
						case LinkProtocol.MAVLink:
							analyzeMAVPackage(package);
							break;						
						case LinkProtocol.MAVLink2:
							analyzeMAVPackage(package);
							break;
					}
				}
			}
		}

		private void Portscanner_OnFindPort(AdvancedPortScanner sender, PortScannerEventArgs e)
		{
			Debug.WriteLine("[main] find port {0}", e.Link.Port.PortName);
			//portscanner.Stop();
			//if (currentVehicle.Link != null)
			//	return;
			//currentVehicle.Link = e.Link;
			////currentVehicle.Link.OnReceivePackage += Link_OnReceivePackage;
			//currentVehicle.Link.OpenLink();
			//Action a = () => { linkStateText.Text = currentVehicle.Link.LinkName + Environment.NewLine + currentVehicle.Link.Protocol.ToString(); linkspd.DataContext = currentVehicle.Link; };
			//linkStateText.Dispatcher.Invoke(a);
			//linkAvilable = true;

			//logger = new CommLogger(GCSconfig.LogPath + "\\" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".sblog", e.Link);
			//logger.Start(e.Link.ConnectTime);
			//if (GCSconfig.AutoRecord && hudWindow.cameraPlayer.IsCameraRunning())
			//{
			//	string str = logger.Path.Substring(0, logger.Path.LastIndexOf(".") + 1) + "mpg";
			//	hudWindow?.cameraPlayer.StartRecord(str);
			//}

			connectVehicle(e.Link); 

			//missionSender = new MissionSender(currentVehicle);
			//missionSender.OnFinished += MissionSender_OnFinished;

		}

		private void connectVehicle(CommLink link)
		{
			bool firstConnect = false;
			if(currentVehicle.Link==null||currentVehicle.Link is LogLink)
			{
				firstConnect = true;
			}
			portscanner.Stop();
			currentVehicle.Link = link;
			currentVehicle.Link.OpenLink();
			Action a = () => { linkStateText.Text = currentVehicle.Link.LinkName + Environment.NewLine + currentVehicle.Link.Protocol.ToString(); linkspd.DataContext = currentVehicle.Link; };
			linkStateText.Dispatcher.Invoke(a);
			linkAvilable = true;

			if(firstConnect)
			{
				logger = new CommLogger(GCSconfig.LogPath + "\\" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".sblog", link);
				logger.Start(link.ConnectTime);
				if (GCSconfig.AutoRecord && (hudWindow!=null && hudWindow.cameraPlayer.IsCameraRunning()))
				{
					string str = logger.Path.Substring(0, logger.Path.LastIndexOf(".") + 1) + "mpg";
					hudWindow?.cameraPlayer.StartRecord(str);
				}
				requestParam();
			}
		}

		private void disconnectVehicle()
		{
			currentVehicle.Link.CloseLink();
			linkAvilable = false;
		}
			
		private void portMenuItem_Click(object sender, RoutedEventArgs e)
		{
			portscanner.Start();
			isReplay = false;
			logCtrlBorder.Visibility = Visibility.Hidden;
		}

		private void logMenuItem_Click(object sender, RoutedEventArgs e)
		{
			portscanner.Stop();
			isReplay = true;
			logCtrlBorder.Visibility = Visibility.Visible;
			extendLogGrid();
			linkStateText.Text = "Replay";
			if(hudWindow!=null)
			{
				hudWindow.cameraPlayer.Visibility = Visibility.Hidden;
				hudWindow.logPlayer.Visibility = Visibility.Visible;
			}
			
		}

		private void openLogBtn_Click(object sender, RoutedEventArgs e)
		{
			System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();
			ofd.InitialDirectory = GCSconfig.LogPath;
			ofd.Filter = "日志文件 (*.sblog)|*.sblog|All files (*.*)|*.*";
			var res = ofd.ShowDialog();
			if (res != System.Windows.Forms.DialogResult.Cancel)
			{
				LoadFileResualt lfr = LogLink.OpenFile(ofd.FileName);
				if (lfr == LoadFileResualt.OK)
				{
					currentVehicle.Link = LogLink;
					linkAvilable = true;
					logPathText.Text = ofd.SafeFileName;
					string str = ofd.FileName.Substring(0, ofd.FileName.LastIndexOf(".") + 1) + "mpg";
					FileInfo fi = new FileInfo(str);
					if (fi.Exists)
					{
						if(hudWindow!=null)
						{
							hudWindow.logPlayer.SetLogLink(logLink);
							hudWindow.logPlayer.OpenFile(str);
						}						
						videoPathText.Text = fi.Name;
					}
				}
				else
				{
					MessageBox.Show(lfr.ToString(), "orz");
				}
			}
		}

		private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if (LogLink.ReplayState == LogReplayState.TempPause)
			{				
				LogLink.SetProgress(e.NewValue);				
				hudWindow?.logPlayer.SetProgress();
			}
		}
		private void Slider_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			if (LogLink.ReplayState == LogReplayState.TempPause)
			{
				LogLink.ReplayState = LogReplayState.Playing;
			}
		}

		private void Slider_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (LogLink.ReplayState == LogReplayState.Playing)
			{
				LogLink.ReplayState = LogReplayState.TempPause;
			}
		}
		private void StopBtn_Click(object sender, RoutedEventArgs e)
		{
			LogLink.Stop();
			hudWindow?.logPlayer.Stop();
		}

		private void PauseBtn_Click(object sender, RoutedEventArgs e)
		{
			LogLink.Pause();
		}

		private void PlayBtn_Click(object sender, RoutedEventArgs e)
		{			
			LogLink.Play();
			hudWindow?.logPlayer.Play();
		}

		private void uploadBtn_Click(object sender, RoutedEventArgs e)
		{
			//MessageBox.Show("这是没有实装的上传航线", "orz");
			if(!missionManager.StartSendMission())
			{
				MessageBox.Show("未与飞控连接或串口正忙.");
			}
		}

		private void downloadBtn_Click(object sender, RoutedEventArgs e)
		{
			//MessageBox.Show("下载航线暂未实装", "orz");
			if (!missionManager.StartReceiveMission())
			{
				MessageBox.Show("未与飞控连接或串口正忙.");
			}
		}
	}
}
