using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace SharpBladeGroundStation.DataStructs
{
	public class NavigationData:INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;
		double longitude;
		double latitude;
		float homingAngle;
		float distToHome;
		float distToWayPoint;


		public double Longitude
		{
			get
			{
				return longitude;
			}

			set
			{
				longitude = value;
			}
		}

		public double Latitude
		{
			get
			{
				return latitude;
			}

			set
			{
				latitude = value;
			}
		}

		public float HomingAngle
		{
			get
			{
				return homingAngle;
			}

			set
			{
				homingAngle = value;
			}
		}

		public float DistToHome
		{
			get
			{
				return distToHome;
			}

			set
			{
				distToHome = value;
			}
		}

		public float DistToWayPoint
		{
			get
			{
				return distToWayPoint;
			}

			set
			{
				distToWayPoint = value;
			}
		}
	}
}
