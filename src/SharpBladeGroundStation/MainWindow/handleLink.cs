using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using System.Threading;
using System.Diagnostics;
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

		void initLinkListener()
		{
			portscanner = new AdvancedPortScanner(GCSconfig.BaudRate, 1000, 3);
			portscanner.OnFindPort += Portscanner_OnFindPort;
			//portscanner.Start();
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
		}

		private void openLogBtn_Click(object sender, RoutedEventArgs e)
		{
			System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();
			ofd.InitialDirectory = GCSconfig.LogPath;
			ofd.Filter = "日志文件 (*.sblog)|*.sblog|All files (*.*)|*.*";
			var res = ofd.ShowDialog();
			if (res != System.Windows.Forms.DialogResult.Cancel)
			{				
				LoadFileResualt lfr = logLink.OpenFile(ofd.FileName);
				if (lfr == LoadFileResualt.OK)
				{
					currentVehicle.Link = logLink;
					logPathText.Text = ofd.SafeFileName;
				}
				else
				{
					MessageBox.Show(lfr.ToString(), "orz");
				}
			}
		}

		private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if (logLink.ReplayState==LogReplayState.TempPause)
			{
				if(e.NewValue<e.OldValue)
				{
					initGraph();
				}
				logLink.SetProgress(e.NewValue);
				
			}
		}
		private void Slider_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			if(logLink.ReplayState==LogReplayState.TempPause)
			{
				logLink.ReplayState = LogReplayState.Playing;
			}
		}

		private void Slider_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if(logLink.ReplayState==LogReplayState.Playing)
			{
				logLink.ReplayState = LogReplayState.TempPause;
			}
		}
		private void StopBtn_Click(object sender, RoutedEventArgs e)
		{
			logLink.Stop();
		}

		private void PauseBtn_Click(object sender, RoutedEventArgs e)
		{
			logLink.Pause();
		}

		private void PlayBtn_Click(object sender, RoutedEventArgs e)
		{
			if(logLink.ReplayState!=LogReplayState.Pause)
			{
				initGraph();
			}
			logLink.Play();
		}
	}
}
