using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FlightDisplay
{
	/// <summary>
	/// HeadingIndicator.xaml 的交互逻辑
	/// </summary>
	public partial class HeadingIndicatorControl : UserControl
	{
		public static readonly DependencyProperty TicksProperty =
			DependencyProperty.Register("Ticks", typeof(IList<Object>), typeof(HeadingIndicatorControl));
		public IList<Object> Ticks
		{
			get { return (IList<Object>)GetValue(TicksProperty); }
			set { SetValue(TicksProperty, value); }
		}

		public static readonly DependencyProperty TextTicksProperty =
			DependencyProperty.Register("TextTicks", typeof(IList<Object>), typeof(HeadingIndicatorControl));
		public IList<Object> TextTicks
		{
			get { return (IList<Object>)GetValue(TextTicksProperty); }
			set { SetValue(TextTicksProperty, value); }
		}


		public static readonly DependencyProperty HeadingProperty =
			DependencyProperty.Register("Heading", typeof(float), typeof(HeadingIndicatorControl), new PropertyMetadata(0f, HeadingIndicatorControl.OnHeadingPropertyChanged));
		public float Heading
		{
			get { return (float)GetValue(HeadingProperty); }
			set { SetValue(HeadingProperty, value); }
		}
		public static void OnHeadingPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
		{
			HeadingIndicatorControl s = sender as HeadingIndicatorControl;
			s.OnHeadingChanged(e);
		}
		public void OnHeadingChanged(DependencyPropertyChangedEventArgs e)
		{
			
		}

		public HeadingIndicatorControl()
		{
			InitializeComponent();
			InitTicks();
		}

		private void InitTicks()
		{
			Ticks = new List<Object>();
			TextTicks = new List<Object>();
			int longtickl = 10;
			int shorttickl = 5;
			for(int i=0;i<109;i++)
			{
				Ticks.Add(i % 3 == 0 ? longtickl : shorttickl);				
			}
			for(int i=0;i<37;i++)
			{
				TextTicks.Add(((i - 6) * 15 + 360) % 360);
			}
			TextTicks[0] = "W";
			TextTicks[3] = "NW";
			TextTicks[6] = "N";
			TextTicks[9] = "NE";
			TextTicks[12] = "E";
			TextTicks[15] = "SE";
			TextTicks[18] = "S";
			TextTicks[21] = "SW";
			TextTicks[24] = "W";
			TextTicks[27] = "NW";
			TextTicks[30] = "N";
			TextTicks[33] = "NE";
			TextTicks[36] = "E";
			//TextTicks.Add("W");
			//TextTicks.Add("NW");
			//TextTicks.Add("N");
			//TextTicks.Add("NE");
			//TextTicks.Add("E");
			//TextTicks.Add("SE");
			//TextTicks.Add("S");
			//TextTicks.Add("SW");
			//TextTicks.Add("W");
			//TextTicks.Add("NW");
			//TextTicks.Add("N");
			//this.lineTicks.ItemsSource = Ticks;
		}
	}
}
