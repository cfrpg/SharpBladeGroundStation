using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace SharpBladeGroundStation.DataStructs
{
	public class GPSData:INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		int satelliteCount;
		GPSPositionState state;
		float longitude;
		float latitude;
		float homingAngle;
		float vdop;
		float hdop;

		public int SatelliteCount
		{
			get { return satelliteCount; }
			set
			{
				satelliteCount = value;
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SatelliteCount"));
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("GPSStateText"));
			}
		}

		public GPSPositionState State
		{
			get { return state; }
			set
			{
				state = value;
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("State"));
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("GPSPositionStateText"));
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("GPSStateText"));
			}
		}

		public float Longitude
		{
			get { return longitude; }
			set
			{
				longitude = value;
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Longitude"));
			}
		}

		public float Latitude
		{
			get { return latitude; }
			set
			{
				latitude = value;
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Latitude"));
			}
		}

		public float HomingAngle
		{
			get { return homingAngle; }
			set
			{
				homingAngle = value;
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("HomingAngle"));
			}
		}

		public string GPSPositionStateText
		{
			get { return Enum.GetName(typeof(GPSPositionState), state); }
		}

		public string GPSStateText
		{
			get
			{
				return satelliteCount.ToString()+"星 "+ Enum.GetName(typeof(GPSPositionState), state);				
			}
		}

		public float Vdop
		{
			get
			{
				return vdop;
			}

			set
			{
				vdop = value;
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Vdop"));
			}
		}

		public float Hdop
		{
			get
			{
				return hdop;
			}

			set
			{
				hdop = value;
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Hdop"));
			}
		}

		public GPSData()
		{
			satelliteCount = 0;
			state = GPSPositionState.Undefined;
			longitude = 0;
			latitude = 0;
			homingAngle = 0;
		}
		
	}

	public enum GPSPositionState
	{
		Undefined1=0,
		Undefined2,
		Undefined3,
		Undefined4,
		Undefined5,
		Undefined6,
		Undefined	//This one is real "Undefined"
	}
}
