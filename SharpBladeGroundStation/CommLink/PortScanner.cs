using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO.Ports;

namespace SharpBladeGroundStation.CommLink
{
	public class PortScanner
	{
		int maxDataSize;
		int timeout;
		LinkProtocol protocol;
		int baudRate;		

		System.Timers.Timer scantimer;
		List<PortData> ports;

		public delegate void FindPortEvent(PortScanner sender, EventArgs e);

		public event FindPortEvent OnFindPort;

		public PortScanner(LinkProtocol p,int br, int md,int to)
		{
			maxDataSize = md;
			protocol = p;
			baudRate = br;
			timeout = to;

			scantimer = new System.Timers.Timer(500);
			scantimer.Elapsed += Scantimer_Elapsed;
			ports = new List<PortData>();
		}

		private void Scantimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			string[] portnames=SerialPort.GetPortNames();
			foreach(string pn in portnames)
			{
				bool flag = false;
				for(int i=0;i<ports.Count;i++)
				{
					if(ports[i].name==pn)
					{
						flag = true;						
						break;
					}
				}
				if(!flag)
				{
					PortData port = new PortData();
					port.name = pn;
					port.link = new SerialLink(pn, protocol);
					port.lastCheckTime = DateTime.Now;
					port.state = PortScannerState.NewPort;
					ports.Add(port);		
				}
			}
			for(int i=0;i<ports.Count;i++)
			{
				switch(ports[i].state)
				{
					case PortScannerState.NewPort:
						ports[i].link.ResetLink();
						ports[i].link.OpenPort();
						break;
					case PortScannerState.Scanning:
						if(ports[i].link.ReceivedPackageQueue.Count>1)
						{
							ports[i].SetState(PortScannerState.Available);
						}
						else
						{
							if(ports[i].link.DataReceived>3000|| DateTime.Now.Subtract(ports[i].lastCheckTime).TotalSeconds > 5.0)
							{
								ports[i].SetState(PortScannerState.Unavailable);
							}
						}
						break;
					case PortScannerState.Available:
						ports[i].link.ClosePort();
						OnFindPort(this, new EventArgs());
						break;
					case PortScannerState.Unavailable:
						ports[i].link.ClosePort();
						if(DateTime.Now.Subtract(ports[i].lastCheckTime).TotalSeconds > 2.0)
						{
							ports[i].SetState(PortScannerState.NewPort);
						}
						break;
				}
			}
		}		

		public int MaxDataSize
		{
			get { return maxDataSize; }
			set { maxDataSize = value; }
		}

		public LinkProtocol Protocol
		{
			get { return protocol; }
			set { protocol = value; }
		}

		public int BaudRate
		{
			get { return baudRate; }
			set { baudRate = value; }
		}

		public int Timeout
		{
			get { return timeout; }
			set { timeout = value; }
		}

		public void Start()
		{
			scantimer.Start();
		}

		public void Stop()
		{
			scantimer.Stop();
		}
	}

	struct PortData
	{
		public string name;
		public Thread scanner;
		public PortScannerState state;
		public SerialLink link;
		public DateTime lastCheckTime;
		public void SetState(PortScannerState s)
		{
			state = s;
		}
	}

	public enum PortScannerState
	{
		/// <summary>
		/// 尚未检查的端口
		/// </summary>
		NewPort,
		/// <summary>
		/// 正在检查的端口
		/// </summary>
		Scanning,
		/// <summary>
		/// 检查完毕，可用的端口
		/// </summary>
		Available,
		/// <summary>
		/// 检查完毕，不可用的端口
		/// </summary>
		Unavailable
	}
}
