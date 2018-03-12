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
    /// MapScaleControl.xaml 的交互逻辑
    /// </summary>
    public partial class MapScaleControl : UserControl
    {
		private static int[] scaleSet = { 10, 20, 50, 100, 200, 500, 1000, 2000, 5000, 10000, 20000, 50000, 100000, 200000, 500000 };
        public MapScaleControl()
        {
            InitializeComponent();
			
        }

		public static readonly DependencyProperty ScaleProperty =
			DependencyProperty.Register("Scale", typeof(float), typeof(MapScaleControl), new PropertyMetadata(1.0f, MapScaleControl.OnScalePropertyChanged));
		public float Scale
		{
			get { return (float)GetValue(ScaleProperty); }
			set { SetValue(ScaleProperty, value); }
		}

		public static readonly DependencyProperty ScaleLengthProperty =
			DependencyProperty.Register("ScaleLength", typeof(float), typeof(MapScaleControl), new PropertyMetadata(100.0f, MapScaleControl.OnScaleLengthPropertyChanged));
		public float ScaleLength
		{
			get { return (float)GetValue(ScaleLengthProperty); }
			set { SetValue(ScaleLengthProperty, value); }
		}

		public static readonly DependencyProperty ScaleTextProperty =
			DependencyProperty.Register("ScaleText", typeof(string), typeof(MapScaleControl), new PropertyMetadata("100 m", MapScaleControl.OnScaleTextPropertyChanged));
		public string ScaleText
		{
			get { return (string)GetValue(ScaleTextProperty); }
			set { SetValue(ScaleTextProperty, value); }
		}

		public static void OnScaleTextPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
		{
			MapScaleControl s = sender as MapScaleControl;
			s.OnScaleTextChanged(e);
		}
		public void OnScaleTextChanged(DependencyPropertyChangedEventArgs e)
		{


		}

		public static void OnScaleLengthPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
		{
			MapScaleControl s = sender as MapScaleControl;
			s.OnScaleLengthChanged(e);
		}
		public void OnScaleLengthChanged(DependencyPropertyChangedEventArgs e)
		{


		}

		public static void OnScalePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
		{
			MapScaleControl s = sender as MapScaleControl;
			s.OnScaleChanged(e);
		}
		public void OnScaleChanged(DependencyPropertyChangedEventArgs e)
		{
			float s = (float)e.NewValue;
			if(s<0.1)
			{
				this.Visibility = Visibility.Hidden;
				s = 1;
				return;
			}
			this.Visibility = Visibility.Visible;
			int p = 0;
			for(p=1;p<15;p++)
			{
				if(scaleSet[p] / s>190)				
					break;
			}
			p = p - 1;
			ScaleLength = scaleSet[p]/s+3;
			ScaleText = getText(scaleSet[p]);
		}
		string getText(int a)
		{
			return a > 1000 ? (a / 1000).ToString() + "km" : a.ToString() + "m";
		}

	}
}
