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

using SharpBladeGroundStation.CommunicationLink;

namespace SharpBladeGroundStation
{
	/// <summary>
	/// VideoLogPlayer.xaml 的交互逻辑
	/// </summary>
	public partial class VideoLogPlayer : UserControl
	{
		VideoFileReader reader;
		string videoPath;
		bool frameLock;

		Thread background;

		LogLink logLink;

		State state;

		double frameTime;

		public VideoLogPlayer()
		{
			InitializeComponent();
			if (DesignerProperties.GetIsInDesignMode(this))
				return;
			reader = new VideoFileReader();
			frameLock = false;
			frameTime = 0;
			background = new Thread(backgroundWorker);
			background.IsBackground = true;
			state = State.Stop;

		}

		public void OpenFile(string path)
		{
			videoPath = path;
			if (reader.IsOpen)
				reader.Close();
			reader.Open(path);
			frameTime = 1000.0 / reader.FrameRate;
			state = State.Pause;
			background.Start();
		}

		public void Close()
		{
			state = State.Stop;
			Thread.Sleep(50);
			while (frameLock) ;
			if (reader.IsOpen)
				reader.Close();
		}

		public void Play()
		{
			if (state != State.Stop)
			{
				state = State.Play;
				SetProgress();
			}
		}

		public void Stop()
		{
			state = State.Pause;
		}

		public void SetProgress()
		{
			if (logLink == null)
				return;
			state = State.Pause;
			while (frameLock) ;
			reader.Seek((logLink.CurrentTime - frameTime * 2) / 1000 + 1);
			state = State.Play;
		}

		public void SetLogLink(LogLink ll)
		{
			logLink = ll;
		}
		private void backgroundWorker()
		{
			while (state != State.Stop)
			{
				if (state == State.Pause)
				{
					Thread.Sleep(10);
					continue;
				}
				frameLock = true;
				if (logLink.CurrentTime - frameTime >= (reader.CurrentVideoTime - 1) * 1000+150)
				{
					try
					{
						Bitmap b = reader.ReadVideoFrame().ToBitmap();
						BitmapImage bi = bitmap2BitmapImage(b);
						Dispatcher.BeginInvoke(new ThreadStart(delegate
						{
							frameHolder.Source = bi;
						}));
					}
					catch//(Exception ex)
					{
						state = State.Pause;
					}
				}
				frameLock = false;
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

		enum State
		{
			Play,
			Pause,
			Stop
		}
	}
}
