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
	public partial class WayPointMarker : GMapElement
	{
		string markerText;
		Popup popup;
		TextBlock labelText;
		MapRouteData route;
        bool isMoving;
        Point offset;
        Point clickPos;
		public WayPointMarker(MapRouteData r,GMapMarker m,string wptext, string labeltext)
		{
			InitializeComponent();
			route = r;
			marker = m;
			MarkerText = wptext;
            isMoving = false;
            offset = new Point();

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

        public WayPointMarker(Waypoint wp)
        {

        }

		private void WayPointMarker_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			if(IsMouseCaptured)
			{
                isMoving = false;
				Mouse.Capture(null);
			}
            //e.Handled = true;
		}

		private void WayPointMarker_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (!IsMouseCaptured)
			{
                isMoving = false;
                clickPos = e.GetPosition(this);
				Mouse.Capture(this);
			}
			e.Handled = true;
		}

		private void WayPointMarker_MouseMove(object sender, MouseEventArgs e)
		{
			if (e.LeftButton == MouseButtonState.Pressed && IsMouseCaptured)
			{
                if (!isMoving)
                {
                    offset = e.GetPosition(this);
                    offset.X -= clickPos.X;
                    offset.Y -= clickPos.Y;
                    if(Math.Abs(offset.X)>this.ActualWidth/4||Math.Abs(offset.Y)>this.ActualHeight/4)
                    {
                        isMoving = true;
                        offset = e.GetPosition(this);
                        offset.X -= this.ActualWidth / 2;
                        offset.Y -= this.ActualHeight / 2;
                    }
                }
                if(isMoving)
                {
                    Point p = e.GetPosition(route.Map);
                    p.X -= offset.X;
                    p.Y -= offset.Y;
                    marker.Position = route.Map.FromLocalToLatLng((int)(p.X), (int)(p.Y));
                }
			}
            e.Handled = true;
		}

		private void WayPointMarker_MouseLeave(object sender, MouseEventArgs e)
		{
			marker.ZIndex -= 10000;
			if (popup != null)
				popup.IsOpen = false;
		}

		private void WayPointMarker_MouseEnter(object sender, MouseEventArgs e)
		{
			marker.ZIndex += 10000;
			if (popup != null)
				popup.IsOpen = true;
		}

		private void WayPointMarker_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			marker.Offset = new Point(-e.NewSize.Width / 2, -e.NewSize.Height / 2);
		}

		private void WayPointMarker_Loaded(object sender, RoutedEventArgs e)
		{
			
		}

		private void WayPointMarker_Unloaded(object sender, RoutedEventArgs e)
		{
            popup.IsOpen = false;
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

		public string LabelText
		{
			get { return labelText.Text; }
			set { labelText.Text = value; }
		}
	}
}
