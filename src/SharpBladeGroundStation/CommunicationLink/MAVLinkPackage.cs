using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.MAVLink;
using System.Diagnostics;

namespace SharpBladeGroundStation.CommunicationLink
{
	public class MAVLinkPackage : LinkPackage
	{
		int function;
		int version;
		int headerSize;

		byte incompatibility;
		byte compatibility;
		byte sequence;
		byte system;
		byte component;
		byte[] signature;
		public override int DataSize
		{
			get { return dataSize; }
		}

		public override int HeaderSize
		{
			get { return headerSize; }
		}

		public override int PackageSize
		{
			get
			{
				if ((incompatibility & MAVLink.MAVLINK_IFLAG_MASK) != 0)
				{
					return dataSize + HeaderSize + 2 + 13;
				}
				return dataSize + HeaderSize + 2;
			}
		}

		public override LinkProtocol Protocol
		{
			get { return LinkProtocol.MAVLink; }
		}

		public int Function
		{
			get { return function; }
			set { function = value; }
		}
		/// <summary>
		/// 顺序号
		/// </summary>
		public byte Sequence
		{
			get { return sequence; }
			set { sequence = value; }
		}
		/// <summary>
		/// SystemID
		/// </summary>
		public byte System
		{
			get { return system; }
			set { system = value; }
		}
		/// <summary>
		/// ComponentID
		/// </summary>
		public byte Component
		{
			get { return component; }
			set { component = value; }
		}

		public int Version
		{
			get { return version; }
		}

		/// <summary>
		/// 
		/// </summary>
		public byte[] Signature
		{
			get { return signature; }
			set { signature = value; }
		}

		public byte Incompatibility
		{
			get { return incompatibility; }
			set { incompatibility = value; }
		}

		public byte Compatibility
		{
			get { return compatibility; }
			set { compatibility = value; }
		}

		/// <summary>
		/// 构造空白的数据包，自行识别版本
		/// </summary>
		public MAVLinkPackage() : base(280)
		{
			function = 0;
			version = 0;
			headerSize = 0;
			signature = new byte[13];
		}

		/// <summary>
		/// 构造数据包，指定功能和版本
		/// </summary>
		/// <param name="f">数据包内容</param>
		/// <param name="v">MAVLink版本</param>
		public MAVLinkPackage(int f,int v) : base(256)
		{
			function = f;
			version = v < 2 ? 1 : v;
			if (version <= 1)
				headerSize = 6;
			else
				headerSize = 9;
			signature = new byte[13];
		}
		public override PackageParseResult ReadFromBuffer(byte[] buff, int length, int offset)
		{
			int hs = 6;
			int fun = 0;
			if (length - offset < hs)
				return PackageParseResult.NoEnoughData;
			//Check STX
			if (!(buff[offset + 0] == 0xFE|| buff[offset + 0] == 0xFD))
				return PackageParseResult.NoSTX;
			//Get LEN
			int len = buff[offset + 1];
			if (buff[offset + 0] == 0xFD)
			{
				hs = 10;				
			}			
			if (len + hs + 2 + offset > length)
				return PackageParseResult.NoEnoughData;
			if (buff[offset + 0] == 0xFE)
			{				
				fun = buff[offset + 5];
			}
			else
			{
				fun = buff[offset + 9] << 16 | buff[offset + 8] << 8 | buff[offset + 7];
			}
			//Check checksum
			ushort crc = MavlinkCRC.Calculate(buff, len + hs, offset);
			crc = MavlinkCRC.Accumulate(MAVLink.MAVLINK_MESSAGE_INFOS.GetMessageInfo((uint)fun).crc, crc);
			if (buff[len + hs + offset] != ((byte)(crc & 0xFF)))
				return PackageParseResult.BadCheckSum;
			if (buff[len + hs + offset + 1] != ((byte)(crc >> 8)))
				return PackageParseResult.BadCheckSum;

			//if (buff[len + HeaderSize + 2 + offset]!=0xFE)
			//return PackageParseResult.BadCheckSum;
			headerSize = hs;
			if (hs == 6)	//v1.0
			{
				version = 1;
				sequence = buff[offset + 2];
				system = buff[offset + 3];
				component = buff[offset + 4];
				
			}
			else			//v2.0
			{
				version = 2;
				incompatibility = buff[offset + 2];
				compatibility = buff[offset + 3];
				sequence = buff[offset + 4];
				system = buff[offset + 5];
				component = buff[offset + 6];
						
			}
			function = fun;
			dataSize = len;

			for (int i = 0; i < buffer.Length; i++)
			{
				buffer[i] = 0;
			}
			for (int i = 0; i < PackageSize; i++)
			{
				buffer[i] = buff[offset + i];
			}
			if((incompatibility & MAVLink.MAVLINK_IFLAG_MASK) != 0)
			{				
				for(int i=0;i<13;i++)
				{
					signature[i] = buffer[PackageSize - 13 + i];
				}
			}
			return PackageParseResult.Yes;
		}
		public override bool StartRead()
		{
			return base.StartRead();
		}
		public override void SetVerify()
		{
			buffer[0] = 0xFE;
			buffer[1] = (byte)(dataSize & 0xFF);
			buffer[5] = (byte)function;
			ushort crc = MavlinkCRC.Calculate(buffer, dataSize + HeaderSize);
			crc = MavlinkCRC.Accumulate(MAVLink.MAVLINK_MESSAGE_INFOS.GetMessageInfo((uint)function).crc, crc);
			AddData(crc);
		}
		public override string ToString()
		{
			return string.Format("MAVLink{3}.0 package SIZE={2},FUN={0},LEN={1}", function, DataSize, PackageSize,Version);
		}
		public override LinkPackage Clone()
		{
			MAVLinkPackage p = new MAVLinkPackage();
			Array.Copy(buffer, p.buffer, dataSize + HeaderSize + 3);
			//buffer.CopyTo(p.buffer, 0);
			//int function;
			//int version;
			//int headerSize;

			//byte incompatibility;
			//byte compatibility;
			//byte sequence;
			//byte system;
			//byte component;
			//byte[] signature;
			p.function = function;
			p.version = version;
			p.headerSize = headerSize;
			p.incompatibility = incompatibility;
			p.compatibility = compatibility;
			p.sequence = sequence;
			p.system = system;
			p.component = component;
			signature.CopyTo(p.signature,0);

			p.dataSize = dataSize;
			p.timeStamp = timeStamp;		
			return p;
		}
	}
}
