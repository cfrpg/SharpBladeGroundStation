using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GMap.NET;
using GMap.NET.WindowsPresentation;

namespace SharpBladeGroundStation.DataStructs
{
    public class WaypointBase
    {
        protected PointLatLng position;
        protected float altitude;
        private bool useRelativeAlt;

        public WaypointBase() : this(new PointLatLng(0, 0), 0) { }

        public WaypointBase(PointLatLng pos,float alt)
        {
            position = pos;
            altitude = alt;
            useRelativeAlt = true;          
        }

        public PointLatLng Position
        {
            get { return position; }
            set { position = value; }
        }

        public float Altitude
        {
            get { return altitude; }
            set { altitude = value; }
        }

        protected bool UseRelativeAlt
        {
            get { return useRelativeAlt; }
            set { useRelativeAlt = value; }
        }
    }
}
