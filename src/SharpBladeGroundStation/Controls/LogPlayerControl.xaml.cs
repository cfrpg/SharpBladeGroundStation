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
using SharpBladeGroundStation.CommunicationLink;

namespace SharpBladeGroundStation
{
	/// <summary>
	/// LogPlayerControl.xaml 的交互逻辑
	/// </summary>
	public partial class LogPlayerControl : UserControl
	{
		public bool IsDragingSlider { get; set; }

		public static readonly DependencyProperty FullTimeProperty =
			DependencyProperty.Register("FullTime", typeof(double), typeof(LogPlayerControl), new PropertyMetadata(0.0, LogPlayerControl.OnFullTimePropertyChanged));
		public double FullTime
		{
			get { return (double)GetValue(FullTimeProperty); }
			set { SetValue(FullTimeProperty, value); }
		}

		public static readonly DependencyProperty CurrentTimeProperty =
			DependencyProperty.Register("CurrentTime", typeof(double), typeof(LogPlayerControl), new PropertyMetadata(0.0, LogPlayerControl.OnCurrentTimePropertyChanged));
		public double CurrentTime
		{
			get { return (double)GetValue(CurrentTimeProperty); }
			set { SetValue(CurrentTimeProperty, value); }
		}

		public static readonly DependencyProperty SpeedProperty =
			DependencyProperty.Register("Speed", typeof(double), typeof(LogPlayerControl), new PropertyMetadata(1.0, LogPlayerControl.OnSpeedPropertyChanged));
		public double Speed
		{
			get { return (double)GetValue(SpeedProperty); }
			set { SetValue(SpeedProperty, value); }
		}
		public static readonly DependencyProperty ReplayStateProperty =
			DependencyProperty.Register("ReplayState", typeof(LogReplayState), typeof(LogPlayerControl), new PropertyMetadata(LogReplayState.NoFile, LogPlayerControl.OnReplayStatePropertyChanged));
		public LogReplayState ReplayState
		{
			get { return (LogReplayState)GetValue(ReplayStateProperty); }
			set { SetValue(ReplayStateProperty, value); }
		}
		public static void OnReplayStatePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
		{
			LogPlayerControl s = sender as LogPlayerControl;
			s.OnReplayStateChanged(e);
		}
		public void OnReplayStateChanged(DependencyPropertyChangedEventArgs e)
		{
			LogReplayState s = (LogReplayState)e.NewValue;
			switch (s)
			{
				case LogReplayState.NoFile:
					pauseBtn.Visibility = Visibility.Collapsed;
					playBtn.Visibility = Visibility.Visible;
					playBtn.IsEnabled = false;
					slider.IsEnabled = false;
					break;
				case LogReplayState.Pause:
					pauseBtn.Visibility = Visibility.Collapsed;
					playBtn.Visibility = Visibility.Visible;
					playBtn.IsEnabled = true;
					slider.IsEnabled = true;
					break;
				case LogReplayState.Playing:
					pauseBtn.Visibility = Visibility.Visible;
					playBtn.Visibility = Visibility.Collapsed;
					playBtn.IsEnabled = true;
					slider.IsEnabled = true;
					break;
				case LogReplayState.Stop:
					pauseBtn.Visibility = Visibility.Collapsed;
					playBtn.Visibility = Visibility.Visible;
					playBtn.IsEnabled = true;
					slider.IsEnabled = true;
					break;
				default:
					break;
			}
		}

		public static void OnSpeedPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
		{
			LogPlayerControl s = sender as LogPlayerControl;
			s.OnSpeedChanged(e);
		}

		public void OnSpeedChanged(DependencyPropertyChangedEventArgs e)
		{
			spdText.Text = Speed.ToString("0.0") + "x";
		}

		public static void OnCurrentTimePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
		{
			LogPlayerControl s = sender as LogPlayerControl;
			s.OnCurrentTimeChanged(e);
		}
		public void OnCurrentTimeChanged(DependencyPropertyChangedEventArgs e)
		{

		}

		public static void OnFullTimePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
		{
			LogPlayerControl s = sender as LogPlayerControl;
			s.OnFullTimeChanged(e);
		}
		public void OnFullTimeChanged(DependencyPropertyChangedEventArgs e)
		{

		}

		public LogPlayerControl()
		{
			InitializeComponent();
			IsDragingSlider = false;
		}

		private void incSpdBtn_Click(object sender, RoutedEventArgs e)
		{
			double spd = Speed + 0.1;
			if (spd > 3)
				spd = 3;
			Speed = spd;
		}

		private void stopBtn_Click(object sender, RoutedEventArgs e)
		{

		}

		private void playBtn_Click(object sender, RoutedEventArgs e)
		{

		}

		private void pauseBtn_Click(object sender, RoutedEventArgs e)
		{

		}

		private void decSpdBtn_Click(object sender, RoutedEventArgs e)
		{
			double spd = Speed - 0.1;
			if (spd < 0.09)
				spd = 0.1;
			Speed = spd;
		}

		private void slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{

		}

		private void slider_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{

		}

		private void slider_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{

		}

		private void slider_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{

		}

		private void slider_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{

		}
	}
}
