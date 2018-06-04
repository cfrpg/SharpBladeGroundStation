using System;
using System.Collections.Generic;
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
		protected Queue<LinkPackage> receivedPackageQueue;
		protected LinkPackage receivePackage;
		protected Queue<LinkPackage> sendPackageQueue;
		protected int txRate;
		protected int rxRate;
		protected int dataReceived;
		protected int dataSent;
		protected LinkState state;
		protected LinkProtocol protocol;
		protected DateTime connectTime;

		public event PropertyChangedEventHandler PropertyChanged;

		public delegate void PackageEvent(CommLink sender, LinkEventArgs e);
		public event PackageEvent OnReceivePackage;
		public event PackageEvent OnSendPackage;
		public Queue<LinkPackage> ReceivedPackageQueue
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
			receivedPackageQueue = new Queue<LinkPackage>();
			sendPackageQueue = new Queue<LinkPackage>();
			switch (p)
			{
				case LinkProtocol.ANOLink:
					receivePackage = new ANOLinkPackage();
					break;
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

		public CommLink():this(LinkProtocol.NoLink)
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

        protected void PropertyChangedEvent(Object sender,string name)
        {
            this.PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs(name));
        }


        public virtual void OpenLink()
		{

		}

		public virtual void CloseLink()
		{

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
