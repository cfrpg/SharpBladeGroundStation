﻿using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using System.Threading;
using System.Diagnostics;
using System.IO;
using GMap.NET;
using SharpBladeGroundStation.CommunicationLink;
using SharpBladeGroundStation.DataStructs;
using SharpBladeGroundStation.Map;
using System.MAVLink;
using System.Windows.Input;
using Microsoft.Xna.Framework;
using Point = System.Windows.Point;

namespace SharpBladeGroundStation
{
	public partial class MainWindow : Window
	{
		Thread linkListener;
		AdvancedPortScanner portscanner;
		CommLogger logger;
		LogLink logLink;

		MissionSender missionSender;

		public LogLink LogLink
		{
			get { return logLink; }
		}

		void initLinkListener()
		{
			portscanner = new AdvancedPortScanner(GCSconfig.BaudRate, 1000, 3);
			portscanner.OnFindPort += Portscanner_OnFindPort;
			portscanner.Start();
			linkStateText.Text = "Connecting";

			linkListener = new Thread(linkListenerWorker);
			linkListener.IsBackground = true;
			linkListener.Start();

			logLink = new LogLink();
			logPlayerCtrl.DataContext = logLink;
			logLink.Initialize();

			
		}

		void linkListenerWorker()
		{
			while (true)
			{
				if (currentVehicle == null || currentVehicle.Link == null)
				{
					continue;
				}
				if (currentVehicle.Link.ReceivedPackageQueue.Count == 0)
				{
					continue;
				}
				if (currentVehicle.Link.ReceivedPackageQueue.Count != 0)
				{
					LinkPackage package;
					lock (currentVehicle.Link.ReceivedPackageQueue)
					{
						package = currentVehicle.Link.ReceivedPackageQueue.Dequeue();
						
					}
					//Debug.WriteLine(currentVehicle.Link.ReceivedPackageQueue.Count);
					switch (currentVehicle.Link.Protocol)
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
		}

		private void Portscanner_OnFindPort(AdvancedPortScanner sender, PortScannerEventArgs e)
		{
			Debug.WriteLine("[main] find port {0}", e.Link.Port.PortName);
			portscanner.Stop();
			if (currentVehicle.Link != null)
				return;
			currentVehicle.Link = e.Link;
			//currentVehicle.Link.OnReceivePackage += Link_OnReceivePackage;
			currentVehicle.Link.OpenLink();
			Action a = () => { linkStateText.Text = currentVehicle.Link.LinkName + Environment.NewLine + currentVehicle.Link.Protocol.ToString(); linkspd.DataContext = currentVehicle.Link; };
			linkStateText.Dispatcher.Invoke(a);

			logger = new CommLogger(GCSconfig.LogPath + "\\" + DateTime.Now.ToString("yyyy-MM-dd") + ".sblog", e.Link);
			logger.Start(e.Link.ConnectTime);
			if (hudWindow.cameraPlayer.IsCameraRunning())
			{
				string str = logger.Path.Substring(0, logger.Path.LastIndexOf(".") + 1) + "mpg";
				hudWindow.cameraPlayer.StartRecord(str);
			}

			missionSender = new MissionSender(currentVehicle);
			missionSender.OnFinished += MissionSender_OnFinished;

		}

		private void MissionSender_OnFinished()
		{
			this.Dispatcher.BeginInvoke(new ThreadStart(delegate { MessageBox.Show("上传成功！"); }));
			missionSender.State = MissionSenderState.Idle;
		}

		private void Link_OnReceivePackage(CommLink sender, EventArgs e)
		{
			while (currentVehicle.Link.ReceivedPackageQueue.Count != 0)
			{
				LinkPackage package = currentVehicle.Link.ReceivedPackageQueue.Dequeue();
				switch (sender.Protocol)
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

		private void portMenuItem_Click(object sender, RoutedEventArgs e)
		{
			portscanner.Start();
			logCtrlBorder.Visibility = Visibility.Hidden;
		}

		private void logMenuItem_Click(object sender, RoutedEventArgs e)
		{
			portscanner.Stop();
			logCtrlBorder.Visibility = Visibility.Visible;
			extendLogGrid();
			linkStateText.Text = "Replay";
			hudWindow.cameraPlayer.Visibility = Visibility.Hidden;
			hudWindow.logPlayer.Visibility = Visibility.Visible;
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
					logPathText.Text = ofd.SafeFileName;
					string str = ofd.FileName.Substring(0, ofd.FileName.LastIndexOf(".") + 1) + "mpg";
					FileInfo fi = new FileInfo(str);
					if(fi.Exists)
					{
						hudWindow.logPlayer.SetLogLink(logLink);
						hudWindow.logPlayer.OpenFile(str);
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
			if (LogLink.ReplayState==LogReplayState.TempPause)
			{
				if(e.NewValue<e.OldValue)
				{
					initGraph();
				}
				LogLink.SetProgress(e.NewValue);
				hudWindow.logPlayer.SetProgress();
			}
		}
		private void Slider_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			if(LogLink.ReplayState==LogReplayState.TempPause)
			{
				LogLink.ReplayState = LogReplayState.Playing;
			}
		}

		private void Slider_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if(LogLink.ReplayState==LogReplayState.Playing)
			{
				LogLink.ReplayState = LogReplayState.TempPause;
			}
		}
		private void StopBtn_Click(object sender, RoutedEventArgs e)
		{
			LogLink.Stop();
			hudWindow.logPlayer.Stop();
		}

		private void PauseBtn_Click(object sender, RoutedEventArgs e)
		{
			LogLink.Pause();
		}

		private void PlayBtn_Click(object sender, RoutedEventArgs e)
		{
			if(LogLink.ReplayState!=LogReplayState.Pause)
			{
				initGraph();
			}
			LogLink.Play();
			hudWindow.logPlayer.Play();
		}

		private void uploadBtn_Click(object sender, RoutedEventArgs e)
		{
			//MessageBox.Show("这是没有实装的上传航线", "orz");
			if (missionSender != null && missionSender.State == MissionSenderState.Idle)
			{
				missionSender.StartSendMission(newroute);
			}
			else
			{
				MessageBox.Show("未与飞控连接或线路正忙", "orz");
			}
			//MAVLinkPackage package = new MAVLinkPackage();
			//package.Sequence = 1;
			//package.System = 0;
			//package.Component = 0;
			//package.Function = (byte)MAVLINK_MSG_ID.MISSION_COUNT;
			//package.AddData((ushort)10);
			//package.AddData((byte)1);
			//package.AddData((byte)190);
			
			//package.SetVerify();
			//currentVehicle.Link.SendPackageQueue.Enqueue(package);
		}

		private void downloadBtn_Click(object sender, RoutedEventArgs e)
		{
			MessageBox.Show("下载航线暂未实装", "orz");
			//MAVLinkPackage package = new MAVLinkPackage();
			//package.Sequence = 1;
			//package.System = 255;
			//package.Component = 190;
			//package.Function = (byte)MAVLINK_MSG_ID.MISSION_REQUEST_LIST;
			//package.AddData((byte)1);
			//package.AddData((byte)190);
			//package.SetVerify();
			//currentVehicle.Link.SendPackageQueue.Enqueue(package);
		}
	}
}
