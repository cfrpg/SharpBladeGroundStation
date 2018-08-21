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

namespace SharpBladeGroundStation.HUD
{
	/// <summary>
	/// HeadingIndicator.xaml 的交互逻辑
	/// </summary>
	public partial class SovietHeadingIndicatorControl : UserControl
	{
		public static readonly DependencyProperty TicksProperty =
			DependencyProperty.Register("Ticks", typeof(IList<Object>), typeof(SovietHeadingIndicatorControl));
		public IList<Object> Ticks
		{
			get { return (IList<Object>)GetValue(TicksProperty); }
			set { SetValue(TicksProperty, value); }
		}

		public static readonly DependencyProperty TextTicksProperty =
			DependencyProperty.Register("TextTicks", typeof(IList<Object>), typeof(SovietHeadingIndicatorControl));
		public IList<Object> TextTicks
		{
			get { return (IList<Object>)GetValue(TextTicksProperty); }
			set { SetValue(TextTicksProperty, value); }
		}


		public static readonly DependencyProperty HeadingProperty =
			DependencyProperty.Register("Heading", typeof(float), typeof(SovietHeadingIndicatorControl), new PropertyMetadata(0f, SovietHeadingIndicatorControl.OnHeadingPropertyChanged));
		public float Heading
		{
			get { return (float)GetValue(HeadingProperty); }
			set { SetValue(HeadingProperty, value); }
		}
		public static void OnHeadingPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
		{
			SovietHeadingIndicatorControl s = sender as SovietHeadingIndicatorControl;
			s.OnHeadingChanged(e);
		}
		public void OnHeadingChanged(DependencyPropertyChangedEventArgs e)
		{

		}

		public SovietHeadingIndicatorControl()
		{
			InitializeComponent();
			InitTicks();
		}

		private void InitTicks()
		{
			Ticks = new List<Object>();
			TextTicks = new List<Object>();
			int longtickl = 12;
			int shorttickl = 12;
			for (int i = 0; i < 109; i++)
			{
				Ticks.Add(i % 2 == 0 ? longtickl : shorttickl);
			}
			for (int i = 0; i < 55; i++)
			{
				TextTicks.Add(((((i - 9) * 10 + 360) % 360) / 10).ToString("00"));
			}

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
