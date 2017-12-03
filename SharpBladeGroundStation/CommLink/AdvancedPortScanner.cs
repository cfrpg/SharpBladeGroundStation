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
	public class AdvancedPortScanner
	{
		List<PortData> ports;
		int maxDataSize;
		int timeout;
		int baudRate;
		bool isStarted;
		bool portFound;

		System.Timers.Timer scantimer;

		public delegate void FindPortEvent(AdvancedPortScanner sender, PortScannerEventArgs e);
		public event FindPortEvent OnFindPort;

		public AdvancedPortScanner(int br, int md, int to)
		{
			baudRate = br;
			maxDataSize = md;
			timeout = to;

			isStarted = false;
			portFound = false;
			ports = new List<PortData>();
			scantimer = new System.Timers.Timer(500);
			scantimer.Elapsed += Scantimer_Elapsed;

		}

		public void Start()
		{
			isStarted = false;
			portFound = false;
			ports.Clear();
			scantimer.Start();
			isStarted = true;
		}
		public void Stop()
		{
			scantimer.Stop();
			isStarted = false;
		}

		private void Scantimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			if (portFound)
				return;
			if (!isStarted)
				return;
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
					PortData port = new PortData(pn,baudRate,maxDataSize);			
					
					port.lastCheckTime = DateTime.Now;
					
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
						try
						{
							//ports[i].port.BaudRate = baudRate;
							ports[i].port.Open();
							ports[i].lastCheckTime = DateTime.Now;
							ports[i].state = PortScannerState.Scanning;
							Debug.WriteLine("[port scanner]start scanning:" + ports[i].name);
						}
						catch
						{
							ports[i].state = PortScannerState.Unavailable;
							Debug.WriteLine("[port scanner]cannot start scanning:" + ports[i].name);
						}
						break;
					case PortScannerState.Scanning:
						if(DateTime.Now.Subtract(ports[i].lastCheckTime).TotalSeconds > timeout)
						{
							ports[i].port.Close();
							//ports[i].port.DiscardInBuffer();
							ports[i].state = PortScannerState.Unavailable;
							ports[i].lastCheckTime = DateTime.Now;
							Debug.WriteLine("[port scanner]unavilable port(timeout):" + ports[i].name);
						}
						
						break;
					case PortScannerState.Available:
						ports[i].port.Close();
						portFound = true;
						SerialLink link = new SerialLink(ports[i].name, ports[i].protocol);
						Debug.WriteLine("[port scanner]find port:" + ports[i].name);
						OnFindPort?.Invoke(this, new PortScannerEventArgs(link));
						break;
					case PortScannerState.Unavailable:
						if (ports[i].port.IsOpen)
							ports[i].port.Close();
						if (DateTime.Now.Subtract(ports[i].lastCheckTime).TotalSeconds > 1.0)
						{
							ports[i].state = PortScannerState.NewPort;
						}
						break;
				}
			}
		}

		class PortData
		{
			public string name;
			public PortScannerState state;
			public SerialPort port;
			public DateTime lastCheckTime;
			public LinkProtocol protocol;
			int buffersize;	
			public PortData(string pn,int br,int md)
			{
				name = pn;
				port = new SerialPort(pn);
				port.BaudRate = br;
				port.DataBits = 8;
				port.StopBits = System.IO.Ports.StopBits.One;
				port.ReceivedBytesThreshold = md;
				port.DataReceived += Port_DataReceived;
				buffersize = md +64;
				protocol = LinkProtocol.NoLink;
				state = PortScannerState.NewPort;
			}

			private void Port_DataReceived(object sender, SerialDataReceivedEventArgs e)
			{
				byte[] buff = new byte[buffersize];
				int len = port.BytesToRead;
				int offset;
				if (len > buffersize)
					len = buffersize;
				port.Read(buff, 0, len);
				port.Close();
				PackageParseResult ppr;

				offset = 0;
				MAVLinkPackage mavp = new MAVLinkPackage();				
				ppr = mavp.ReadFromBuffer(buff, len, offset);
				if(ppr==PackageParseResult.Yes)
				{
					offset = mavp.PackageSize;
					ppr = mavp.ReadFromBuffer(buff, len - offset, offset);
					if(ppr==PackageParseResult.Yes)
					{
						state = PortScannerState.Available;
						protocol = LinkProtocol.MAVLink;
						return;
					}
					else
					{
						Debug.WriteLine("[port scanner]cannot get package:" + name+" "+ppr.ToString());
					}
				}

				offset = 0;
				ANOLinkPackage anop = new ANOLinkPackage();
				ppr = anop.ReadFromBuffer(buff, len, offset);
				if (ppr == PackageParseResult.Yes)
				{
					offset = anop.PackageSize;
					ppr = anop.ReadFromBuffer(buff, len - offset, offset);
					if (ppr == PackageParseResult.Yes)
					{
						state = PortScannerState.Available;
						protocol = LinkProtocol.ANOLink;
						return;
					}
				}
				Debug.WriteLine("[port scanner]unavilable port(no link):" + name);
				state = PortScannerState.Unavailable;
			}
		}
	}
}
