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



        public PFDControl()
        {
            InitializeComponent();
           

        }
    }
}
