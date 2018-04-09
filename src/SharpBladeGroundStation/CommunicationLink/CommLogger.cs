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
		Queue<Tuple<LinkPackage,bool>> packageQueue;
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
			packageQueue = new Queue<Tuple<LinkPackage, bool>>();
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
			LogPackage(e.Package, false);
		}

		private void Link_OnReceivePackage(CommLink sender, LinkEventArgs e)
		{
			LogPackage(e.Package, true);
		}

		/// <summary>
		/// 开始记录
		/// </summary>
		/// <param name="starttime">timeStamp=0的时间</param>
		public void Start(DateTime starttime)
		{
			startTime = starttime;
			fileStream = new FileStream(path, FileMode.Create);
			fileStream.Write(getHeader(), 0, 13);
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
		public void LogPackage(LinkPackage p,bool f)
		{
			if (!isStarted)
				return;
			if (isEnded)
				return;
			packageQueue.Enqueue(new Tuple<LinkPackage, bool>(p,f));
		}

		private void worker()
		{
			int len;
			while(true)
			{
				if (packageQueue.Count != 0)
				{
					var p = packageQueue.Dequeue();
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
		//protocol		byte	12
		//direction		byte	13
		//package		byte[]	14-n
		private byte[] getBytes(LinkPackage p,bool f,out int len)
		{
			len = p.PackageSize + sizeof(int) + sizeof(double) + 2 * sizeof(byte);
			byte[] buff = new byte[len];
			BitConverter.GetBytes(len).CopyTo(buff, 0);
			BitConverter.GetBytes(p.TimeStamp).CopyTo(buff, 4);
			buff[12] = (byte)p.Protocol;
			buff[13] = f ? (byte)1 : (byte)0;
			Array.Copy(p.Buffer, 0, buff, 14, p.PackageSize);
			return buff;
		}

		//"SBLOG"	byte[]	0-4
		//Time		int64	5-12
		private byte[] getHeader()
		{
			byte[] buff = new byte[13];
			buff[0] = 123;//'S'
			buff[1] = 102;//'B'
			buff[2] = 114;//'L'
			buff[3] = 117;//'O'
			buff[4] = 107;//'G'
			BitConverter.GetBytes(startTime.ToBinary()).CopyTo(buff, 5);
			return buff;
		}
	}
}
