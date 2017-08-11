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

        public static void OnTickNumberPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            SliderControl s = sender as SliderControl;
            s.OnTickNumberChanged(e);
        }
        public void OnTickNumberChanged(DependencyPropertyChangedEventArgs e)
        {
            this.InitTicks();
        }

        public static void OnValuePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            SliderControl s = sender as SliderControl;
            s.OnValueChanged(e);
        }
        public void OnValueChanged(DependencyPropertyChangedEventArgs e)
        {
            
        }

        public SliderControl()
        {
            InitializeComponent();
            this.InitTicks();
        }

        private void InitTicks()
        {
            this.LongTicks = new List<Object>();
            for (int i = 0; i < TickNumber*2; i++)
            {
                this.LongTicks.Add(i);

            }
            this.ShortTicks = new List<Object>();
            for (int i = 0; i < TickNumber * 4-1; i++)
            {
                this.ShortTicks.Add(i);
            }

            this.TextTicks = new List<Object>();
        }
    }
}
