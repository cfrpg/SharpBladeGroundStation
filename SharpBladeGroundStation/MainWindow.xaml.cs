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
using SharpBladeGroundStation.CommLink;

namespace SharpBladeGroundStation
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        SerialLink link;
        LinkPackage package;
        string msg = "";

        PortScanner portscanner;

        public MainWindow()
        {
            InitializeComponent();
            //link = new SerialLink("COM3", LinkProtocol.MAVLink);
            //link.OnReceivePackage += Link_OnReceivePackage;

            portscanner = new PortScanner(LinkProtocol.ANOLink, 115200, 20480, 1);
            portscanner.OnFindPort += Portscanner_OnFindPort;
        }

        private void Portscanner_OnFindPort(PortScanner sender, EventArgs e)
        {

        }

        private void Link_OnReceivePackage(SerialLink sender, EventArgs e)
        {
            while (link.ReceivedPackageQueue.Count != 0)
            {
                package = link.ReceivedPackageQueue.Dequeue();
                msg += package.ToString() + System.Environment.NewLine;

            }
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {

            msg = "";
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            if (portscanner.IsStarted)
            {
                portscanner.Stop();
            }
            else
            {
                portscanner.Start();
            }
        }
    }
}
