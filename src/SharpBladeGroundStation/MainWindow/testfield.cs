using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Speech.Synthesis;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using FlightDisplay;
using GMap.NET;
using GMap.NET.WindowsPresentation;
using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using SharpBladeGroundStation.CommunicationLink;
using SharpBladeGroundStation.DataStructs;
using SharpBladeGroundStation.Map;
using SharpBladeGroundStation.Map.Markers;
using SharpBladeGroundStation.Configuration;
using System.MAVLink;

namespace SharpBladeGroundStation
{
    public partial class MainWindow : Window
    {

		LogLink logLink;
        private void button_Click(object sender, RoutedEventArgs e)
        {
			//MessageBox.Show("Only for developers.", "Orz");
			//SpeechSynthesizer ss = new SpeechSynthesizer();
			//ss.Rate = 0;
			//ss.Speak("鹅鹅鹅鹅鹅鹅鹅鹅鹅嗯，鹅鹅鹅鹅鹅鹅鹅鹅鹅嗯，启动失败。");
			//MessageBox.Show("黑科技启动失败", "Orz");
			replayLog();

			//triggerCamera();
			// testCamera();
		}
        void testCamera()
        {
           MessageBox.Show(currentVehicle.Camera.GetScreenPosition(new Microsoft.Xna.Framework.Vector3(0f, 0f, 1f)).ToString());

        }
        void triggerCamera()
        {
            MAVLinkPackage package = new MAVLinkPackage();
            package.Sequence = 1;
            package.System = 0;
            package.Component = 0;
            package.Function = (byte)MAVLINK_MSG_ID.COMMAND_LONG;
            package.AddData((float)0);
            package.AddData((float)0);
            package.AddData((float)0);
            package.AddData((float)0);

            package.AddData((float)1);
            package.AddData((float)0);
            package.AddData((float)0);

            package.AddData((ushort)203);//CMD_ID
            package.AddData((byte)1);//TGT_SYS
            package.AddData((byte)0);//TGT_COMP
            
            package.AddData((byte)0);

            
            package.SetVerify();
            currentVehicle.Link.SendPackageQueue.Enqueue(package);
        }

        void setScreen()
        {
            var screens = System.Windows.Forms.Screen.AllScreens;
            var currscn = System.Windows.Forms.Screen.FromPoint(System.Windows.Forms.Cursor.Position);
            if (screens.Count() == 1)
            {
                SpeechSynthesizer ss = new SpeechSynthesizer();
                ss.Rate = 0;
                ss.Speak("鹅鹅鹅鹅鹅鹅鹅鹅鹅嗯，鹅鹅鹅鹅鹅鹅鹅鹅鹅嗯，启动失败。");
                MessageBox.Show("黑科技启动失败", "Orz");
                return;
            }
            int a = 0, b = 0;
            for (int i = 0; i < screens.Count(); i++)
            {
                if (screens[i].DeviceName == currscn.DeviceName)
                {
                    a = (i + 1) % screens.Count();
                    b = i;
                    break;
                }
            }
            this.WindowState = WindowState.Normal;
            this.Top = screens[a].WorkingArea.Top;
            this.Left = screens[a].WorkingArea.Left;
            this.WindowState = WindowState.Maximized;
            hudWindow.WindowState = WindowState.Normal;
            hudWindow.Top = screens[b].WorkingArea.Top;
            hudWindow.Left = screens[b].WorkingArea.Left;
            hudWindow.WindowState = WindowState.Maximized;
        }

		void replayLog()
		{
			portscanner.Stop();
			System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();
			ofd.InitialDirectory = GCSconfig.LogPath;
			ofd.Filter = "日志文件 (*.sblog)|*.sblog|All files (*.*)|*.*";
			var res = ofd.ShowDialog();
			if(res!=System.Windows.Forms.DialogResult.Cancel)
			{
				logLink = new LogLink();
				LoadFileResualt lfr= logLink.OpenFile(ofd.FileName);
				if(lfr==LoadFileResualt.OK)
				{
					currentVehicle.Link = logLink;
					currentVehicle.Link.OnReceivePackage += Link_OnReceivePackage;
                    logLink.Play();
				}
				else
				{
					MessageBox.Show(lfr.ToString(), "orz");
				}
			}
		}
    }
}
