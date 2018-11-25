﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.ComponentModel;
using System.Timers;
using System.Diagnostics;

using Timer = System.Timers.Timer;

namespace SharpBladeGroundStation.CommunicationLink
{
	public class LogLink : CommLink
	{
		string path;
		string filename;
		FileStream stream;
		byte[] buffer;

		LogReplayState replayState;


		LinkPackage currentPackage;
		double currentPackageTime;
		LinkPackageDirection currPkgDir;
		DateTime openTime;
		bool timerRunning;

		Timer backgroundTimer;
		DateTime lastUpdateTime;

		double fullTime;
		double currentTime;
		double speed;

		List<Tuple<double, long>> logMetadata;

		public string Path
		{
			get { return path; }
		}

		public string Filename
		{
			get { return filename; }
		}

		public override string LinkName
		{
			get { return "Replay"; }
		}



		/// <summary>
		/// 回放的总时间，ms
		/// </summary>
		public double FullTime
		{
			get { return fullTime; }
			set
			{
				fullTime = value;
				PropertyChangedEvent(this, "FullTime");
			}
		}

		/// <summary>
		/// 目前回放的时间进度，ms
		/// </summary>
		public double CurrentTime
		{
			get { return currentTime; }
			private set
			{
				double t = value > fullTime ? fullTime : value;
				currentTime = t;
				PropertyChangedEvent(this, "CurrentTime");
			}
		}

		/// <summary>
		/// 速度倍数
		/// </summary>
		public double Speed
		{
			get { return speed; }
			set
			{
				speed = value;
				PropertyChangedEvent(this, "Speed");
			}
		}

		public LogReplayState ReplayState
		{
			get { return replayState; }
			set
			{
				replayState = value;
				PropertyChangedEvent(this, "ReplayState");
			}
		}

		/// <summary>
		/// 构造函数
		/// </summary>
		public LogLink() : base()
		{
			buffer = new byte[16384];
			backgroundTimer = new Timer(20);
			backgroundTimer.Elapsed += BackgroundTimer_Elapsed;
			lastUpdateTime = DateTime.MinValue;
			replayState = LogReplayState.NoFile;
			timerRunning = false;
			replayState = LogReplayState.NoFile;
			speed = 1;
		}

		/// <summary>
		/// 手动初始化为初始状态
		/// </summary>
		public void Initialize()
		{
			ReplayState = LogReplayState.NoFile;
			CurrentTime = 0;
			FullTime = 1;
			Speed = 1;
		}

		private void BackgroundTimer_Elapsed(object sender, ElapsedEventArgs e)
		{
			if (!timerRunning)
				return;
			if (replayState == LogReplayState.TempPause)
			{
				lastUpdateTime = e.SignalTime;
				return;
			}
			if (lastUpdateTime == DateTime.MinValue)
			{
				lastUpdateTime = e.SignalTime;
				return;
			}
			TimeSpan dt = e.SignalTime.Subtract(lastUpdateTime);
			//if(dt.TotalMilliseconds!=0)
			//{
			//	Debug.WriteLine("[LogLink]" + dt.TotalMilliseconds.ToString()+" "+speed.ToString());
			//}
			lastUpdateTime = e.SignalTime;
			CurrentTime += dt.TotalMilliseconds * speed;
			lock (stream)
			{

				bool res = getPackages();
				if (!res)
					ReplayState = LogReplayState.Stop;
			}
			if (ReplayState == LogReplayState.Stop || ReplayState == LogReplayState.Pause)
			{
				timerRunning = false;
				backgroundTimer.Stop();
			}
		}

		private bool getPackages()
		{
			bool res = true;
			while (res)
			{
				if (currentPackageTime < 0)
				{
					res = readPackage();
					continue;
				}
				if (currentTime >= currentPackageTime)
				{
					if (currPkgDir == LinkPackageDirection.ToGCS)
					{
						if (currentPackage == null)
						{

						}
						lock (ReceivedPackageQueue)
						{
							ReceivedPackageQueue.Enqueue(currentPackage.Clone());
						}
						OnReceivePackageEvent(this, new LinkEventArgs(receivePackage));
					}
					res = readPackage();
				}
				else
					break;
			}
			return res;
		}

