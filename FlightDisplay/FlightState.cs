using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightDisplay
{
    public class FlightState
    {
        public float Yaw { get; set; }
        public float Pitch { get; set; }
        public float Roll { get; set; }
        public float Heading { get; set; }

        public float GroundSpeed { get; set; }
        public float AirSpeed { get; set; }
        public float ClimbRate { get; set; }

        public static FlightState Zero
        {
            get
            {
                return new FlightState() { Yaw = 0, Pitch = 0, Roll = 0, Heading = 0, GroundSpeed = 0, AirSpeed = 0, ClimbRate = 0 };
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
        }
    }
}
