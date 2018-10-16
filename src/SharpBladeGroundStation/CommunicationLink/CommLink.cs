using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.ComponentModel;

namespace SharpBladeGroundStation.CommunicationLink
{
	public class CommLink : INotifyPropertyChanged
	{
		protected ConcurrentQueue<LinkPackage> receivedPackageQueue;
		protected LinkPackage receivePackage;
		protected Queue<LinkPackage> sendPackageQueue;
		protected int txRate;
		protected int rxRate;
		protected int dataReceived;
		protected int dataSent;
		protected LinkState state;
		protected LinkProtocol protocol;
		protected DateTime connectTime;
		protected bool isSending;	

		public event PropertyChangedEventHandler PropertyChanged;

		public delegate void PackageEvent(CommLink sender, LinkEventArgs e);
		public event PackageEvent OnReceivePackage;
		public event PackageEvent OnSendPackage;

		public ConcurrentQueue<LinkPackage> ReceivedPackageQueue
		{
			get { return receivedPackageQueue; }
			set { receivedPackageQueue = value; }
		}

		public LinkPackage ReceivePackage
		{
			get { return receivePackage; }
			set { receivePackage = value; }
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

		public int DataReceived
		{
			get { return dataReceived; }
		}

		public int DataSent
		{
			get { return dataSent; }
		}

		public LinkProtocol Protocol
		{
			get { return protocol; }
			set { protocol = value; }
		}

		public LinkState State
		{
			get { return state; }
			set { state = value; }
		}

		public virtual string LinkName
		{
			get { return "NoLink"; }
		}

		public DateTime ConnectTime
		{
			get { return connectTime; }
		}

		public CommLink(LinkProtocol p)
		{
			protocol = p;			
			dataReceived = 0;
			dataSent = 0;
			TxRate = 0;
			RxRate = 0;
			state = LinkState.Disconnected;
			isSending = false;
			receivedPackageQueue = new ConcurrentQueue<LinkPackage>();
			sendPackageQueue = new Queue<LinkPackage>();
			switch (p)
			{				
				case LinkProtocol.MAVLink:
					receivePackage = new MAVLinkPackage();
					break;
				case LinkProtocol.MAVLink2:
					receivePackage = new MAVLinkPackage();
					break;
				default:
					receivePackage = new LinkPackage(2048);
					break;
			}
		}

		public CommLink() : this(LinkProtocol.NoLink)
		{

		}

		protected void OnReceivePackageEvent(CommLink sender, LinkEventArgs e)
		{
			OnReceivePackage?.Invoke(this, e);
		}

		protected void OnSendPackageEvent(CommLink sender, LinkEventArgs e)
		{
			OnSendPackage?.Invoke(this, e);
		}

		protected void PropertyChangedEvent(Object sender, string name)
		{
			this.PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs(name));
		}

		public virtual void OpenLink()
		{

		}

		public virtual void CloseLink()
		{

		}

		/// <summary>
		/// 直接发送一个数据包
		/// </summary>
		/// <param name="package">要发送的数据包</param>
		/// <param name="wait">是否允许等待发送</param>
		/// <returns></returns>
		public virtual bool SendPackage(LinkPackage package,bool wait)
		{
			if(isSending)
			{
				if (wait)
					while (isSending) ;
				else
					return false;
			}
			return true;
		}

		/// <summary>
		/// 直接发送一个数据包
		/// </summary>
		/// <param name="package">要发送的数据包</param>
		/// <returns></returns>
		public virtual bool SendPackage(LinkPackage package)
		{
			return SendPackage(package, true);
		}
	}

	public class LinkEventArgs : EventArgs
	{
		public List<LinkPackage> Package { get; set; }

		public LinkEventArgs()
		{
			Package = new List<LinkPackage>();
		}

		public LinkEventArgs(LinkPackage p)
		{
			Package = new List<LinkPackage>();
			Package.Add(p);
		}
	}
}
