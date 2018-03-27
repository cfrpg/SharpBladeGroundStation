using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using FlightDisplay;
using Matrix = Microsoft.Xna.Framework.Matrix;

namespace SharpBladeGroundStation.DataStructs.Vehicle
{
	public class Vehicle:INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;
        Vector3 position;
        Vector3 velocity;
        Vector3 angleVelocity;
        Vector3 eulerAngle;
        GPSData gpsState;
        float heading;
        float groundSpeed;
        float airSpeed;
        float climbRate;
        float altitude;

        string flightModeText;
        bool isArmed;

        FlightState flightState;

        public Vector3 Position
        {
            get { return position; }
            set
            {
                position = value;               
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Position"));
                //PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("FlightState"));
            }
        }

        public Vector3 Velocity
        {
            get { return velocity; }
            set
            {
                velocity = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Velocity"));
                //PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("FlightState"));
            }
        }

        public Vector3 AngleVelocity
        {
            get { return angleVelocity; }
            set
            {
                angleVelocity = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("AngleVelocity"));
                //PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("FlightState"));
            }
        }

        public Vector3 EulerAngle
        {
            get { return eulerAngle; }
            set
            {
                eulerAngle = value;
                flightState.Roll = eulerAngle.X;
                flightState.Pitch = eulerAngle.Y;
                flightState.Yaw = eulerAngle.Z;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("EulerAngle"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("FlightState"));
            }
        }

        public GPSData GpsState
        {
            get { return gpsState; }
            set
            {
                gpsState = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("GpsState"));
                //PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("FlightState"));
            }
        }

        public float Heading
        {
            get { return heading; }
            set
            {
                heading = value;
                flightState.Heading = heading;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Heading"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("FlightState"));
            }
        }

        public float GroundSpeed
        {
            get { return groundSpeed; }
            set
            {
                groundSpeed = value;
                flightState.GroundSpeed = groundSpeed;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("GroundSpeed"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("FlightState"));
            }
        }

        public float AirSpeed
        {
            get { return airSpeed; }
            set
            {
                airSpeed = value;
                flightState.AirSpeed = airSpeed;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("AirSpeed"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("FlightState"));
            }
        }

        public float ClimbRate
        {
            get { return climbRate; }
            set
            {
                climbRate = value;
                flightState.ClimbRate = climbRate;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ClimbRate"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("FlightState"));
            }
        }

        public float Altitude
        {
            get { return altitude; }
            set
            {
                altitude = value;
                flightState.Altitude = altitude;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Altitude"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("FlightState"));
            }
        }

        public string FlightModeText
        {
            get { return flightModeText; }
            set
            {
                flightModeText = value;
                flightState.FlightModeText = flightModeText;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("FlightModeText"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("FlightState"));
            }
        }

        public bool IsArmed
        {
            get { return isArmed; }
            set
            {
                isArmed = value;
                flightState.IsArmed = isArmed;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsArmed"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("FlightState"));
            }
        }

        public FlightState FlightState
        {
            get { return flightState; }
        }

        public Vehicle()
        {
            position = Vector3.Zero;
            velocity = Vector3.Zero;
            angleVelocity = Vector3.Zero;
            eulerAngle = Vector3.Zero;
            gpsState = new GPSData();
            heading = 0;
            groundSpeed = 0;
            airSpeed = 0;
            climbRate = 0;
            altitude = 0;
            flightModeText = "";
            isArmed = false;

            flightState = FlightState.Zero;
        }
        
    }
}
