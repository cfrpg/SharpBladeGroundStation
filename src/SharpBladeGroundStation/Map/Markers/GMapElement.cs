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

		public GMapMarker Marker
		{
			get { return marker; }
			set { marker = value; }
		}

		public PointLatLng Position
		{
			get { return marker.Position; }
		}

		public GMapElement() : base()
		{
            
		}
	}
}
