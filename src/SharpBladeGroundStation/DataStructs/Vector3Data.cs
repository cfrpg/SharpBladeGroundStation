using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Ports;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;

namespace SharpBladeGroundStation.DataStructs
{
	public class Vector3Data: INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		double x;
		double y;
		double z;
		string name;
		DateTime updateTime;
		double updateFrequency;

		public double X
		{
			get { return x; }
			set
			{
				x = value;
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("X"));
				
			}
		}

		public double Y
		{
			get { return y; }
			set
			{
				y = value;
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Y"));
				
			}
		}

		public double Z
		{
			get { return z; }
			set
			{
				z = value;
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Z"));
				UpdateTime = DateTime.Now;
			}
		}

		public string Name
		{
			get { return name; }
			set
			{
				name = value;
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Name"));
			}
		}

		public DateTime UpdateTime
		{
			get { return updateTime; }
			private set
			{
				double dt=value.Subtract(updateTime).TotalMilliseconds;
				Frequency = 1000.0 / dt;
				updateTime = value;
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("UpdateTime"));
			}
		}

		public double Frequency
		{
			get { return updateFrequency; }
			private set
			{
				updateFrequency = value;
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Frequency"));
			}
		}

		public Vector3Data():this("")
		{

		}

		public Vector3Data(string n):this(n,0,0,0)
		{

		}

		public Vector3Data(string n, double _x, double _y, double _z)
		{
			Name = n;
			X = _x;
			Y = _y;
			Z = _z;			
		}

		public Vector3Data Clone()
		{
			return new Vector3Data(name, x, y, z);
		}
	}
}
