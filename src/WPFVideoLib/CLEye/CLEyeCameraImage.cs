using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Diagnostics;
using System.Windows.Data;
using System.Windows.Controls;

namespace WPFVideoLib.CLEye
{
	public class CLEyeCameraImage : Image, IDisposable
	{
		public CLEyeCameraDevice Device { get; private set; }

		public CLEyeCameraImage()
		{
			Device = new CLEyeCameraDevice();
			Device.BitmapReady += OnBitmapReady;
		}

		~CLEyeCameraImage()
		{
			// Finalizer calls Dispose(false)
			Dispose(false);
		}

		void OnBitmapReady(object sender, EventArgs e)
		{
			Source = Device.BitmapSource;
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		protected void Dispose(bool disposing)
		{
			if (disposing)
			{
				// free managed resources
				if (Device != null)
				{
					Device.Stop();
					Device.Dispose();
					Device = null;
				}
			}
			// free native resources if there are any.
		}

		#region [ Dependency Properties ]
		public float Framerate
		{
			get { return (float)GetValue(FramerateProperty); }
			set { SetValue(FramerateProperty, value); }
		}
		public static readonly DependencyProperty FramerateProperty =
			DependencyProperty.Register("Framerate", typeof(float), typeof(CLEyeCameraImage),
			new UIPropertyMetadata((float)15, (PropertyChangedCallback)delegate (DependencyObject sender, DependencyPropertyChangedEventArgs e)
			{
				CLEyeCameraImage typedSender = sender as CLEyeCameraImage;
				typedSender.Device.Framerate = (float)e.NewValue;
			}));

		public CLEyeCameraColorMode ColorMode
		{
			get { return (CLEyeCameraColorMode)GetValue(ColorModeProperty); }
			set { SetValue(ColorModeProperty, value); }
		}
		public static readonly DependencyProperty ColorModeProperty =
			DependencyProperty.Register("ColorMode", typeof(CLEyeCameraColorMode), typeof(CLEyeCameraImage),
			new UIPropertyMetadata(default(CLEyeCameraColorMode), (PropertyChangedCallback)delegate (DependencyObject sender, DependencyPropertyChangedEventArgs e)
			{
				CLEyeCameraImage typedSender = sender as CLEyeCameraImage;
				typedSender.Device.ColorMode = (CLEyeCameraColorMode)e.NewValue;
			}));

		public CLEyeCameraResolution Resolution
		{
			get { return (CLEyeCameraResolution)GetValue(ResolutionProperty); }
			set { SetValue(ResolutionProperty, value); }
		}
		public static readonly DependencyProperty ResolutionProperty =
			DependencyProperty.Register("Resolution", typeof(CLEyeCameraResolution), typeof(CLEyeCameraImage),
			new UIPropertyMetadata(default(CLEyeCameraResolution), (PropertyChangedCallback)delegate (DependencyObject sender, DependencyPropertyChangedEventArgs e)
			{
				CLEyeCameraImage typedSender = sender as CLEyeCameraImage;
				typedSender.Device.Resolution = (CLEyeCameraResolution)e.NewValue;
			}));
		#endregion
	}
}
