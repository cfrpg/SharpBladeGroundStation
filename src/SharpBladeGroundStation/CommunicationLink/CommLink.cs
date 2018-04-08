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

		public event PropertyChangedEventHandler PropertyChanged;

		public delegate void ReceivePackageEvent(CommLink sender, EventArgs e);
		public event ReceivePackageEvent OnReceivePackage;

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

		protected void OnReceivePackageEvent(CommLink sender, EventArgs e)
		{
			OnReceivePackage?.Invoke(this, new EventArgs());
		}
	}
}
