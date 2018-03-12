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
    /// LinkSpeedControl.xaml 的交互逻辑
    /// </summary>
    public partial class LinkSpeedControl : UserControl
    {
        /// <summary>
        /// 发送数据速度
        /// </summary>
        public static readonly DependencyProperty TxSpeedProperty=
            DependencyProperty.Register("TxSpeed",typeof(int),typeof(LinkSpeedControl),new PropertyMetadata(0,new PropertyChangedCallback(LinkSpeedControl.OnTxSpeedPropertyChanged)));

        /// <summary>
        /// 接收数据速度
        /// </summary>
        public static readonly DependencyProperty RxSpeedProperty =
            DependencyProperty.Register("RxSpeed", typeof(int), typeof(LinkSpeedControl), new PropertyMetadata(0, new PropertyChangedCallback(LinkSpeedControl.OnRxSpeedPropertyChanged)));

        /// <summary>
        /// 最大速度，进度条的最大值
        /// </summary>
        public static readonly DependencyProperty MaxSpeedProperty =
            DependencyProperty.Register("MaxSpeed", typeof(int), typeof(LinkSpeedControl),new PropertyMetadata(1000));

        /// <summary>
        /// 发送数据速度
        /// </summary>
        public int TxSpeed
        {
            get { return (int)GetValue(TxSpeedProperty); }
            set { SetValue(TxSpeedProperty, value); }
        }

        /// <summary>
        /// 接收数据速度
        /// </summary>
        public int RxSpeed
        {
            get { return (int)GetValue(RxSpeedProperty); }
            set { SetValue(RxSpeedProperty, value); }
        }

        /// <summary>
        /// 最大速度，进度条的最大值
        /// </summary>
        public int MaxSpeed
        {
            get { return (int)GetValue(MaxSpeedProperty); }
            set { SetValue(MaxSpeedProperty, value); }
        }

        public LinkSpeedControl()
        {
            InitializeComponent();
            
        }

        public static void OnTxSpeedPropertyChanged(DependencyObject sender,DependencyPropertyChangedEventArgs e)
        {
            LinkSpeedControl lsc = sender as LinkSpeedControl;
            lsc.OnTxSpeedChanged(e);
        }

        public void OnTxSpeedChanged(DependencyPropertyChangedEventArgs e)
        {
            progTxSpd.Foreground = getBrush(TxSpeed, MaxSpeed);
            
        }

        public static void OnRxSpeedPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            LinkSpeedControl lsc = sender as LinkSpeedControl;
            lsc.OnRxSpeedChanged(e);
        }

        public void OnRxSpeedChanged(DependencyPropertyChangedEventArgs e)
        {
            progRxSpd.Foreground = getBrush(RxSpeed, MaxSpeed);

        }

        private LinearGradientBrush getBrush(int value,int max)
        {
            double p = (double)value / (double)max;
            GradientStopCollection gsc = new GradientStopCollection();
            gsc.Add(new GradientStop(Colors.Green, 0));          
            gsc.Add(new GradientStop(Colors.Yellow, 0.7/p));
            gsc.Add(new GradientStop(Colors.Red,1.0/p));
           
            return new LinearGradientBrush(gsc,new Point(0,0.5),new Point(1,0.5));
        }
     

    }
}
