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
    /// HorizonControl.xaml 的交互逻辑
    /// </summary>
    public partial class HorizonControl : UserControl
    {
        public static readonly DependencyProperty RollLongTicksProperty =
            DependencyProperty.Register("RollLongTicks", typeof(IList<Object>), typeof(HorizonControl));
        public IList<Object> RollLongTicks
        {
            get { return (IList<Object>)GetValue(RollLongTicksProperty); }
            private set { SetValue(RollLongTicksProperty, value); }
        }

        public static readonly DependencyProperty RollShortTicksProperty =
            DependencyProperty.Register("RollShortTicks", typeof(IList<Object>), typeof(HorizonControl));
        public IList<Object> RollShortTicks
        {
            get { return (IList<Object>)GetValue(RollShortTicksProperty); }
            private set { SetValue(RollShortTicksProperty, value); }
        }
        public static readonly DependencyProperty PitchLongTicksProperty =
            DependencyProperty.Register("PitchLongTicks", typeof(IList<Object>), typeof(HorizonControl));
        public IList<Object> PitchLongTicks
        {
            get { return (IList<Object>)GetValue(PitchLongTicksProperty); }
            private set { SetValue(PitchLongTicksProperty, value); }
        }
        public static readonly DependencyProperty PitchShortTicksProperty =
            DependencyProperty.Register("PitchShortTicks", typeof(IList<Object>), typeof(HorizonControl));
        public IList<Object> PitchShortTicks
        {
            get { return (IList<Object>)GetValue(PitchShortTicksProperty); }
            private set { SetValue(PitchShortTicksProperty, value); }
        }
        public static readonly DependencyProperty PitchTextTicksProperty =
           DependencyProperty.Register("PitchTextTicks", typeof(IList<Object>), typeof(HorizonControl));
        public IList<Object> PitchTextTicks
        {
            get { return (IList<Object>)GetValue(PitchTextTicksProperty); }
            private set { SetValue(PitchTextTicksProperty, value); }
        }
        public static readonly DependencyProperty PitchTranslateProperty =
            DependencyProperty.Register("PitchTranslate", typeof(double), typeof(HorizonControl), new PropertyMetadata(0.0));
        public double PitchTranslate
        {
            get { return (double)GetValue(PitchTranslateProperty); }
            private set { SetValue(PitchTranslateProperty, value); }
        }


        public static readonly DependencyProperty PitchProperty =
            DependencyProperty.Register("Pitch", typeof(float), typeof(HorizonControl), new PropertyMetadata(0f, HorizonControl.OnPitchPropertyChanged));
        public float Pitch
        {
            get { return (float)GetValue(PitchProperty); }
            set { SetValue(PitchProperty, value); }
        }

        public static readonly DependencyProperty RollProperty =
            DependencyProperty.Register("Roll", typeof(float), typeof(HorizonControl), new PropertyMetadata(0f, HorizonControl.OnRollPropertyChanged));
        public float Roll
        {
            get { return (float)GetValue(RollProperty); }
            set { SetValue(RollProperty, value); }
        }

        public static void OnRollPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            HorizonControl s = sender as HorizonControl;
            s.OnRollChanged(e);
        }
        public void OnRollChanged(DependencyPropertyChangedEventArgs e)
        {


        }

        public static void OnPitchPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            HorizonControl s = sender as HorizonControl;
            s.OnPitchChanged(e);
        }
        public void OnPitchChanged(DependencyPropertyChangedEventArgs e)
        {
            SetPitchTicks((float)e.NewValue);

        }
        public HorizonControl()
        {
            InitializeComponent();
            InitTicks();
        }

        private void InitTicks()
        {
            RollLongTicks = new List<Object>();
            RollShortTicks = new List<Object>();
            PitchLongTicks = new List<Object>();
            PitchShortTicks = new List<Object>();
            PitchTextTicks = new List<Object>();
            for(int i=0;i<7;i++)
            {
                RollLongTicks.Add(i);
            }
            for(int i=0;i<19;i++)
            {
                RollShortTicks.Add(i);
            }
            for(int i=0;i<7;i++)
            {
                PitchLongTicks.Add(i);                
            }
            for(int i=0;i<13;i++)
            {
                PitchShortTicks.Add(i);
            }
            SetPitchTicks(0);
        }

        private void SetPitchTicks(float v)
        {
            float v2 = v / 10;
            int vint = (int)v2;
            int num = 7;
            PitchTextTicks.Clear();
            if(vint<6&&vint>-6)
            {
                //Normal
                for(int i=0;i<num;i++)
                {
                    PitchTextTicks.Add(Math.Abs(((vint - i + 3) * 10)).ToString());
                }
                PitchTranslate = (v2 - vint) * 45;
            }
            else if(vint>=6)
            {
                //Top
                for(int i=0;i<num;i++)
                {
                    PitchTextTicks.Add(Math.Abs(((9 - i) * 10)).ToString());
                }
                PitchTranslate = (v2 - vint + (vint - 6)) * 45;
            }
            else if(vint<=-6)
            {
                //bottom
                for (int i = 0; i < num; i++)
                {
                    PitchTextTicks.Add(Math.Abs(((-9+6 - i) * 10)).ToString());
                }
                PitchTranslate = (v2 - vint + (vint + 6)) * 45;
            }
            leftTextTick.ItemsSource = null;
            leftTextTick.ItemsSource = PitchTextTicks;
            rightTextTick.ItemsSource = null;
            rightTextTick.ItemsSource = PitchTextTicks;
        }
    }
}
