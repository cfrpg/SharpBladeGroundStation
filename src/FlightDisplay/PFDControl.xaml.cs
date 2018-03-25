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
    /// PFDControl.xaml 的交互逻辑
    /// </summary>
    public partial class PFDControl : UserControl
    {

		bool transparent;


		public static readonly DependencyProperty RollLongTicksProperty =
            DependencyProperty.Register("RollLongTicks", typeof(IList<Object>), typeof(PFDControl));
        public IList<Object> RollLongTicks
        {
            get { return (IList<Object>)GetValue(RollLongTicksProperty); }
            private set { SetValue(RollLongTicksProperty, value); }
        }

        public static readonly DependencyProperty RollShortTicksProperty =
            DependencyProperty.Register("RollShortTicks", typeof(IList<Object>), typeof(PFDControl));
        public IList<Object> RollShortTicks
        {
            get { return (IList<Object>)GetValue(RollShortTicksProperty); }
            private set { SetValue(RollShortTicksProperty, value); }
        }

        public static readonly DependencyProperty PitchShortTicksProperty =
            DependencyProperty.Register("PitchShortTicks", typeof(IList<Object>), typeof(PFDControl));
        public IList<Object> PitchShortTicks
        {
            get { return (IList<Object>)GetValue(PitchShortTicksProperty); }
            private set { SetValue(PitchShortTicksProperty, value); }
        }
        public static readonly DependencyProperty PitchTextTicksProperty =
           DependencyProperty.Register("PitchTextTicks", typeof(IList<Object>), typeof(PFDControl));
        public IList<Object> PitchTextTicks
        {
            get { return (IList<Object>)GetValue(PitchTextTicksProperty); }
            private set { SetValue(PitchTextTicksProperty, value); }
        }
        public static readonly DependencyProperty PitchTranslateProperty =
            DependencyProperty.Register("PitchTranslate", typeof(double), typeof(PFDControl), new PropertyMetadata(0.0));
        public double PitchTranslate
        {
            get { return (double)GetValue(PitchTranslateProperty); }
            private set { SetValue(PitchTranslateProperty, value); }
        }
        

        public static readonly DependencyProperty FlightStateProperty =
            DependencyProperty.Register("FlightState", typeof(FlightState), typeof(PFDControl), new PropertyMetadata(FlightState.Zero, PFDControl.OnFlightStatePropertyChanged));
        public FlightState FlightState
        {
            get { return (FlightState)GetValue(FlightStateProperty); }
            set { SetValue(FlightStateProperty, value); }
        }
        public static void OnFlightStatePropertyChanged(DependencyObject sender,DependencyPropertyChangedEventArgs e)
        {
            PFDControl pc = sender as PFDControl;
            pc.OnFlightStateChanged(e);
        }
        public void OnFlightStateChanged(DependencyPropertyChangedEventArgs e)
        {

        }
        public static readonly DependencyProperty PitchProperty =
            DependencyProperty.Register("Pitch", typeof(float), typeof(PFDControl), new PropertyMetadata(0f, PFDControl.OnPitchPropertyChanged));
        public float Pitch
        {
            get { return (float)GetValue(PitchProperty); }
            set { SetValue(PitchProperty, value); }
        }

        public static readonly DependencyProperty RollProperty =
            DependencyProperty.Register("Roll", typeof(float), typeof(PFDControl), new PropertyMetadata(0f, PFDControl.OnRollPropertyChanged));
        public float Roll
        {
            get { return (float)GetValue(RollProperty); }
            set { SetValue(RollProperty, value); }
        }

		public bool Transparent
		{
			get
			{
				return transparent;
			}

			set
			{
				transparent = value;
				if(transparent)
				{
					horizon.Visibility = Visibility.Visible;
					back.Visibility = Visibility.Hidden;
				}
				else
				{
					horizon.Visibility = Visibility.Hidden;
					back.Visibility = Visibility.Visible;
				}
			}
		}

		public static void OnRollPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            PFDControl s = sender as PFDControl;
            s.OnRollChanged(e);
        }
        public void OnRollChanged(DependencyPropertyChangedEventArgs e)
        {


        }

        public static void OnPitchPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            PFDControl s = sender as PFDControl;
            s.OnPitchChanged(e);
        }
        public void OnPitchChanged(DependencyPropertyChangedEventArgs e)
        {
            SetPitchTicks((float)e.NewValue);

        }

        public PFDControl()
        {
            InitializeComponent();
            InitTicks();
			Binding b1 = new Binding();
			b1.Source = pfdControl;
			b1.Path = new PropertyPath("FlightState.Pitch");
			b1.Mode = BindingMode.OneWay;
			pfdControl.SetBinding(PitchProperty, b1);
		}

		


		private void InitTicks()
        {
            RollLongTicks = new List<Object>();
            RollShortTicks = new List<Object>();
           
            PitchShortTicks = new List<Object>();
            PitchTextTicks = new List<Object>();
            for (int i = 0; i < 7; i++)
            {
                RollLongTicks.Add(i);
            }
            for (int i = 0; i < 19; i++)
            {
                RollShortTicks.Add(i);
            }
           
            for (int i = 0; i < 13; i++)
            {
                PitchShortTicks.Add(i%2==0?60:40);
            }
            SetPitchTicks(0);
        }

        private void SetPitchTicks(float v)
        {
            float v2 = v / 10;
            int vint = (int)v2;
            int num = 7;

			int longTickSize = 63;
            PitchTextTicks.Clear();
            if (vint < 6 && vint > -6)
            {
                //Normal
                for (int i = 0; i < num; i++)
                {
                    PitchTextTicks.Add(Math.Abs(((vint - i + 3) * 10)).ToString());
                }
                PitchTranslate = (v2 - vint) * longTickSize;
            }
            else if (vint >= 6)
            {
                //Top
                for (int i = 0; i < num; i++)
                {
                    PitchTextTicks.Add(Math.Abs(((9 - i) * 10)).ToString());
                }
                PitchTranslate = (v2 - vint + (vint - 6)) * longTickSize;
            }
            else if (vint <= -6)
            {
                //bottom
                for (int i = 0; i < num; i++)
                {
                    PitchTextTicks.Add(Math.Abs(((-9 + 6 - i) * 10)).ToString());
                }
                PitchTranslate = (v2 - vint + (vint + 6)) * longTickSize;
            }
            leftTextTick.ItemsSource = null;
            leftTextTick.ItemsSource = PitchTextTicks;
            rightTextTick.ItemsSource = null;
            rightTextTick.ItemsSource = PitchTextTicks;
        }
    }
}
