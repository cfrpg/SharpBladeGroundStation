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
	/// WayPointMarker.xaml 的交互逻辑
	/// </summary>
	public partial class WayPointMarker : ClickableGMapMarker
	{
		string markerText;
		
		MapRouteData route;

		public WayPointMarker(MapRouteData r, GMapMarker m, string wptext)
		{
			InitializeComponent();
			route = r;
			marker = m;
			MarkerText = wptext;
			altitude = 50;
			this.Unloaded += WayPointMarker_Unloaded;
			this.Loaded += WayPointMarker_Loaded;
			this.MouseEnter += WayPointMarker_MouseEnter;
			this.MouseLeave += WayPointMarker_MouseLeave;

		}
		
		private void WayPointMarker_MouseLeave(object sender, MouseEventArgs e)
		{
			
		}

		private void WayPointMarker_MouseEnter(object sender, MouseEventArgs e)
		{
			
		}

		private void WayPointMarker_Loaded(object sender, RoutedEventArgs e)
		{

		}

		private void WayPointMarker_Unloaded(object sender, RoutedEventArgs e)
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
			get { return route.Map; }
		}


		public override Point GetOffset(Size s)
		{
			return base.GetOffset(s);
		}

	}
}
