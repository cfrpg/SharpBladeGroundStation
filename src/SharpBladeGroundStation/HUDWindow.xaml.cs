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
using System.Threading;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Diagnostics;

using AForge.Video;
using AForge.Video.DirectShow;
using Geb.Video.FFMPEG;




namespace SharpBladeGroundStation
{
	/// <summary>
	/// HUDWindow.xaml 的交互逻辑
	/// </summary>
	public partial class HUDWindow : Window
	{		
		VideoFileReader reader;
				

		public HUDWindow(MainWindow mw)
		{
			InitializeComponent();
            mainwin = mw;			
			reader = new VideoFileReader();
		
        }

		MainWindow mainwin;

        public MainWindow Mainwin
        {
            get { return mainwin; }
            set { mainwin = value; }
        }
				
		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
            navhud.Vehicle = Mainwin.CurrentVehicle;			
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			cameraPlayer.Close();
			logPlayer.Close();
		}

        private void Window_Initialized(object sender, EventArgs e)
        {
           
        }

						
		private void Timer_Elapsed_Replay(object sender, System.Timers.ElapsedEventArgs e)
		{
			
		}

		

		

		

		

		
    }

	
}
