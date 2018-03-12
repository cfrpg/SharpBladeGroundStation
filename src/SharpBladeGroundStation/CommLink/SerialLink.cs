using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.ComponentModel;

namespace SharpBladeGroundStation.CommLink
{
	public class SerialLink:INotifyPropertyChanged
	{
		SerialPort port;
		LinkState state;
		LinkProtocol protocol;
		byte[] buffer;
		int bufferSize;
		int receiveTimeOut;
		int dataReceived;
		int dataSent;
		int txRate;
		int rxRate;

		Queue<LinkPackage> receivedPackageQueue;
		LinkPackage receivePackage;
		Queue<LinkPackage> sendPackageQueue;

		bool isUpdatingBuffer;
		bool isParsingBuffer;
		DateTime lastPackageTime;
		DateTime connectedTime;

		Thread backgroundListener;

		public event PropertyChangedEventHandler PropertyChanged;

		public SerialPort Port
		{
			get { return port; }
			set { port = value; }
		}

		public LinkState State
		{
			get { return state; }
			set { state = value; }
		}

		public Queue<LinkPackage> ReceivedPackageQueue
		{
			get { return receivedPackageQueue; }
			set { receivedPackageQueue = value; }
		}

		public LinkProtocol Protocol
		{
			get { return protocol; }
			set { protocol = value; }
		}

		public LinkPackage ReceivePackage
		{
			get { return receivePackage; }
			set { receivePackage = value; }
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

		public int DataReceived
		{
			get { return dataReceived; }
		}

		public int DataSent
		{
			get { return dataSent; }
		}

		public Queue<LinkPackage> SendPackageQueue
		{
			get { return sendPackageQueue; }
			set { sendPackageQueue = value; }
		}

		public int TxRate
		{
			get { return txRate; }
			set
			{
				txRate = value;
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TxRate"));
			}
		}

		public int RxRate
		{
			get { return rxRate; }
			set
			{
				rxRate = value;
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("RxRate"));
			}
		}

		/// <summary>
		/// 建立连接后经过的总毫秒数
		/// </summary>
		public double ConnectedTime
		{
			get
			{
				return DateTime.Now.Subtract(connectedTime).TotalMilliseconds;
			}
		}

		public delegate void ReceivePackageEvent(SerialLink sender, EventArgs e);

		public event ReceivePackageEvent OnReceivePackage;

		public SerialLink(string portName,LinkProtocol p)
		{
			port = new SerialPort(portName);
			port.BaudRate = 115200;
			port.DataBits = 8;
			port.StopBits = System.IO.Ports.StopBits.One;
			port.ReceivedBytesThreshold = 8;
			port.DataReceived += Port_DataReceived;
			protocol = p;
			dataReceived = 0;
			dataSent = 0;
			TxRate = 0;
			RxRate = 0;

			state = LinkState.Disconnected;
			receivedPackageQueue = new Queue<LinkPackage>();
			sendPackageQueue = new Queue<LinkPackage>();
			switch(p)
			{
				case LinkProtocol.ANOLink:
					receivePackage = new ANOLinkPackage();
					break;
				case LinkProtocol.MAVLink:
					receivePackage = new MAVLinkPackage();
					break;
				default:
					receivePackage = new LinkPackage(2048);
					break;
			}
			buffer = new byte[1048576];//1M
			isUpdatingBuffer = false;
			isParsingBuffer = false;
			lastPackageTime = DateTime.Now;
			receiveTimeOut = 5000;
			backgroundListener = new Thread(backgroundWorker);
			backgroundListener.IsBackground = true;
			backgroundListener.Start();
			connectedTime = DateTime.Now;
		}

		private void backgroundWorker()
		{
			DateTime lasttime=DateTime.Now;
			int lasttx = 0, lastrx = 0;
			while(true)
			{
				if (!port.IsOpen)
				{
					Thread.Sleep(500);
					lasttime = DateTime.Now;
					continue;
				}
				try
				{
					if (port.BytesToWrite == 0)
					{
						if (sendPackageQueue.Count != 0)
						{
							LinkPackage p = sendPackageQueue.Dequeue();
							port.Write(p.Buffer, 0, p.PackageSize);
						}
					}
					else
					{
						Thread.Sleep(50);
					}
				}
				catch
				{
					
				}
				DateTime now = DateTime.Now;
				double dt = now.Subtract(lasttime).TotalMilliseconds;
				if(dt>500)
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
			if (isParsingBuffer)
				return;
			isUpdatingBuffer = true;
			int len = port.BytesToRead;
			port.Read(buffer, bufferSize, len);
			dataReceived += len;
			bufferSize += len;
			isUpdatingBuffer = false;
			parseBuffer();
		}

		private void parseBuffer()
		{
			if (isUpdatingBuffer)
				return;
			isParsingBuffer = true;
			int offset = 0;
			bool flag = false;
			bool received = false;
			while (offset < bufferSize && (!flag))
			{
				PackageParseResult res = receivePackage.ReadFromBuffer(buffer, bufferSize, offset);
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
						break;
					case PackageParseResult.Yes:
						offset += receivePackage.PackageSize;
						receivePackage.TimeStamp = this.ConnectedTime;
						receivedPackageQueue.Enqueue(receivePackage.Clone());
						received = true;
						break;
				}
			}
			for (int i = offset; i < bufferSize; i++)
			{
				buffer[i - offset] = buffer[i];
			}
			bufferSize -= offset;
			isParsingBuffer = false;
			if (received)
			{
				OnReceivePackage?.Invoke(this, new EventArgs());
			}
		}

		public void OpenPort()
		{
			if(!port.IsOpen)
				port.Open();
			connectedTime = DateTime.Now;
		}

		public void ClosePort()
		{
			if(port.IsOpen)
				port.Close();
		}

		public void ResetLink()
		{
			if (port.IsOpen)
				port.Close();
			while (port.IsOpen||isParsingBuffer||isUpdatingBuffer) ;
			bufferSize = 0;
			dataReceived = 0;
			dataSent = 0;
			isParsingBuffer = false;
			isUpdatingBuffer = false;
			receivedPackageQueue.Clear();
		}

		
	}
}
