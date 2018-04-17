using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;

namespace SharpBladeGroundStation.CommunicationLink
{
	public class CommLogger
	{
		string path;
		FileStream fileStream;
		Queue<Tuple<LinkPackage, LinkPackageDirection>> packageQueue;
		DateTime startTime;
		CommLink link;
		

		Thread backgroundWorker;
		bool isStarted;
		bool isEnded;

		public string Path
		{
			get { return path; }
		}	

		public bool IsStarted
		{
			get { return isStarted; }
		}

		public bool IsEnded
		{
			get { return isEnded; }
		}

		public DateTime StartTime
		{
			get { return startTime; }
		}

		/// <summary>
		/// 构造函数
		/// </summary>
		/// <param name="p">日志路径</param>
		/// <param name="cl">需要记录的连接</param>
		public CommLogger(string p,CommLink cl)
		{
			path = p;
			packageQueue = new Queue<Tuple<LinkPackage, LinkPackageDirection>>();
			backgroundWorker = new Thread(worker);
			backgroundWorker.IsBackground = true;
			isStarted = false;
			isEnded = false;
			link = cl;
			link.OnReceivePackage += Link_OnReceivePackage;
			link.OnSendPackage += Link_OnSendPackage;


		}

		private void Link_OnSendPackage(CommLink sender, LinkEventArgs e)
		{
			LogPackage(e.Package, LinkPackageDirection.ToUAV);
		}

		private void Link_OnReceivePackage(CommLink sender, LinkEventArgs e)
		{
			LogPackage(e.Package, LinkPackageDirection.ToGCS);
		}

		/// <summary>
		/// 开始记录
		/// </summary>
		/// <param name="starttime">timeStamp=0的时间</param>
		public void Start(DateTime starttime)
		{
			startTime = starttime;
			FileInfo fi = new FileInfo(path);
			if(fi.Exists)
			{
				string ext = fi.Extension;
				string name=path.Substring(0,path.Length-ext.Length);
				int cnt = 1;
				while(true)
				{
					string p = name + "_" + cnt.ToString() + ext;
					fi = new FileInfo(p);
					if(!fi.Exists)
					{
						path = p;
						break;
					}
					cnt++;
				}
			}
			fileStream = new FileStream(path, FileMode.Create);
			fileStream.Write(getHeader(), 0, FileHeaderSize);
			isStarted = true;
			backgroundWorker.Start();
		}

		/// <summary>
		/// 停止记录
		/// </summary>
		public void End()
		{			
			isStarted = false;
			isEnded = true;
		}

		/// <summary>
		/// 记录一个package,并不自带clone
		/// </summary>
		/// <param name="p"></param>
		/// <param name="f">发包方向，true为接收到包,false为发送出的包</param>
		public void LogPackage(LinkPackage p,LinkPackageDirection d)
		{
			if (!isStarted)
				return;
			if (isEnded)
				return;
			lock (packageQueue)
			{
				packageQueue.Enqueue(new Tuple<LinkPackage, LinkPackageDirection>(p, d));
			}
		}

		private void worker()
		{
			int len;
			bool flag;
			Tuple<LinkPackage, LinkPackageDirection> p=new Tuple<LinkPackage, LinkPackageDirection>(null,LinkPackageDirection.ToGCS);
			while (true)
			{
				flag = false;
				lock (packageQueue)
				{
					if (packageQueue.Count != 0)
					{
						p = packageQueue.Dequeue();
						flag = true;
					}
				}
				if(flag)
				{
					var b = getBytes(p.Item1, p.Item2, out len);
					fileStream.Write(b, 0, len);
				}
				else
				{
					Thread.Sleep(50);
				}
				
				if (isEnded && packageQueue.Count == 0)
				{
					fileStream.Flush();
					fileStream.Close();
					break;
				}
			}
		}

		//len			int32	0-3
		//timestamp		double	4-11
		//direction		byte	12
		//package		byte[]	13-n
		private byte[] getBytes(LinkPackage p, LinkPackageDirection d,out int len)
		{
			len = p.PackageSize + PackageHeaderSize;
			byte[] buff = new byte[len];
			BitConverter.GetBytes(len).CopyTo(buff, 0);
			BitConverter.GetBytes(p.TimeStamp).CopyTo(buff, 4);
			buff[12] = (byte)d;
			Array.Copy(p.Buffer, 0, buff, 13, p.PackageSize);
			return buff;
		}

		//"SBLOG"	byte[]	0-4
		//protocol	byte	5
		//Time		int64	6-13
		private byte[] getHeader()
		{
			byte[] buff = new byte[FileHeaderSize];
			buff[0] = 83;//'S'
			buff[1] = 66;//'B'
			buff[2] = 76;//'L'
			buff[3] = 79;//'O'
			buff[4] = 71;//'G'
			buff[5] = (byte)link.Protocol;
			BitConverter.GetBytes(startTime.ToBinary()).CopyTo(buff, 6);
			return buff;
		}

		public static int FileHeaderSize
		{
			get { return 14; }
		}

		public static int PackageHeaderSize
		{
			get { return 13; }
		}

	}
}
