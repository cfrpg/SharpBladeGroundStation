using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;

namespace SharpBladeGroundStation.CommunicationLink
{
	public class LogLink : CommLink
	{
		string path;
		string filename;
		FileStream stream;
		byte[] buffer;

		bool loaded;
		bool ended;

		Thread background;
		LinkPackage currentPackage;
		double currentPackageTime;
		LinkPackageDirection currPkgDir;
		DateTime openTime;

		public string Path
		{
			get { return path; }
		}

		public string Filename
		{
			get { return filename; }
		}

		public LogLink():base()
		{
			buffer = new byte[16384];
			background = new Thread(backgroundWorker);
			background.IsBackground = true;
			loaded = false;
			ended = false;

		}

		public override void OpenLink()
		{
			if (Loaded)
			{
				background.Start();
				openTime = DateTime.Now;
				currentPackageTime = -1;
			}
		}

		public override void CloseLink()
		{
			ended = true;
		}

		public override string LinkName
		{
			get
			{
				return "Replay";
			}
		}

		public bool Ended
		{
			get { return ended; }
		}

		public bool Loaded
		{
			get { return loaded; }
		}

		public LoadFileResualt OpenFile(string p)
		{
			FileInfo fi = new FileInfo(p);
			if (!fi.Exists)
				return LoadFileResualt.NotExist;
			stream = new FileStream(p, FileMode.Open);
			stream.Read(buffer, 0, CommLogger.FileHeaderSize);
			byte[] header = { 83, 66, 76, 79, 71 };
			for(int i=0;i<5;i++)
			{
				if (buffer[i] != header[i])
					return LoadFileResualt.UnkonwType;
			}
			protocol = (LinkProtocol)buffer[5];
			long time = BitConverter.ToInt64(buffer, 6);
			connectTime =DateTime.FromBinary(time);
			path = p;
			filename = fi.Name;
			switch (protocol)
			{
				case LinkProtocol.ANOLink:
					currentPackage = new ANOLinkPackage();
					break;
				case LinkProtocol.MAVLink:
					currentPackage = new MAVLinkPackage();
					break;
				case LinkProtocol.MAVLink2:
					currentPackage = new MAVLinkPackage();
					break;
				default:
					currentPackage = new LinkPackage(2048);
					break;
			}
			loaded = true;
			return LoadFileResualt.OK;
		}

		private void backgroundWorker()
		{
			bool res = true;
			while(!ended)
			{
				if(!res)
				{
					ended = true;
				}
				
				if (currentPackageTime < 0)
				{
					res=readPackage();
					continue;
				}
				double time= DateTime.Now.Subtract(openTime).TotalMilliseconds;
				if(time>currentPackageTime)
				{
					if (currPkgDir == LinkPackageDirection.ToGCS)
					{
						receivePackage = currentPackage.Clone();
						ReceivedPackageQueue.Enqueue(currentPackage.Clone());
						OnReceivePackageEvent(this, new LinkEventArgs(receivePackage));
					}
					res = readPackage();
				}
			}
			stream.Close();
		}

		private bool readPackage()
		{
			long rem = stream.Length - stream.Position;
			if (rem < CommLogger.PackageHeaderSize)
				return false;
			stream.Read(buffer, 0, CommLogger.PackageHeaderSize);
			int len = BitConverter.ToInt32(buffer, 0);
			rem = stream.Length - stream.Position;
			if (rem < len)
				return false;
			currentPackageTime = BitConverter.ToDouble(buffer, 4);
			currPkgDir = (LinkPackageDirection)buffer[12];
			len -= CommLogger.PackageHeaderSize;
			stream.Read(buffer, 0, len);
			PackageParseResult res = currentPackage.ReadFromBuffer(buffer, len, 0);
			if(res!=PackageParseResult.Yes)
			{

			}
			return true;

		}
	}
}
