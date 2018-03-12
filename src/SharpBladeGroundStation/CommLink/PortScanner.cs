using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO.Ports;

using System.Diagnostics;

namespace SharpBladeGroundStation.CommLink
{
	public class PortScanner
	{
		int maxDataSize;
		int timeout;
		LinkProtocol protocol;
		int baudRate;
        bool isStarted;

		System.Timers.Timer scantimer;
		List<PortData> ports;

		public delegate void FindPortEvent(PortScanner sender, PortScannerEventArgs e);

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
            IsStarted = false;

		}

		private void Scantimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			string[] portnames = SerialPort.GetPortNames();
			//查找所有串口
			foreach (string pn in portnames)
			{
				bool flag = false;
				for (int i = 0; i < ports.Count; i++)
				{
					if (ports[i].name == pn)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{//添加新出现的串口
					PortData port = new PortData();
					port.name = pn;
					port.link = new SerialLink(pn, protocol);
					port.lastCheckTime = DateTime.Now;
					port.state = PortScannerState.NewPort;
					ports.Add(port);
					Debug.WriteLine("[port scanner]find new port:" + pn);
				}
			}
			//更新各个串口状态
			for (int i = 0; i < ports.Count; i++)
			{
				switch (ports[i].state)
				{
					case PortScannerState.NewPort:
						ports[i].link.ResetLink();
						try
						{
							ports[i].link.OpenPort();
							ports[i].SetState(PortScannerState.Scanning);
							Debug.WriteLine("[port scanner]start scanning:" + ports[i].name);
						}
						catch
						{
							ports[i].SetState(PortScannerState.Unavailable);
							Debug.WriteLine("[port scanner]cannot start scanning:" + ports[i].name);
						}
						break;
					case PortScannerState.Scanning:
						if (ports[i].link.ReceivedPackageQueue.Count > 1)
						{
							ports[i].SetState(PortScannerState.Available);
							ports[i].lastCheckTime = DateTime.Now;
							Debug.WriteLine("[port scanner]find port:" + ports[i].name);
						}
						else
						{
							if (ports[i].link.DataReceived > 3000 || DateTime.Now.Subtract(ports[i].lastCheckTime).TotalSeconds > 5.0)
							{
								ports[i].SetState(PortScannerState.Unavailable);
								ports[i].lastCheckTime = DateTime.Now;
								Debug.WriteLine("[port scanner]unavilable port:" + ports[i].name);
							}
						}
						break;
					case PortScannerState.Available:
						ports[i].link.ResetLink();
						OnFindPort?.Invoke(this, new PortScannerEventArgs(ports[i].link));
						break;
					case PortScannerState.Unavailable:
						ports[i].link.ResetLink();
						if (DateTime.Now.Subtract(ports[i].lastCheckTime).TotalSeconds > 1.0)
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

        public bool IsStarted
        {
            get { return isStarted; }
            set { isStarted = value; }
        }

        public void Start()
		{
			scantimer.Start();
            IsStarted = true;
		}

		public void Stop()
		{
			scantimer.Stop();
            IsStarted = false;
		}

		class PortData
		{
			public string name;
			public PortScannerState state;
			public SerialLink link;
			public DateTime lastCheckTime;
			public void SetState(PortScannerState s)
			{
				this.state = s;
			}
		}
	}

	
	public class PortScannerEventArgs : EventArgs
	{
		string portName;
		SerialLink link;
		public SerialLink Link
		{
			get { return link; }
		}

		public PortScannerEventArgs(SerialLink l)
		{
			link = l;
			portName = l.Port.PortName;
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
