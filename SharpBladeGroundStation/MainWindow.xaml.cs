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

using System.Diagnostics;

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
			portscanner.Start();
        }

        private void Portscanner_OnFindPort(PortScanner sender, PortScannerEventArgs e)
        {
			Debug.WriteLine("[main] find port {0}", e.Link.Port.PortName);
			portscanner.Stop();
			if (link != null)
				return;
			link = e.Link;
			link.OnReceivePackage += Link_OnReceivePackage;
			link.OpenPort();
        }

		private void Link_OnReceivePackage(SerialLink sender, EventArgs e)
        {
            while (link.ReceivedPackageQueue.Count != 0)
            {
                package = link.ReceivedPackageQueue.Dequeue();
                
            }
        }       
    }
}
