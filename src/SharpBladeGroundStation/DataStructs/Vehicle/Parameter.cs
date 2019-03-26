using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.MAVLink;
using System.ComponentModel;

namespace SharpBladeGroundStation.DataStructs
{
	public class Parameter : INotifyPropertyChanged
	{
		string key;
		ValueUnion value;
		//float value;
		MAVLink.MAV_PARAM_TYPE type;
		bool unsave;

		public event PropertyChangedEventHandler PropertyChanged;

		public string Key
		{
			get { return key; }
			set
			{
				key = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Key"));
			}
		}

		public MAVLink.MAV_PARAM_TYPE Type
		{
			get { return type; }
			set
			{
				type = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Type"));
			}
		}

		public object Value
		{
			get
			{
				switch (type)
				{
					case MAVLink.MAV_PARAM_TYPE.UINT8:
						return value.u8;
					case MAVLink.MAV_PARAM_TYPE.INT8:
						return value.s8;
					case MAVLink.MAV_PARAM_TYPE.UINT16:
						return value.u16;
					case MAVLink.MAV_PARAM_TYPE.INT16:
						return value.s16;
					case MAVLink.MAV_PARAM_TYPE.UINT32:
						return value.u32;
					case MAVLink.MAV_PARAM_TYPE.INT32:
						return value.s32;
					case MAVLink.MAV_PARAM_TYPE.UINT64:
						return value.u64;
					case MAVLink.MAV_PARAM_TYPE.INT64:
						return value.s64;
					case MAVLink.MAV_PARAM_TYPE.REAL32:
						return value.r32;
					case MAVLink.MAV_PARAM_TYPE.REAL64:
						return value.r64;
					default:
						return 0;
				}

			}
		}

		public bool Unsave
		{
			get { return unsave; }
			set
			{
				unsave = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Unsave"));
			}
		}

		//public float Value
		//{
		//	get { return value; }
		//	set
		//	{
		//		this.value = value;
		//		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Value"));
		//	}
		//}

		public Parameter(string k, MAVLink.MAV_PARAM_TYPE t,float v)
		{
			key = k;
			type = t;
			value = new ValueUnion();
			value.r32 = v;
			//Value = 0;
			unsave = false;
		}
		#region Value access
		public void SetValue(byte v)
		{
			if (type != MAVLink.MAV_PARAM_TYPE.UINT8)
				throw new ArgumentException("Value type mismatch.");
			value.u8 = v;
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Value"));
		}

		public void SetValue(sbyte v)
		{
			if (type != MAVLink.MAV_PARAM_TYPE.INT8)
				throw new ArgumentException("Value type mismatch.");
			value.s8 = v;
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Value"));
		}

		public void SetValue(UInt16 v)
		{
			if (type != MAVLink.MAV_PARAM_TYPE.UINT16)
				throw new ArgumentException("Value type mismatch.");
			value.u16 = v;
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Value"));
		}

		public void SetValue(Int16 v)
		{
			if (type != MAVLink.MAV_PARAM_TYPE.INT16)
				throw new ArgumentException("Value type mismatch.");
			value.s16 = v;
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Value"));
		}

		public void SetValue(UInt32 v)
		{
			if (type != MAVLink.MAV_PARAM_TYPE.UINT32)
				throw new ArgumentException("Value type mismatch.");
			value.u32 = v;
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Value"));
		}

		public void SetValue(Int32 v)
		{
			if (type != MAVLink.MAV_PARAM_TYPE.INT32)
				throw new ArgumentException("Value type mismatch.");
			value.s32 = v;
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Value"));
		}

		public void SetValue(UInt64 v)
		{
			if (type != MAVLink.MAV_PARAM_TYPE.UINT64)
				throw new ArgumentException("Value type mismatch.");
			value.u64 = v;
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Value"));
		}

		public void SetValue(Int64 v)
		{
			if (type != MAVLink.MAV_PARAM_TYPE.INT64)
				throw new ArgumentException("Value type mismatch.");
			value.s64 = v;
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Value"));
		}

		public void SetValue(float v)
		{
			if (type != MAVLink.MAV_PARAM_TYPE.REAL32)
				throw new ArgumentException("Value type mismatch.");
			value.r32 = v;
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Value"));
		}

		public void GetValue(ref byte v)
		{
			if (type != MAVLink.MAV_PARAM_TYPE.UINT8)
				throw new ArgumentException("Value type mismatch.");
			v = value.u8;
		}

		public void GetValue(ref sbyte v)
		{
			if (type != MAVLink.MAV_PARAM_TYPE.INT8)
				throw new ArgumentException("Value type mismatch.");
			v = value.s8;
		}

		public void GetValue(ref UInt16 v)
		{
			if (type != MAVLink.MAV_PARAM_TYPE.UINT16)
				throw new ArgumentException("Value type mismatch.");
			v = value.u16;
		}

		public void GetValue(ref Int16 v)
		{
			if (type != MAVLink.MAV_PARAM_TYPE.INT16)
				throw new ArgumentException("Value type mismatch.");
			v = value.s16;
		}

		public void GetValue(ref UInt32 v)
		{
			if (type != MAVLink.MAV_PARAM_TYPE.UINT32)
				throw new ArgumentException("Value type mismatch.");
			v = value.u32;
		}

		public void GetValue(ref Int32 v)
		{
			if (type != MAVLink.MAV_PARAM_TYPE.INT32)
				throw new ArgumentException("Value type mismatch.");
			v = value.s32;
		}

		public void GetValue(ref UInt64 v)
		{
			if (type != MAVLink.MAV_PARAM_TYPE.UINT64)
				throw new ArgumentException("Value type mismatch.");
			v = value.u64;
		}

		public void GetValue(ref Int64 v)
		{
			if (type != MAVLink.MAV_PARAM_TYPE.INT64)
				throw new ArgumentException("Value type mismatch.");
			v = value.s64;
		}

		public void GetValue(ref float v)
		{
			if (type != MAVLink.MAV_PARAM_TYPE.REAL32)
				throw new ArgumentException("Value type mismatch.");
			v = value.r32;
		}

		public void GetValue(ref double v)
		{
			if (type != MAVLink.MAV_PARAM_TYPE.REAL64)
				throw new ArgumentException("Value type mismatch.");
			v = value.r64;
		}

		public void SetAsFloat(float v)
		{
			value.r32 = v;
		}

		public float GetAsFloat()
		{
			return value.r32;
		}
		#endregion
	}

	[StructLayout(LayoutKind.Explicit, Size = 8)]
	public struct ValueUnion
	{
		[FieldOffset(0)]
		public byte u8;
		[FieldOffset(0)]
		public sbyte s8;
		[FieldOffset(0)]
		public ushort u16;
		[FieldOffset(0)]
		public short s16;
		[FieldOffset(0)]
		public UInt32 u32;
		[FieldOffset(0)]
		public int s32;
		[FieldOffset(0)]
		public UInt64 u64;
		[FieldOffset(0)]
		public Int64 s64;
		[FieldOffset(0)]
		public float r32;
		[FieldOffset(0)]
		public double r64;
	}
}
