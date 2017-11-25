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

namespace SharpBladeGroundStation.Map.Markers
{
	/// <summary>
	/// WayPointMarker.xaml 的交互逻辑
	/// </summary>
	public partial class WayPointMarker : UserControl
	{
		string markerText;
		Popup popup;
		TextBlock labelText;
		GMapMarker marker;
		MainWindow mainWindow;

		public WayPointMarker(MainWindow window,GMapMarker m,string wptext, string labeltext)
		{
			InitializeComponent();
			mainWindow = window;
			marker = m;
			MarkerText = wptext;

			popup = new Popup();
			labelText = new TextBlock();
			popup.Placement = PlacementMode.Mouse;
			labelText.Background = Brushes.DarkGray;
			labelText.Foreground = Brushes.White;
			labelText.FontSize = 10;
			labelText.Text = labeltext;

			popup.Child = labelText;

			this.Unloaded += WayPointMarker_Unloaded;
			this.Loaded += WayPointMarker_Loaded;
			this.SizeChanged += WayPointMarker_SizeChanged;
			this.MouseEnter += WayPointMarker_MouseEnter;
			this.MouseLeave += WayPointMarker_MouseLeave;
			this.MouseMove += WayPointMarker_MouseMove;
			this.MouseLeftButtonDown += WayPointMarker_MouseLeftButtonDown;
			this.MouseLeftButtonUp += WayPointMarker_MouseLeftButtonUp;

		}

		private void WayPointMarker_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			if(IsMouseCaptured)
			{
				Mouse.Capture(null);
			}
		}

		private void WayPointMarker_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (!IsMouseCaptured)
			{
				Mouse.Capture(this);
			}
			e.Handled = true;
		}

		private void WayPointMarker_MouseMove(object sender, MouseEventArgs e)
		{
			if (e.LeftButton == MouseButtonState.Pressed && IsMouseCaptured)
			{
				Point p = e.GetPosition(mainWindow.gmap);
				Marker.Position = mainWindow.gmap.FromLocalToLatLng((int)(p.X), (int)(p.Y));
			}
		}

		private void WayPointMarker_MouseLeave(object sender, MouseEventArgs e)
		{
			Marker.ZIndex -= 10000;
			if (popup != null)
				popup.IsOpen = false;
		}

		private void WayPointMarker_MouseEnter(object sender, MouseEventArgs e)
		{
			Marker.ZIndex += 10000;
			if (popup != null)
				popup.IsOpen = true;
		}

		private void WayPointMarker_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			Marker.Offset = new Point(-e.NewSize.Width / 2, -e.NewSize.Height / 2);
		}

		private void WayPointMarker_Loaded(object sender, RoutedEventArgs e)
		{
			
		}

		private void WayPointMarker_Unloaded(object sender, RoutedEventArgs e)
		{
			popup = null;
			labelText = null;
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

		public GMapMarker Marker
		{
			get
			{
				return marker;
			}

		}
	}
}
