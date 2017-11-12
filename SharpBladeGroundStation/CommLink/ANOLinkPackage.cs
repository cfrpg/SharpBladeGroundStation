using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpBladeGroundStation.CommLink
{
	public class ANOLinkPackage : LinkPackage
	{
		byte function;
		public override int DataSize
		{
			get { return dataSize; }
		}

		public override int HeaderSize
		{
			get { return 4; }
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

		public ANOLinkPackage():base(256)
		{
			function = 0;
			reverseBytes = true;
		}

		
		public override PackageParseResult ReadFromBuffer(byte[] buff,int length,int offset)
		{			
			if (length-offset < HeaderSize)
				return PackageParseResult.NoEnoughData;
			//Check STX
			if (!(buff[offset + 0] == 0xAA && buff[offset + 1] == 0xAA))
				return PackageParseResult.NoSTX;
			//Get LEN
			int len = buff[offset + 3];
			if (len + 5 + offset > length)
				return PackageParseResult.NoEnoughData;
			byte sum = 0;
			for (int i = 0; i < len + HeaderSize; i++)
			{
				sum += buff[offset + i];
			}
			//Check checksum
			if (sum != buff[offset + len + HeaderSize])
				return PackageParseResult.BadCheckSum;
			for(int i=0;i<=len+4;i++)
			{
				buffer[i] = buff[offset + i];
			}
			function = buff[offset + 2];
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
			buffer[0] = 0xAA;
			buffer[1] = 0xAF;
			buffer[2] = function;
			buffer[3] = (byte)dataSize;
			for (i = 0; i < HeaderSize + dataSize; i++)
			{
				v += buffer[i];
			}
			buffer[i] = v;
			
		}
		public override string ToString()
		{
			return string.Format("ANOLink package SIZE={2},FUN={0},LEN={1}", (int)buffer[2], DataSize, PackageSize);
		}
		public override LinkPackage Clone()
		{
			ANOLinkPackage p = new ANOLinkPackage();
			buffer.CopyTo(p.buffer, 0);
			p.dataSize = dataSize;
			p.function = function;
			p.timeStamp = timeStamp;
			return p;
		}
	}
}
