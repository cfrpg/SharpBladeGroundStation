using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpBladeGroundStation.CommLink
{
	public class MAVLinkPackage : LinkPackage
	{
		byte function;
		public override int DataSize
		{
			get { return dataSize; }
		}

		public override int HeaderSize
		{
			get { return 6; }
		}

		public override int PackageSize
		{
			get { return dataSize + HeaderSize + 1; }
		}

		public byte Function
		{
			get { return function; }
			set { function = value; }
		}

		public MAVLinkPackage() : base(512)
		{
			function = 0;
		}


		public override PackageParseResult ReadFromBuffer(byte[] buff, int length, int offset)
		{
			if (length - offset < HeaderSize)
				return PackageParseResult.NoEnoughData;
			//Check STX
			if (!(buff[offset + 0] == 0xFE))
				return PackageParseResult.NoSTX;
			//Get LEN
			int len = buff[offset + 1];
			if (len + HeaderSize+3 + offset > length)
				return PackageParseResult.NoEnoughData;
			
			//Check checksum
			if (buff[len + HeaderSize + 2 + offset]!=0xFE)
				return PackageParseResult.BadCheckSum;
			for(int i=0;i<buffer.Length;i++)
			{
				buffer[i] = 0;
			}
			for (int i = 0; i < len + HeaderSize+2; i++)
			{
				buffer[i] = buff[offset + i];
			}
			function = buff[offset + 5];
			dataSize = len;
			return PackageParseResult.Yes;
		}

		public override bool StartRead()
		{
			return base.StartRead();
		}
		public override void SetVerify()
		{
			byte v = 0;
			int i = 0;
			for (i = 0; i < HeaderSize + dataSize; i++)
			{
				v += buffer[i];
			}
			buffer[i] = v;
		}
		public override string ToString()
		{
			return string.Format("MAVLink package SIZE={2},FUN={0},LEN={1}", function, DataSize, PackageSize);
		}
		public override LinkPackage Clone()
		{
			MAVLinkPackage p = new MAVLinkPackage();
			buffer.CopyTo(p.buffer, 0);
			p.dataSize = dataSize;
			p.function = function;
			p.timeStamp = timeStamp;
			return p;
		}
	}
}
