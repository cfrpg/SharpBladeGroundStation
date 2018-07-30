using System;
using System.Windows;
using System.Windows.Input;
using GMap.NET.WindowsPresentation;

namespace SharpBladeGroundStation.Map.Markers
{
    public class ClickableGMapMarker:GMapElement
    {
        bool isMoving;
        Point clickOffset;
        Point clickPos;

        public ClickableGMapMarker():base()
        {
            this.SizeChanged += ClickableGMapMarker_SizeChanged;
            this.MouseEnter += ClickableGMapMarker_MouseEnter;
            this.MouseLeave += ClickableGMapMarker_MouseLeave;
            this.MouseMove += ClickableGMapMarker_MouseMove;
            this.MouseLeftButtonDown += ClickableGMapMarker_MouseLeftButtonDown;
            this.MouseLeftButtonUp += ClickableGMapMarker_MouseLeftButtonUp;			
        }

		
		public ClickableGMapMarker(GMapMarker m):this()
        {
            marker = m;            
        }

        private void ClickableGMapMarker_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (IsMouseCaptured)
            {
                isMoving = false;
                Mouse.Capture(null);
            }
        }

        private void ClickableGMapMarker_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!IsMouseCaptured)
            {
                isMoving = false;
                clickPos = e.GetPosition(this);
                Mouse.Capture(this);
            }
            e.Handled = true;
        }

        private void ClickableGMapMarker_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && IsMouseCaptured)
            {
                if (!isMoving)
                {
                    clickOffset = e.GetPosition(this);
                    clickOffset.X -= clickPos.X;
                    clickOffset.Y -= clickPos.Y;
                    if (Math.Abs(clickOffset.X) > this.ActualWidth / 4 || Math.Abs(clickOffset.Y) > this.ActualHeight / 4)
                    {
                        isMoving = true;
                        clickOffset = e.GetPosition(this);
                        clickOffset.X -= this.ActualWidth / 2;
                        clickOffset.Y -= this.ActualHeight / 2;
                    }
                }
                if (isMoving)
                {
                    Point p = e.GetPosition(this.Map);
                    p.X -= clickOffset.X;
                    p.Y -= clickOffset.Y;
                    marker.Position = this.Map.FromLocalToLatLng((int)(p.X), (int)(p.Y));
                }
                e.Handled = true;
            }            
        }

        private void ClickableGMapMarker_MouseLeave(object sender, MouseEventArgs e)
        {
            marker.ZIndex -= 10000;          
        }

        private void ClickableGMapMarker_MouseEnter(object sender, MouseEventArgs e)
        {
            marker.ZIndex += 10000;
        }

        private void ClickableGMapMarker_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if(marker!=null)
                marker.Offset = new Point(-e.NewSize.Width / 2, -e.NewSize.Height / 2);
        }        

        public virtual Point GetOffset(Size s)
        {
            return new Point(-s.Width / 2, -s.Height / 2);
        }

        public virtual GMapControl Map
        {
            get { return new GMapControl(); }
        }
				
    }
}
