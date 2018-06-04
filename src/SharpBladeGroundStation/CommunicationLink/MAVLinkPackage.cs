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
			get { return dataSize + HeaderSize + 2; }
		}

		public override LinkProtocol Protocol
		{
			get { return LinkProtocol.MAVLink; }
		}

		public byte Function
		{
			get { return function; }
			set { function = value; }
		}
        /// <summary>
        /// 顺序号
        /// </summary>
        public byte Sequence
        {
            get { return buffer[2]; }
            set { buffer[2] = value; }
        }
        /// <summary>
        /// SystemID
        /// </summary>
        public byte System
        {
            get { return buffer[3]; }
            set { buffer[3] = value; }
        }
        /// <summary>
        /// ComponentID
        /// </summary>
        public byte Component
        {
            get { return buffer[4]; }
            set { buffer[4] = value; }
        }
		public MAVLinkPackage() : base(256)
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
			if (len + HeaderSize+2 + offset > length)
				return PackageParseResult.NoEnoughData;

            //Check checksum
            ushort crc = MavlinkCRC.Calculate(buff, len + HeaderSize, offset);
            crc = MavlinkCRC.Accumulate(MAVLink.MAVLINK_MESSAGE_INFOS.GetMessageInfo(buff[offset + 5]).crc, crc);
            if(buff[len+HeaderSize+offset]!=((byte)(crc&0xFF)))
                return PackageParseResult.BadCheckSum;
            if (buff[len + HeaderSize + offset + 1]!=((byte)(crc>>8)))
                return PackageParseResult.BadCheckSum;

            //if (buff[len + HeaderSize + 2 + offset]!=0xFE)
				//return PackageParseResult.BadCheckSum;
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
            buffer[0] = 0xFE;
            buffer[1] = (byte)(dataSize&0xFF);
            buffer[5] = function;
			ushort crc= MavlinkCRC.Calculate(buffer,  dataSize+ HeaderSize);
            crc = MavlinkCRC.Accumulate(MAVLink.MAVLINK_MESSAGE_INFOS.GetMessageInfo(function).crc, crc);
            AddData(crc);
        }
		public override string ToString()
		{
			return string.Format("MAVLink package SIZE={2},FUN={0},LEN={1}", function, DataSize, PackageSize);
		}
		public override LinkPackage Clone()
		{
			MAVLinkPackage p = new MAVLinkPackage();
			Array.Copy(buffer, p.buffer, dataSize + HeaderSize + 3);
			//buffer.CopyTo(p.buffer, 0);
			p.dataSize = dataSize;
			p.function = function;
			p.timeStamp = timeStamp;
			return p;
		}
	}
}
