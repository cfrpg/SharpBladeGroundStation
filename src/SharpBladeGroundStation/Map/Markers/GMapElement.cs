using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using GMap.NET;
using GMap.NET.WindowsPresentation;

namespace SharpBladeGroundStation.Map.Markers
{
	public class GMapElement : UserControl
	{
		protected GMapMarker marker;
		protected float altitude;

		public GMapMarker Marker
		{
			get { return marker; }
			set { marker = value; }
		}

		public PointLatLng Position
		{
			get { return marker.Position; }
			set { marker.Position = value; }
		}

		public float Altitude
		{
			get { return altitude; }
			set { altitude = value; }
		}

		public GMapElement() : base()
		{

		}
	}
}
