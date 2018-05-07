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
		VideoCaptureDevice localWebCam;
		

		VideoFileWriter writer;
		VideoFileReader reader;
		System.Timers.Timer timer;

		string videoPath;
		string cameraName;

		bool videoLoggerEnabled;

		bool frameLock;
		Queue<Bitmap> frameQueue;
		Thread background;

		public HUDWindow(MainWindow mw)
		{
			InitializeComponent();
            mainwin = mw;
			writer = new VideoFileWriter();
			reader = new VideoFileReader();
			frameLock = false;
			frameQueue = new Queue<Bitmap>();
			background = new Thread(backgroundWorker);
			background.IsBackground = true;
			background.Start();
        }

		MainWindow mainwin;

        public MainWindow Mainwin
        {
            get { return mainwin; }
            set { mainwin = value; }
        }

		public string VideoPath
		{
			get { return videoPath; }
			set { videoPath = value; }
		}

		public string CameraName
		{
			get { return cameraName; }
			set { cameraName = value; }
		}

		public bool VideoLoggerEnabled
		{
			get { return videoLoggerEnabled; }			
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
            navhud.Vehicle = Mainwin.CurrentVehicle;			
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			closeAllVideo();
		}

        private void Window_Initialized(object sender, EventArgs e)
        {
           
        }

		void backgroundWorker()
		{
			while(true)
			{
				if(!writer.IsOpen)
				{
					Thread.Sleep(50);
					continue;
				}
				Bitmap frame =null;
				bool flag=false;

				lock (frameQueue)
				{
					if(frameQueue.Count>0)
					{
						frame = frameQueue.Dequeue();
						flag = true;
					}
				}
				if(flag)
					writer.WriteVideoFrame(new Geb.Image.ImageRgb24( frame));
			}
		}


		public void InitVideo()
		{
			switch(mainwin.HudVideoSource)
			{
				case HUDVideoSource.NoVideo:

					break;
				case HUDVideoSource.Camera:

					break;
				case HUDVideoSource.Replay:

					break;
			}
		}

		public bool StartRecord(string path)
		{
			VideoPath = path;
			if(writer.IsOpen)
			{
				writer.Close();				
			}
			if (localWebCam == null || !localWebCam.IsRunning)
				return false;
			videoLoggerEnabled = true;
			
			return true;
		}

		public bool OpenCamera(string cam)
		{
			if(localWebCam!=null&&localWebCam.IsRunning)
			{
				localWebCam.Stop();
				localWebCam.WaitForStop();
			}
			localWebCam = new VideoCaptureDevice(cam);
			localWebCam.NewFrame += LocalWebCam_NewFrame;
			localWebCam.Start();
			return true;
		}

		private void LocalWebCam_NewFrame(object sender, NewFrameEventArgs eventArgs)
		{
			if(frameLock)
			{
				Debug.WriteLine("FrameLock!");
				return;
			}
			frameLock = true;
			Bitmap b = (Bitmap)eventArgs.Frame.Clone();
			BitmapImage bi = bitmap2BitmapImage((Bitmap)b.Clone());
			Dispatcher.BeginInvoke(new ThreadStart(delegate
			{
				frameHolder.Source = bi;
			}));
			if (videoLoggerEnabled)
			{
				Stopwatch sw = new Stopwatch();
				sw.Start();
				if (!writer.IsOpen)
				{
					writer.Open(videoPath, b.Width, b.Height, 25, VideoCodec.Default);
				}
				//writer.WriteVideoFrame(new Geb.Image.ImageRgb24(b));
				lock(frameQueue)
				{
					frameQueue.Enqueue(b);
				}
				sw.Stop();
				Debug.WriteLine("[VideoLogger]:" + sw.Elapsed.TotalMilliseconds.ToString());
			}
			frameLock = false;			
		}

		private void closeAllVideo()
		{
			if (localWebCam != null && localWebCam.IsRunning)
			{
				localWebCam.Stop();
				localWebCam.WaitForStop();
			}
			if (writer!=null&&writer.IsOpen)
			{
				writer.Close();
			}
			if(reader!=null&&reader.IsOpen)
			{
				reader.Close();
			}
			
		}

		private BitmapImage bitmap2BitmapImage(Bitmap b)
		{
			MemoryStream ms = new MemoryStream();
			b.Save(ms, ImageFormat.Bmp);
			ms.Seek(0, SeekOrigin.Begin);
			BitmapImage bi = new BitmapImage();
			bi.BeginInit();
			bi.StreamSource = ms;
			bi.EndInit();
			bi.Freeze();
			return bi;
		}		

		public bool IsCameraRunning()
		{
			return localWebCam != null && localWebCam.IsRunning;
		}
    }

	
}
