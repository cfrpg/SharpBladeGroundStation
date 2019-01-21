using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
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
using SharpBladeGroundStation.Configuration;
using AForge.Video.DirectShow;
using System.MAVLink;
using SharpDX.DirectInput;

namespace SharpBladeGroundStation
{
	/// <summary>
	/// MainWindow.xaml 的交互逻辑
	/// </summary>
	public partial class MainWindow : Window
	{
		Vehicle currentVehicle;
		GCSConfiguration GCSconfig;

		Dictionary<int, UInt64> dataSkipCount;	

		HUDWindow hudWindow;
		HUDVideoSource hudVideoSource;
		FilterInfoCollection localWebCamsCollection;

	
		//temps
		
		const string messageboxTitle = "SharpBladeGroundStation";

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

		public HUDVideoSource HudVideoSource
		{
			get { return hudVideoSource; }
			set { hudVideoSource = value; }
		}

		public MainWindow()
		{
			InitializeComponent();
			initControls();
			initConfig();
			currentVehicle = new Vehicle(0);
			initGmap();
			//initLinkListener();
			
			pfd.DataContext = currentVehicle.FlightState;
			//gpsData = new GPSData();
			vdopText.DataContext = currentVehicle.GpsState;
			hdopText.DataContext = currentVehicle.GpsState;
			gpsStateText.DataContext = currentVehicle.GpsState;
			battText.DataContext = currentVehicle.Battery;
			flightDataGrid.DataContext = currentVehicle.FlightState;		

			dataSkipCount = new Dictionary<int, ulong>();
			for (int i = 0; i < 255; i++)
			{
				dataSkipCount[i] = 0;
			}
		}

		private void initConfig()
		{
			string path = Environment.CurrentDirectory + "\\config";

			DirectoryInfo di = new DirectoryInfo(path);
			if (!di.Exists)
				di.Create();			
			XmlSerializer xs = new XmlSerializer(typeof(GCSConfiguration));
			Stream s = new FileStream(path + "\\gcs.xml", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
			try
			{
				GCSconfig = (GCSConfiguration)xs.Deserialize(s);
				s.Close();
			}
			catch
			{				
				s.Close();
				s = new FileStream(path + "\\gcs.xml", FileMode.Create, FileAccess.Write, FileShare.None);
				GCSconfig = GCSConfiguration.DefaultConfig();
				xs.Serialize(s, GCSConfig);
				s.Close();
			}
			di = new DirectoryInfo(GCSconfig.LogPath);
			if (!di.Exists)
			{
				di.Create();
			}
			gcsCfgGroup.DataContext = GCSconfig;
		}

		private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			leftcol.MaxWidth = Math.Min(400, (e.NewSize.Height - 30) / 669 * 300);
		}

		private void initControls()
		{
			logPlayerCtrl.playBtn.Click += PlayBtn_Click;
			logPlayerCtrl.pauseBtn.Click += PauseBtn_Click;
			logPlayerCtrl.stopBtn.Click += StopBtn_Click;
			logPlayerCtrl.slider.ValueChanged += Slider_ValueChanged;
			logPlayerCtrl.slider.PreviewMouseLeftButtonDown += Slider_PreviewMouseLeftButtonDown;
			logPlayerCtrl.slider.PreviewMouseLeftButtonUp += Slider_PreviewMouseLeftButtonUp;
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
			if (MessageBox.Show("清空所有航线?", messageboxTitle, MessageBoxButton.YesNoCancel) == MessageBoxResult.Yes)
			{
				missionManager.LocalMission.Clear();
			}
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
			hudWindow?.Close();
			logger?.End();
		}

		private void mainwindow_Loaded(object sender, RoutedEventArgs e)
		{
			string[] args = Environment.GetCommandLineArgs();
			int id;
			if (args.Contains("-nohud"))
			{				
				cameraComboBox.IsEnabled = false;	
			}
			else
			{
				hudWindow = new HUDWindow(this);
				hudWindow.Mainwin = this;
				hudWindow.Show();
				localWebCamsCollection = new FilterInfoCollection(FilterCategory.VideoInputDevice);
				id = -1;
				cameraComboBox.ItemsSource = localWebCamsCollection;
				cameraComboBox.DisplayMemberPath = "Name";
				if (localWebCamsCollection.Count > 0)
				{
					for (int i = 0; i < localWebCamsCollection.Count; i++)
					{
						if (GCSconfig.CameraName != "" && localWebCamsCollection[i].Name == GCSconfig.CameraName)
						{
							cameraComboBox.SelectedValue = localWebCamsCollection[i];
							id = i;
						}
					}
				}
				if (id >= 0)
				{
					HudVideoSource = HUDVideoSource.Camera;
					hudWindow.cameraPlayer.OpenCamera(localWebCamsCollection[id].MonikerString);
					//hudWindow.StartRecord("D:\\temp\\test.mpg");
				}
				else
				{
					HudVideoSource = HUDVideoSource.NoVideo;
				}
				cameraComboBox.SelectionChanged += CameraComboBox_SelectionChanged;
			}
			initDxInput();
			initLinkListener();
			initFirmwareUpdater();
		}

		

		private void CameraComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			string str = ((FilterInfo)(cameraComboBox.SelectedItem)).Name;
			if (str == "")
				return;
			GCSconfig.CameraName = str;
		}

		private void logpathbtn_Click(object sender, RoutedEventArgs e)
		{
			System.Windows.Forms.FolderBrowserDialog fbdig = new System.Windows.Forms.FolderBrowserDialog();
			var dr = fbdig.ShowDialog();
			if (dr != System.Windows.Forms.DialogResult.Cancel)
			{
				GCSconfig.LogPath = fbdig.SelectedPath;
			}
		}

		private void logCtrlBtn_Click(object sender, RoutedEventArgs e)
		{
			if (logCtrlGrid.Visibility == Visibility.Visible)
			{
				retractLogGrid();
			}
			else
			{
				extendLogGrid();
			}

		}

		private void extendLogGrid()
		{
			logCtrlGrid.Visibility = Visibility.Visible;
			logCtrlBtn.Content = "▼";
			logCtrlBtn.FontSize = 9;
			logCtrlBtn.Padding = new Thickness(0, -1, 0, 0);
		}

		private void retractLogGrid()
		{
			logCtrlGrid.Visibility = Visibility.Collapsed;
			logCtrlBtn.Content = "▲";
			logCtrlBtn.FontSize = 12;
			logCtrlBtn.Padding = new Thickness(0, -3, 0, 1);
		}

		private void pfd_Loaded(object sender, RoutedEventArgs e)
		{

			
		}

		private void button1_Click(object sender, RoutedEventArgs e)
		{
			if (rightCol.Width.Value != 0)
			{
				rightCol.MinWidth = 0;
				rightCol.Width = new GridLength(0, GridUnitType.Star);				
				button1.Background = new SolidColorBrush(Color.FromArgb(204, 100, 100, 100));
			}
			else
			{
				rightCol.MinWidth = 150;
				rightCol.Width = new GridLength(1, GridUnitType.Star);
				button1.Background = new SolidColorBrush(Colors.Green);
			}
			
		}

		private void mainwindow_KeyDown(object sender, KeyEventArgs e)
		{
			hudWindow?.KeyEvent(sender, e);
		}
	}
	public enum HUDVideoSource
	{
		NoVideo,
		Camera,
		Replay
	}

	
}
