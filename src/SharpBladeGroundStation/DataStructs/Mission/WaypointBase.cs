using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GMap.NET;
using GMap.NET.WindowsPresentation;

namespace SharpBladeGroundStation.DataStructs
{
    /// <summary>
    /// 航点的基类
    /// </summary>
    public class WaypointBase : MissionItem
    {
        protected PointLatLng position;
        protected float altitude;
        protected float heading;
        protected bool useRelativeAlt;

        public WaypointBase(int i) : this(i, new PointLatLng(0, 0), 0) { }

        public WaypointBase(int i, PointLatLng pos, float alt) : base()
        {
            position = pos;
            altitude = alt;
            useRelativeAlt = true;
            id = i;
        }

        public PointLatLng Position
        {
            get { return position; }
            set
            {
                position = value;
                NotifyPropertyChanged("Position");
            }
        }

        public float Altitude
        {
            get { return altitude; }
            set
            {
                altitude = value;
                NotifyPropertyChanged("Altitude");
            }
        }

        public bool UseRelativeAlt
        {
            get { return useRelativeAlt; }
            set
            {
                useRelativeAlt = value;
                NotifyPropertyChanged("UseRelativeAlt");
            }
        }

        public float Heading
        {
            get { return heading; }
            set
            {
                heading = value;
                NotifyPropertyChanged("Heading");
            }
        }
    }
}
