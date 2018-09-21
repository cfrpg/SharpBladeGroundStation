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
using System.Threading;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Diagnostics;
using System.ComponentModel;

using AForge.Video;
using AForge.Video.DirectShow;
using Geb.Video.FFMPEG;

namespace SharpBladeGroundStation
{
	/// <summary>
	/// CameraPlayer.xaml 的交互逻辑
	/// </summary>
	public partial class CameraPlayer : UserControl
	{
		VideoCaptureDevice localWebCam;
		VideoFileWriter writer;
		string videoPath;

		bool videoLoggerEnabled;

		bool frameLock;
		bool writerLock;
		Queue<Bitmap> frameQueue;
		Thread background;

		public CameraPlayer()
		{
			InitializeComponent();
			if (DesignerProperties.GetIsInDesignMode(this))
				return;
			writer = new VideoFileWriter();
			frameLock = false;
			writerLock = false;
			frameQueue = new Queue<Bitmap>();
			background = new Thread(backgroundWorker);
			background.IsBackground = true;

		}
		void backgroundWorker()
		{
			while (true)
			{
				if (!writer.IsOpen)
				{
					Thread.Sleep(50);
					continue;
				}
				Bitmap frame = null;
				bool flag = false;
				writerLock = true;
				lock (frameQueue)
				{
					if (frameQueue.Count > 0)
					{
						frame = frameQueue.Dequeue();
						flag = true;
					}
				}
				if (flag)
				{
					writer.WriteVideoFrame(new Geb.Image.ImageRgb24(frame));
					frame.Dispose();
				}
				writerLock = false;
			}
		}

		public bool StartRecord(string path)
		{
			videoPath = path;
			if (writer.IsOpen)
			{
				writer.Close();
			}
			if (localWebCam == null || !localWebCam.IsRunning)
				return false;

			background.Start();
			videoLoggerEnabled = true;
			return true;
		}

		public bool OpenCamera(string cam)
		{
			if (localWebCam != null && localWebCam.IsRunning)
			{
				localWebCam.Stop();
				localWebCam.WaitForStop();
			}
			localWebCam = new VideoCaptureDevice(cam);
			localWebCam.NewFrame += LocalWebCam_NewFrame;
			localWebCam.Start();

			return true;
		}

		public void Close()
		{
			if (localWebCam != null && localWebCam.IsRunning)
			{
				localWebCam.Stop();
				localWebCam.WaitForStop();
			}
			int a = 1;
			while (a != 0)
			{
				lock (frameQueue)
				{
					a = frameQueue.Count;
				}
			}
			while (writerLock) ;
			if (writer != null && writer.IsOpen)
			{
				writer.Close();
			}
		}

		public bool IsCameraRunning()
		{
			return localWebCam != null && localWebCam.IsRunning;
		}

		private void LocalWebCam_NewFrame(object sender, NewFrameEventArgs eventArgs)
		{
			if (frameLock)
			{
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
				lock (frameQueue)
				{
					frameQueue.Enqueue(b);
				}
				sw.Stop();
				//Debug.WriteLine("[VideoLogger]:" + sw.Elapsed.TotalMilliseconds.ToString());
			}
			frameLock = false;
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
	}
}
