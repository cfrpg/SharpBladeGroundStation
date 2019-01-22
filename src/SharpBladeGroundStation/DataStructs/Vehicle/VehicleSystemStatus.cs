using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace SharpBladeGroundStation.DataStructs
{

	public class VehicleSystemStatus : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		int accelerometer;
		int gyroscope;
		int compass;
		int airspeed;
		int gps;
		int telemetey;
		int radio;
		int battery;
		int ekf;


		public int Accelerometer
		{
			get { return accelerometer; }
			set
			{
				accelerometer = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Accelerometer"));
			}
		}

		public int Gyroscope
		{
			get { return gyroscope; }
			set
			{
				gyroscope = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Gyroscope"));
			}
		}

		public int Compass
		{
			get { return compass; }
			set
			{
				compass = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Compass"));
			}
		}

		public int Airspeed
		{
			get { return airspeed; }
			set
			{
				airspeed = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Airspeed"));
			}
		}

		public int Gps
		{
			get { return gps; }
			set
			{
				gps = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Gps"));
			}
		}

		public int Radio
		{
			get { return radio; }
			set
			{
				radio = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Radio"));
			}
		}

		public int Battery
		{
			get { return battery; }
			set
			{
				battery = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Battery"));
			}
		}

		public int Ekf
		{
			get { return ekf; }
			set
			{
				ekf = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Ekf"));
			}
		}

		public int Telemetey
		{
			get { return telemetey; }
			set
			{
				telemetey = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Telemetey"));
			}
		}

		public VehicleSystemStatus()
		{

		}
	}
}