		public override void OpenLink()
		{
			if (replayState != LogReplayState.NoFile)
			{
				openTime = DateTime.Now;
				currentPackageTime = -1;
			}
		}

		public override void CloseLink()
		{
			if (replayState == LogReplayState.Stop)
			{
				ReplayState = LogReplayState.NoFile;
				stream.Close();
			}
		}

		public void Play()
		{
			if (replayState == LogReplayState.Pause)
			{
				ReplayState = LogReplayState.Playing;
				timerRunning = true;
				lastUpdateTime = DateTime.MinValue;
				backgroundTimer.Start();
			}
			if (replayState == LogReplayState.Stop)
			{
				ReplayState = LogReplayState.Playing;
				stream.Position = logMetadata[0].Item2;
				CurrentTime = 0;
				currentPackageTime = -1;
				lastUpdateTime = DateTime.MinValue;
				timerRunning = true;
				backgroundTimer.Start();
			}
		}

		public void Pause()
		{
			if (replayState == LogReplayState.Playing)
			{
				ReplayState = LogReplayState.Pause;
				timerRunning = false;
				backgroundTimer.Stop();
			}
		}

		public void Stop()
		{
			if (replayState != LogReplayState.NoFile)
			{
				ReplayState = LogReplayState.Stop;
				timerRunning = false;
				backgroundTimer.Stop();
				CurrentTime = fullTime;
			}
		}

		public void SetProgress(double time)
		{
			if (CurrentTime > time)
			{
				stream.Position = logMetadata[0].Item2;
				currentPackageTime = -1;
			}
			CurrentTime = time;

		}

		public LoadFileResualt OpenFile(string p)
		{
			FileInfo fi = new FileInfo(p);
			if (!fi.Exists)
				return LoadFileResualt.NotExist;
			stream = new FileStream(p, FileMode.Open);
			stream.Read(buffer, 0, CommLogger.FileHeaderSize);
			byte[] header = { 83, 66, 76, 79, 71 };
			for (int i = 0; i < 5; i++)
			{
				if (buffer[i] != header[i])
					return LoadFileResualt.UnkonwType;
			}

			protocol = (LinkProtocol)buffer[5];
			long time = BitConverter.ToInt64(buffer, 6);
			connectTime = DateTime.FromBinary(time);
			path = p;
			filename = fi.Name;
			switch (protocol)
			{				
				case LinkProtocol.MAVLink:
					currentPackage = new MAVLinkPackage();
					break;
				case LinkProtocol.MAVLink2:
					currentPackage = new MAVLinkPackage();
					break;
				default:
					currentPackage = new LinkPackage(2048);
					break;
			}
			logMetadata = new List<Tuple<double, long>>();
			long pos;
			double ts = 0;
			pos = stream.Position;
			while (readPackage())
			{
				ts = currentPackageTime;
				logMetadata.Add(new Tuple<double, long>(ts, pos));
				pos = stream.Position;
			}
			stream.Position = logMetadata[0].Item2;
			ReplayState = LogReplayState.Pause;
			FullTime = ts;
			CurrentTime = 0;
			currentPackageTime = -1;
			lastUpdateTime = DateTime.MinValue;
			return LoadFileResualt.OK;
		}

		private bool readPackage()
		{
			long rem = stream.Length - stream.Position;
			if (rem < CommLogger.PackageHeaderSize)
				return false;
			stream.Read(buffer, 0, CommLogger.PackageHeaderSize);
			int len = BitConverter.ToInt32(buffer, 1);
			rem = stream.Length - stream.Position;
			if (rem < len)
				return false;
			currentPackageTime = BitConverter.ToDouble(buffer, 5);
			currPkgDir = (LinkPackageDirection)buffer[13];
			len -= CommLogger.PackageHeaderSize;
			stream.Read(buffer, 0, len);			
			PackageParseResult res = currentPackage.ReadFromBufferWithoutCheck(buffer, len, 0);
			//if (res != PackageParseResult.Yes)
			//{

			//}
           
			return true;
		}

		private int findPackage(double time)
		{
			int l = 0, r = logMetadata.Count - 1;
			int mid;
			while (l < r)
			{
				mid = (l + r) / 2;
				if (logMetadata[mid].Item1 < time)
					l = mid + 1;
				else
					r = mid;
			}
			return l;
		}
	}

}
