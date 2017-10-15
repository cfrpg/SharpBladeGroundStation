using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace FlightDisplay
{
    public class FlightState:INotifyPropertyChanged
    {
		public event PropertyChangedEventHandler PropertyChanged;
		float yaw;
		float pitch;
		float roll;
		float heading;

		float groundSpeed;
		float airSpeed;
		float climbRate;
		float altitude;

		string flightModeText;
		bool isArmed;
        public static FlightState Zero
        {
            get
            {
                return new FlightState() { Yaw = 0, Pitch = 0, Roll = 0, Heading = 0, GroundSpeed = 0, AirSpeed = 0, ClimbRate = 0 };
            }
        }

		public float Yaw
		{
			get { return yaw; }
			set
			{
				yaw = value;
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Yaw"));
			}
		}

		public float Pitch
		{
			get { return pitch; }
			set
			{
				pitch = value;
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Pitch"));
			}
		}

		public float Roll
		{
			get { return roll; }
			set
			{
				roll = value;
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Roll"));
			}
		}

		public float Heading
		{
			get { return heading; }
			set
			{
				heading = value;
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Heading"));
			}
		}

		public float GroundSpeed
		{
			get { return groundSpeed; }
			set
			{
				groundSpeed = value;
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("GroundSpeed"));
			}
		}

		public float AirSpeed
		{
			get { return airSpeed; }
			set
			{
				airSpeed = value;
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("AirSpeed"));
			}
		}

		public float ClimbRate
		{
			get { return climbRate; }
			set
			{
				climbRate = value;
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ClimbRate"));
			}
		}

		public float Altitude
		{
			get { return altitude; }
			set
			{
				altitude = value;
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Altitude"));
			}
		}

		public string FlightModeText
		{
			get { return flightModeText; }
			set
			{
				flightModeText = value;
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("FlightModeText"));
			}
		}

		public bool IsArmed
		{
			get { return isArmed; }
			set
			{
				isArmed = value;
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsArmed"));
			}
		}

		public FlightState()
        {
            Yaw = 0;
            Pitch = 0;
			Roll = 0;
            Heading = 0;
            GroundSpeed = 0;
            AirSpeed = 0;
            ClimbRate = 0;
			FlightModeText = "未知";
			IsArmed = false;
        }
    }
}
