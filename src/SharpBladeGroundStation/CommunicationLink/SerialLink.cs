using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.ComponentModel;
using System.Diagnostics;

namespace SharpBladeGroundStation.CommunicationLink
{
	public class SerialLink : CommLink
	{
		SerialPort port;
		byte[] buffer;
		int bufferSize;
		int receiveTimeOut;

		bool isUpdatingBuffer;
		bool isParsingBuffer;
		DateTime lastPackageTime;

		Thread backgroundListener;
		delegate void EnqueuePackage(LinkPackage p);
		EnqueuePackage enqueueHandler;
		Stopwatch sw;

		public SerialPort Port
		{
			get { return port; }
			set { port = value; }
		}

		public int BufferSize
		{
			get { return bufferSize; }
		}

		public int ReceiveTimeOut
		{
			get { return receiveTimeOut; }
			set { receiveTimeOut = value; }
		}

		/// <summary>
		/// 建立连接后经过的总毫秒数
		/// </summary>
		public double ConnectedTime
		{
			get
			{
				return DateTime.Now.Subtract(ConnectTime).TotalMilliseconds;
			}
		}

		public override string LinkName
		{
			get
			{
				if (port != null)
					return port.PortName;
				return base.LinkName;
			}
		}

		public SerialLink(string portName, LinkProtocol p, int br) : base(p)
		{
			port = new SerialPort(portName);
			port.BaudRate = br;
			port.DataBits = 8;
			port.StopBits = StopBits.One;
			port.ReceivedBytesThreshold = 16;
			port.DataReceived += Port_DataReceived;
			buffer = new byte[1048576];//1M
			isUpdatingBuffer = false;
			isParsingBuffer = false;
			lastPackageTime = DateTime.Now;
			receiveTimeOut = 5000;
			backgroundListener = new Thread(backgroundWorker);
			backgroundListener.IsBackground = true;
			backgroundListener.Start();
			connectTime = DateTime.Now;
			sw = new Stopwatch();
			enqueueHandler = new EnqueuePackage(receivedPackageQueue.Enqueue);
		}

		private void backgroundWorker()
		{
			DateTime lasttime = DateTime.Now;
			int lasttx = 0, lastrx = 0;
			while (true)
			{
				if (!port.IsOpen)
				{
					//Thread.Sleep(500);
					//lasttime = DateTime.Now;
					continue;
				}
				//try
				//{
				//	if (port.BytesToWrite == 0)
				//	{
				//		if (sendPackageQueue.Count != 0)
				//		{
				//			LinkPackage p = sendPackageQueue.Dequeue();
				//			port.Write(p.Buffer, 0, p.PackageSize);
				//			OnSendPackageEvent(this, new LinkEventArgs(p));
				//			Debug.WriteLine("[Serial]Package sent.");
				//		}
				//	}
				//	else
				//	{
				//		Thread.Sleep(50);
				//	}
				//}
				//catch
				//{

				//}
				Thread.Sleep(1000);
				DateTime now = DateTime.Now;
				double dt = now.Subtract(lasttime).TotalMilliseconds;
				if (dt > 1000)
				{
					TxRate = (int)((dataReceived - lastrx) / (dt / 1000));
					RxRate = (int)((dataSent - lasttx) / (dt / 1000));
					lastrx = dataReceived;
					lasttx = dataSent;
					lasttime = now;
				}
			}
		}

		private void Port_DataReceived(object sender, SerialDataReceivedEventArgs e)
		{						
			if (isUpdatingBuffer || isParsingBuffer)
			{
				return;
			}
			isUpdatingBuffer = true;
			int len = port.BytesToRead;
			port.Read(buffer, bufferSize, len);
			dataReceived += len;
			bufferSize += len;
			parseBuffer();
			isUpdatingBuffer = false;			
		}

		private void parseBuffer()
		{			
			isParsingBuffer = true;
			int offset = 0;
			bool flag = false;
			bool received = false;
			LinkEventArgs lea = new LinkEventArgs();
			int dataused;			
			while (offset < bufferSize&&receivedPackageQueue.Count<6)
			{				
				PackageParseResult res = receivePackage.ReadFromBuffer(buffer, bufferSize, offset,out dataused);
				switch (res)
				{
					case PackageParseResult.NoSTX:
						offset++;
						break;
					case PackageParseResult.NoEnoughData:
						flag = true;						
						break;
					case PackageParseResult.BadCheckSum:
						offset++;						
						Debug.WriteLine("[Link]Bad checksum.");
						break;
					case PackageParseResult.Yes:
						offset += dataused;
						receivePackage.TimeStamp = this.ConnectedTime;
                        
                            MAVLinkPackage tp = new MAVLinkPackage();
                            int ti;
                            var res1 = tp.ReadFromBuffer(receivePackage.Buffer, receivePackage.PackageSize, 0, out ti);
                        if(res1!=PackageParseResult.Yes)
                          Debug.WriteLine("[SerialLink]{0} {1}",receivePackage.Function,res1);
                        
						//lock (ReceivedPackageQueue)
						{
							enqueueHandler.BeginInvoke(receivePackage.Clone(), null, null);
							//receivedPackageQueue.Enqueue(receivePackage.Clone());
						}
						received = true;
						lea.Package.Add(receivePackage.Clone());
						break;
				}
				if (flag)
					break;
			}

			for (int i = offset; i < bufferSize; i++)
			{
				buffer[i - offset] = buffer[i];
			}
			bufferSize -= offset;
			isParsingBuffer = false;
			if (received)
			{				
				OnReceivePackageEvent(this, lea);
			}
		}

		public void OpenPort()
		{
			if (!port.IsOpen)
				port.Open();
			connectTime = DateTime.Now;
		}

		public void ClosePort()
		{
			if (port.IsOpen)
				port.Close();
		}

		public override void OpenLink()
		{
			OpenPort();
		}

		public override void CloseLink()
		{
			ClosePort();
		}

		public void ResetLink()
		{
			if (port.IsOpen)
				port.Close();
			while (port.IsOpen || isParsingBuffer || isUpdatingBuffer) ;
			bufferSize = 0;
			dataReceived = 0;
			dataSent = 0;
			isParsingBuffer = false;
			isUpdatingBuffer = false;
			LinkPackage p;
			while (receivedPackageQueue.TryDequeue(out p)) ;
		}

		public override bool SendPackage(LinkPackage package, bool wait)
		{
			if (isSending)
			{
				if (wait)
					while (isSending) ;
				else
					return false;
			}
			isSending = true;
			port.Write(package.Buffer, 0, package.PackageSize);
			OnSendPackageEvent(this, new LinkEventArgs(package));
			isSending = false;
			return true;
		}
	}
}
