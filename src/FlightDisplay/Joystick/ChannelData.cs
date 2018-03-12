using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace FlightDisplay
{
	public class ChannelData : INotifyPropertyChanged
	{
		
		int rawValue;
		int offset;
		int minValue;
		int maxValue;
		int id;
		int mappedID;
		int scale;

		public int Value
		{
			get
			{
				int v = rawValue - offset;
				if(v>=0)
				{
					v = v * scale / (maxValue - offset);
				}
				else
				{
					v = v * scale / (offset - minValue);
				}
				return v;
			}
		}

		public int RawValue
		{
			get { return rawValue; }
			set
			{
				rawValue = value;
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Value"));
			}
		}

		public int Offset
		{
			get { return offset; }
			set
			{
				offset = value;
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Value"));
			}
		}

		public int MinValue
		{
			get { return minValue; }
			set
			{
				minValue = value;
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Value"));
			}
		}

		public int MaxValue
		{
			get { return maxValue; }
			set
			{
				maxValue = value;
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Value"));
			}
		}

		public int ID
		{
			get { return id; }
			set { id = value; }
		}

		public int MappingID
		{
			get { return mappedID; }
			set { mappedID = value; }
		}

		public int Scale
		{
			get { return scale; }
			set
			{
				scale = value;
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Value"));
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public ChannelData(int i,int max,int min,int off,int s)
		{
			id = i;
			maxValue = max;
			minValue = min;
			offset = off;
			mappedID = 0;
			scale = s;
			rawValue = 0;
		}

		public ChannelData(int i) : this(i, 1000, -1000, 0, 1000) { }

	}


}
