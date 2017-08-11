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
    /// CompassControl.xaml 的交互逻辑
    /// </summary>
    public partial class CompassControl : UserControl
    {

        public static readonly DependencyProperty LongTicksProperty =
            DependencyProperty.Register("LongTicks", typeof(IList<Object>), typeof(CompassControl));

        public IList<Object> LongTicks
        {
            get { return (IList<Object>)GetValue(LongTicksProperty); }
            private set { SetValue(LongTicksProperty, value); }
        }

        public static readonly DependencyProperty ShortTicksProperty =
            DependencyProperty.Register("ShortTicks", typeof(IList<Object>), typeof(CompassControl));

        public IList<Object> ShortTicks
        {
            get { return (IList<Object>)GetValue(ShortTicksProperty); }
            private set { SetValue(ShortTicksProperty, value); }
        }

        public static readonly DependencyProperty TextTicksProperty =
            DependencyProperty.Register("TextTicks", typeof(IList<Object>), typeof(CompassControl));

        public IList<Object> TextTicks
        {
            get { return (IList<Object>)GetValue(TextTicksProperty); }
            set { SetValue(TextTicksProperty, value); }
        }
        public static readonly DependencyProperty LongTickLengthProperty =
            DependencyProperty.Register("LongTickLength", typeof(double), typeof(CompassControl));
        public double LongTickLength
        {
            get { return (double)GetValue(LongTickLengthProperty); }
            set { SetValue(LongTickLengthProperty, value); }
        }
        public static readonly DependencyProperty ShortTickLengthProperty =
            DependencyProperty.Register("ShortTickLength", typeof(double), typeof(CompassControl));
        public double ShortTickLength
        {
            get { return (double)GetValue(ShortTickLengthProperty); }
            set { SetValue(ShortTickLengthProperty, value); }
        }

        public static readonly DependencyProperty HeadingProperty =
            DependencyProperty.Register("Heading", typeof(int), typeof(CompassControl), new PropertyMetadata(0, CompassControl.OnHeadingPropertyChanged));
        public int Heading
        {
            get { return (int)GetValue(HeadingProperty); }
            set { SetValue(HeadingProperty, value); }
        }
        public static void OnHeadingPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            CompassControl s = sender as CompassControl;
            s.OnHeadingChanged(e);
        }
        public void OnHeadingChanged(DependencyPropertyChangedEventArgs e)
        {
            this.Rotation = -Heading;
        }

        public static readonly DependencyProperty RotationProperty =
            DependencyProperty.Register("Rotation", typeof(int), typeof(CompassControl));

        public int Rotation
        {
            get { return (int)GetValue(RotationProperty); }
            private set { SetValue(RotationProperty, value); }
        }

        public CompassControl()
        {
            InitializeComponent();
            InitTicks();
        }

        private void InitTicks()
        {
            this.LongTicks = new List<Object>();
            for(int i=0;i<36;i++)
            {
                this.LongTicks.Add(i);
               
            }
            this.ShortTicks = new List<Object>();
            for(int i=0;i<72;i++)
            {
                this.ShortTicks.Add(i);
            }
            this.TextTicks = new List<Object>();
            for(int i=0;i<12;i++)
            {
                int d = ((i -3) + 12) % 12;
                this.TextTicks.Add((d * 3).ToString());
            }
            this.TextTicks[0] = "W";
            this.TextTicks[3] = "N";
            this.TextTicks[6] = "E";
            this.TextTicks[9] = "S";
            Binding b1 = new Binding();
            b1.Source = compass;
            b1.Path = new PropertyPath("Width");
            b1.Converter = new CompassTickConverter();
            b1.Mode = BindingMode.OneWay;
            this.SetBinding(LongTickLengthProperty, b1);
            Binding b2 = new Binding();
            b2.Source = compass;
            b2.Path = new PropertyPath("LongTickLength");
            b2.Converter = new HalfConverter();
            b2.Mode = BindingMode.OneWay;
            this.SetBinding(ShortTickLengthProperty, b2);
        }
    }
}
