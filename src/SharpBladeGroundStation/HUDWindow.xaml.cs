﻿using System;
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
using System.Windows.Shapes;
using WPFMediaKit.DirectShow.Controls;


namespace SharpBladeGroundStation
{
	/// <summary>
	/// HUDWindow.xaml 的交互逻辑
	/// </summary>
	public partial class HUDWindow : Window
	{

		int numCameras = 0;
		public HUDWindow()
		{
			InitializeComponent();
			
		}

		MainWindow mainwin;

		public MainWindow Mainwin
		{
			get
			{
				return mainwin;
			}

			set
			{
				mainwin = value;
			}
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			pfd.DataContext = mainwin.FlightState;

			if(MultimediaUtil.VideoInputDevices.Count()>0)
			{
				cameraCaptureElement.VideoCaptureDevice = MultimediaUtil.VideoInputDevices[0];
			}
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			
		}
	}
}
