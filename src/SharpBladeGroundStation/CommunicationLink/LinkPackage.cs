using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpBladeGroundStation.CommunicationLink
{
	/// <summary>
	/// 数据包的基类
	/// </summary>
	public class LinkPackage
	{
		protected int function;
		protected byte[] buffer;
		protected int dataSize;
		protected int cursor;
		protected bool reverseBytes;
		protected double timeStamp;

		public byte[] Buffer
		{
			get { return buffer; }
			set { buffer = value; }
		}
		/// <summary>
		/// 整个数据包的大小
		/// </summary>
		public virtual int PackageSize
		{
			get { return dataSize + HeaderSize + 1; }
		}

		/// <summary>
		/// 数据部分的长度
		/// </summary>
		public virtual int DataSize
		{
			get { return dataSize; }
		}

		/// <summary>
		/// 读取的位置
		/// </summary>
		public int Cursor
		{
			get { return cursor; }
			set { cursor = value; }
		}

		public virtual int HeaderSize
		{
			get { return 0; }
		}

		public LinkPackage(int buffsize)
		{
			buffer = new byte[buffsize];
			dataSize = 0;
			reverseBytes = false;
			timeStamp = 0;
		}

		public byte[] PackageBuffer
		{
			get
			{
				byte[] b = new byte[PackageSize];
				Array.Copy(buffer, b, PackageSize);
				return b;
			}
		}

		public double TimeStamp
		{
			get { return timeStamp; }
			set { timeStamp = value; }
		}

		/// <summary>
		/// 数据包使用的协议
		/// </summary>
		public virtual LinkProtocol Protocol
		{
			get { return LinkProtocol.NoLink; }
		}

		public virtual int Function
		{
			get { return function; }
			set { function = value; }
		}

		#region Read&Write
		public bool AddData(byte[] data)
		{
			if (data.Length + dataSize + HeaderSize > buffer.Length)
				return false;
			if (reverseBytes)
			{
				Array.Reverse(data);
			}
			data.CopyTo(buffer, dataSize + HeaderSize);
			dataSize += data.Length;
			return true;
		}
		/// <summary>
		/// Add uint8
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		public bool AddData(byte data)
		{
			if (1 + dataSize + HeaderSize > buffer.Length)
				return false;
			buffer[dataSize + HeaderSize] = data;
			dataSize++;
			return true;
		}
		/// <summary>
		/// Add int8
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		public bool AddData(sbyte data)
		{
			if (1 + dataSize + HeaderSize > buffer.Length)
				return false;
			buffer[dataSize + HeaderSize] = (byte)data;
			dataSize++;
			return true;
		}
		/// <summary>
		/// Add int16
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		public bool AddData(short data)
		{
			return AddData(BitConverter.GetBytes(data));
		}
		/// <summary>
		/// Add uint16
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		public bool AddData(ushort data)
		{
			return AddData(BitConverter.GetBytes(data));
		}
		/// <summary>
		/// Add int32
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		public bool AddData(int data)
		{
			return AddData(BitConverter.GetBytes(data));
		}
		/// <summary>
		/// Add uint32
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		public bool AddData(uint data)
		{
			return AddData(BitConverter.GetBytes(data));
		}
		/// <summary>
		/// Add single
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		public bool AddData(float data)
		{
			return AddData(BitConverter.GetBytes(data));
		}
		/// <summary>
		/// Add double
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		public bool AddData(double data)
		{
			return AddData(BitConverter.GetBytes(data));
		}
		/// <summary>
		/// Add UTF-16 char(2bytes)
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		public bool AddData(char data)
		{
			return AddData(BitConverter.GetBytes(data));
		}

		public virtual bool StartRead()
		{
			cursor = 0;
			return true;
		}

		public sbyte NextSByte()
		{
			if (cursor + 1 <= dataSize)
			{
				cursor += 1;
				unchecked
				{
					return (sbyte)buffer[cursor + HeaderSize - 1];
				}
			}
			else
			{
				throw new IndexOutOfRangeException("No enough data to read.");
			}

		}
		public byte NextByte()
		{
			if (cursor + 1 <= dataSize)
			{
				cursor += 1;
				return buffer[cursor + HeaderSize - 1];
			}
			else
			{
				throw new IndexOutOfRangeException("No enough data to read.");
			}

		}
		public short NextShort()
		{
			if (cursor + 2 <= dataSize)
			{
				cursor += 2;
				if (reverseBytes)
				{
					byte[] b = new byte[2];
					Array.Copy(buffer, cursor + HeaderSize - 2, b, 0, 2);
					Array.Reverse(b);
					return BitConverter.ToInt16(b, 0);
				}
				return BitConverter.ToInt16(buffer, cursor + HeaderSize - 2);
			}
			else
			{
				throw new IndexOutOfRangeException("No enough data to read.");
			}

		}
		public ushort NextUShort()
		{
			if (cursor + 2 <= dataSize)
			{
				cursor += 2;
				if (reverseBytes)
				{
					byte[] b = new byte[2];
					Array.Copy(buffer, cursor + HeaderSize - 2, b, 0, 2);
					Array.Reverse(b);
					return BitConverter.ToUInt16(b, 0);
				}
				return BitConverter.ToUInt16(buffer, cursor + HeaderSize - 2);
			}
			else
			{
				throw new IndexOutOfRangeException("No enough data to read.");
			}

		}
		public int NextInt32()
		{
			if (cursor + 4 <= dataSize)
			{
				cursor += 4;
				if (reverseBytes)
				{
					byte[] b = new byte[4];
					Array.Copy(buffer, cursor + HeaderSize - 4, b, 0, 4);
					Array.Reverse(b);
					return BitConverter.ToInt32(b, 0);
				}
				return BitConverter.ToInt32(buffer, cursor + HeaderSize - 4);
			}
			else
			{
				throw new IndexOutOfRangeException("No enough data to read.");
			}

		}
		public uint NextUInt32()
		{
			if (cursor + 4 <= dataSize)
			{
				cursor += 4;
				if (reverseBytes)
				{
					byte[] b = new byte[4];
					Array.Copy(buffer, cursor + HeaderSize - 4, b, 0, 4);
					Array.Reverse(b);
					return BitConverter.ToUInt32(b, 0);
				}
				return BitConverter.ToUInt32(buffer, cursor + HeaderSize - 4);
			}
			else
			{
				throw new IndexOutOfRangeException("No enough data to read.");
			}

		}
		public float NextSingle()
		{
			if (cursor + 4 <= dataSize)
			{
				cursor += 4;
				if (reverseBytes)
				{
					byte[] b = new byte[4];
					Array.Copy(buffer, cursor + HeaderSize - 4, b, 0, 4);
					Array.Reverse(b);
					return BitConverter.ToSingle(b, 0);
				}
				return BitConverter.ToSingle(buffer, cursor + HeaderSize - 4);
			}
			else
			{
				throw new IndexOutOfRangeException("No enough data to read.");
			}

		}
		public double NextDouble()
		{
			if (cursor + 8 <= dataSize)
			{
				cursor += 8;
				if (reverseBytes)
				{
					byte[] b = new byte[8];
					Array.Copy(buffer, cursor + HeaderSize - 8, b, 0, 8);
					Array.Reverse(b);
					return BitConverter.ToDouble(b, 0);
				}
				return BitConverter.ToDouble(buffer, cursor + HeaderSize - 8);
			}
			else
			{
				throw new IndexOutOfRangeException("No enough data to read.");
			}

		}

		public UInt64 NextUInt64()
		{
			if (cursor + 8 <= dataSize)
			{
				cursor += 8;
				if (reverseBytes)
				{
					byte[] b = new byte[8];
					Array.Copy(buffer, cursor + HeaderSize - 8, b, 0, 8);
					Array.Reverse(b);
					return BitConverter.ToUInt64(b, 0);
				}
				return BitConverter.ToUInt64(buffer, cursor + HeaderSize - 8);
			}
			else
			{
				throw new IndexOutOfRangeException("No enough data to read.");
			}
		}
		#endregion

		public virtual void SetVerify()
		{
			byte v = 0;
			int i = 0;
			for (i = 0; i < HeaderSize + dataSize; i++)
			{
				v += buffer[i];
			}
			buffer[i] = v;
		}
		/// <summary>
		/// 尝试从缓冲区中读取一个包，成功则返回Yes，并将包读取到对象中
		/// 失败则返回失败原因，对象内容不变。
		/// </summary>
		/// <param name="buff">要读取的缓冲区</param>
		/// <param name="length">缓冲区有效数据长度</param>
		/// <returns></returns>
		public virtual PackageParseResult ReadFromBuffer(byte[] buff, int length, int offset,out int dataUsed)
		{
			dataUsed = 0;
			if (length - offset < 1)
				return PackageParseResult.NoEnoughData;
			return PackageParseResult.NoSTX;
		}

		public virtual PackageParseResult ReadFromBuffer(byte[] buff, int length, int offset)
		{
			int t;
			return ReadFromBuffer(buff, length, offset, out t);
		}

		public virtual PackageParseResult ReadFromBufferWithoutCheck(byte[] buff, int length, int offset)
		{
			if (length - offset < 1)
				return PackageParseResult.NoEnoughData;
			return PackageParseResult.NoSTX;
		}

		public virtual LinkPackage Clone()
		{
			LinkPackage p = new LinkPackage(buffer.Length);
			buffer.CopyTo(p.buffer, 0);
			p.dataSize = dataSize;
			p.reverseBytes = this.reverseBytes;
			p.timeStamp = timeStamp;
			return p;
		}

	}
}
