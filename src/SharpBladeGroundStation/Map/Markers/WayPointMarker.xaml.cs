using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using GMap.NET.WindowsPresentation;
using SharpBladeGroundStation.DataStructs;

namespace SharpBladeGroundStation.Map.Markers
{
	/// <summary>
	/// WaypointMarker.xaml 的交互逻辑
	/// </summary>
	public partial class WaypointMarker : ClickableGMapMarker
	{
		string markerText;
		
		MapRouteData route;

		public WaypointMarker(MapRouteData r, GMapMarker m, string wptext)
		{
			InitializeComponent();
			route = r;
			marker = m;
			MarkerText = wptext;
			altitude = 50;
			this.Unloaded += WaypointMarker_Unloaded;
			this.Loaded += WaypointMarker_Loaded;
			this.MouseEnter += WaypointMarker_MouseEnter;
			this.MouseLeave += WaypointMarker_MouseLeave;

		}
		
		private void WaypointMarker_MouseLeave(object sender, MouseEventArgs e)
		{
			
		}

		private void WaypointMarker_MouseEnter(object sender, MouseEventArgs e)
		{
			
		}

		private void WaypointMarker_Loaded(object sender, RoutedEventArgs e)
		{

		}

		private void WaypointMarker_Unloaded(object sender, RoutedEventArgs e)
		{
			
		}

		public string MarkerText
		{
			get { return markerText; }
			set
			{
				markerText = value;
				text.Text = markerText;
			}
		}

		
		public override GMapControl Map
		{
			get { return Route.Map; }
		}

		public MapRouteData Route
		{
			get { return route; }
		}

		public override Point GetOffset(Size s)
		{
			return base.GetOffset(s);
		}

	}
}
