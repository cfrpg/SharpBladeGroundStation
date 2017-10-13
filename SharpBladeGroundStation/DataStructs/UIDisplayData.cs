using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace SharpBladeGroundStation.DataStructs
{
	public class UIDisplayData :INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		double attRoll;
		double attPitch;
		double attYaw;
		double altitude;
		int flyMode;
		string flyModeText;
		bool armed;
		string armedText;




		public double AttRoll
		{
			get
			{
				return attRoll;
			}

			set
			{
				attRoll = value;
			}
		}

		public double AttPitch
		{
			get
			{
				return attPitch;
			}

			set
			{
				attPitch = value;
			}
		}

		public double AttYaw
		{
			get
			{
				return attYaw;
			}

			set
			{
				attYaw = value;
			}
		}

		public double Altitude
		{
			get
			{
				return altitude;
			}

			set
			{
				altitude = value;
			}
		}

		public int FlyMode
		{
			get
			{
				return flyMode;
			}

			set
			{
				flyMode = value;
			}
		}

		public string FlyModeText
		{
			get
			{
				return flyModeText;
			}

			set
			{
				flyModeText = value;
			}
		}

		public bool Armed
		{
			get
			{
				return armed;
			}

			set
			{
				armed = value;
			}
		}

		public string ArmedText
		{
			get
			{
				return armedText;
			}

			set
			{
				armedText = value;
			}
		}
	}
}
