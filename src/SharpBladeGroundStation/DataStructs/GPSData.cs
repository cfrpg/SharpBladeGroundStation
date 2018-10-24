using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using GMap.NET;
using SharpBladeGroundStation.Map;

namespace SharpBladeGroundStation.DataStructs
{
	public class GPSData : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		int satelliteCount;
		GPSPositionState state;
		double longitude;
		double latitude;
		float homingAngle;
		float vdop;
		float hdop;

		PointLatLng homePosition;
		float distanceToHome;

		bool homed;

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

		public double Longitude
		{
			get { return longitude; }
			set
			{
				longitude = value;
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Longitude"));
				if (Math.Abs(homePosition.Lat) > 1)
				{
					DistanceToHome = (float)PositionHelper.GetDistance(new PointLatLng(latitude, longitude), homePosition);
				}

			}
		}

		public double Latitude
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
				return satelliteCount.ToString() + "星 " + Enum.GetName(typeof(GPSPositionState), state);
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

		public PointLatLng HomePosition
		{
			get
			{
				return homePosition;
			}

			set
			{
				homePosition = value;
			}
		}

		public float DistanceToHome
		{
			get
			{
				return distanceToHome;
			}

			set
			{
				distanceToHome = value;
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("DistanceToHome"));
			}
		}

		public GPSData()
		{
			satelliteCount = 0;
			state = GPSPositionState.Undefined;
			longitude = 0;
			latitude = 0;
			homingAngle = 0;
			homePosition = new PointLatLng(0, 0);
			distanceToHome = 0;
			homed = false;
		}

		public void SetHome()
		{
			if (homed)
				return;
			homePosition.Lat = latitude;
			homePosition.Lng = longitude;
			homed = true;
		}

		public void ForceSetHome()
		{			
			homePosition.Lat = latitude;
			homePosition.Lng = longitude;
			homed = true;
		}

	}

	public enum GPSPositionState
	{
		NoGPS = 0,
		NoFix,
		Fix2D,
		Fix3D,
		DGPS,
		RTK_FLT,
		RTK_FIXED,
		STATIC,
		PPP,
		Undefined   //This one is real "Undefined"
	}
}
