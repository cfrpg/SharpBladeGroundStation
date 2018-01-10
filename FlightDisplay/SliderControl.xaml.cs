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
    /// SliderControl.xaml 的交互逻辑
    /// </summary>
    public partial class SliderControl : UserControl
    {
        public static readonly DependencyProperty LongTicksProperty =
           DependencyProperty.Register("LongTicks", typeof(IList<Object>), typeof(SliderControl));
        public IList<Object> LongTicks
        {
            get { return (IList<Object>)GetValue(LongTicksProperty); }
            private set { SetValue(LongTicksProperty, value); }
        }

        public static readonly DependencyProperty ShortTicksProperty =
            DependencyProperty.Register("ShortTicks", typeof(IList<Object>), typeof(SliderControl));
        public IList<Object> ShortTicks
        {
            get { return (IList<Object>)GetValue(ShortTicksProperty); }
            private set { SetValue(ShortTicksProperty, value); }
        }

        public static readonly DependencyProperty TextTicksProperty =
            DependencyProperty.Register("TextTicks", typeof(IList<Object>), typeof(SliderControl));
        public IList<Object> TextTicks
        {
            get { return (IList<Object>)GetValue(TextTicksProperty); }
            private set { SetValue(TextTicksProperty, value); }
        }
        public static readonly DependencyProperty PathTranslateProperty =
            DependencyProperty.Register("PathTranslate", typeof(double), typeof(SliderControl), new PropertyMetadata(0.0));
        public double PathTranslate
        {
            get { return (double)GetValue(PathTranslateProperty); }
            private set { SetValue(PathTranslateProperty, value); }
        }


        public static readonly DependencyProperty ReverseTickProperty =
            DependencyProperty.Register("ReverseTick", typeof(double), typeof(SliderControl), new PropertyMetadata(1.0));
        public double ReverseTick
        {
            get { return (double)GetValue(ReverseTickProperty); }
            set { SetValue(ReverseTickProperty, value); }
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(float), typeof(SliderControl), new PropertyMetadata(0f, SliderControl.OnValuePropertyChanged));
        public float Value
        {
            get { return (float)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }
        public static readonly DependencyProperty TickNumberProperty =
            DependencyProperty.Register("TickNumber", typeof(int), typeof(SliderControl), new PropertyMetadata(5, SliderControl.OnTickNumberPropertyChanged));
        public int TickNumber
        {
            get { return (int)GetValue(TickNumberProperty); }
            set { SetValue(TickNumberProperty, value); }
        }
        public static readonly DependencyProperty MinValueProperty =
            DependencyProperty.Register("MinValue", typeof(int), typeof(SliderControl), new PropertyMetadata(0, SliderControl.OnMinValuePropertyChanged));
        public int MinValue
        {
            get { return (int)GetValue(MinValueProperty); }
            set { SetValue(MinValueProperty, value); }
        }
        public static readonly DependencyProperty MaxValueProperty =
            DependencyProperty.Register("MaxValue", typeof(int), typeof(SliderControl), new PropertyMetadata(int.MaxValue, SliderControl.OnMaxValuePropertyChanged));
        public int MaxValue
        {
            get { return (int)GetValue(MaxValueProperty); }
            set { SetValue(MaxValueProperty, value); }
        }

		public static readonly DependencyProperty TickSizeProperty =
			DependencyProperty.Register("TickSize", typeof(float), typeof(SliderControl), new PropertyMetadata(1f, SliderControl.OnTickSizePropertyChanged));
		public float TickSize
		{
			get { return (float)GetValue(TickSizeProperty); }
			set { SetValue(TickSizeProperty, value); }
		}
		public static void OnTickSizePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
		{
			SliderControl s = sender as SliderControl;
			s.OnTickSizeChanged(e);
		}
		public void OnTickSizeChanged(DependencyPropertyChangedEventArgs e)
		{
			SetTicks();
			SetText(Value);
		}

		public static void OnMaxValuePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            SliderControl s = sender as SliderControl;
            s.OnMaxValueChanged(e);
        }
        public void OnMaxValueChanged(DependencyPropertyChangedEventArgs e)
        {


        }

        public static void OnMinValuePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            SliderControl s = sender as SliderControl;
            s.OnMinValueChanged(e);
        }
        public void OnMinValueChanged(DependencyPropertyChangedEventArgs e)
        {


        }

        public static void OnTickNumberPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            SliderControl s = sender as SliderControl;
            s.OnTickNumberChanged(e);
        }
        public void OnTickNumberChanged(DependencyPropertyChangedEventArgs e)
        {
            SetTicks();
            SetText(Value);
        }

        public static void OnValuePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            SliderControl s = sender as SliderControl;
            s.OnValueChanged(e);
        }
        public void OnValueChanged(DependencyPropertyChangedEventArgs e)
        {
            //this.TextTicks.Clear();            
            SetText((float)e.NewValue);

        }

        public SliderControl()
        {
            InitializeComponent();
            this.InitTicks();
        }

        private void InitTicks()
        {
            this.LongTicks = new List<Object>();
            this.ShortTicks = new List<Object>();
            this.TextTicks = new List<Object>();
            SetTicks();
            SetText(Value);
        }

        private void SetTicks()
        {
            this.LongTicks.Clear();
            this.ShortTicks.Clear();
            for (int i = 0; i < TickNumber * 2; i++)
            {
                this.LongTicks.Add(i);
            }
            for (int i = 0; i < TickNumber * 4 - 1; i++)
            {
                this.ShortTicks.Add(i);
            }
            for (int i = 0; i < TickNumber * 2; i++)
            {
                this.TextTicks.Add(i);
            }

        }

        private void SetText(float v)
        {
			v /= TickSize;  
            int intv = (int)v;
            int t;
            this.TextTicks.Clear();
            for (int i = 0; i < TickNumber * 2; i++)
            {
                t = (intv - i + TickNumber);
                if(t>MaxValue||t<MinValue)
                {
                    this.TextTicks.Add(" ");
                }
                else
                {
                    this.TextTicks.Add((t*TickSize).ToString("F0"));
                }
                
            }
            double ticksize = 600.0 / (2 * TickNumber - 1);
            this.PathTranslate = (v - intv) * ticksize - (0.5 * ticksize);
            this.TextTick.ItemsSource = null;
            this.TextTick.ItemsSource = TextTicks;
        }
    }
}
