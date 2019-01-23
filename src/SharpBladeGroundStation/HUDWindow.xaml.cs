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
using System.Windows.Shapes;
using System.Threading;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Diagnostics;
using AForge.Video;
using AForge.Video.DirectShow;
using Geb.Video.FFMPEG;
using SharpBladeGroundStation.HUD;
using Microsoft.Xna.Framework.Input;

using Keyboard = Microsoft.Xna.Framework.Input.Keyboard;




namespace SharpBladeGroundStation
{
	/// <summary>
	/// HUDWindow.xaml 的交互逻辑
	/// </summary>
	public partial class HUDWindow : Window
	{
		VideoFileReader reader;

		Dictionary<Key, HUDBase> hudCollection;

		public HUDWindow(MainWindow mw)
		{
			InitializeComponent();
			mainwin = mw;
			reader = new VideoFileReader();

		}

		MainWindow mainwin;

		public MainWindow Mainwin
		{
			get { return mainwin; }
			set { mainwin = value; }
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			hudCollection = new Dictionary<Key, HUDBase>();
			navhud.Vehicle = Mainwin.CurrentVehicle;
			//sshud.Vehicle = Mainwin.CurrentVehicle;
			pfdhud.Vehicle = Mainwin.CurrentVehicle;
			hudCollection.Add(Key.D1, navhud);
			hudCollection.Add(Key.D2, pfdhud);
			//hudCollection.Add(Key.D3, sshud);
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			cameraPlayer.Close();
			logPlayer.Close();
		}

		private void Window_Initialized(object sender, EventArgs e)
		{
			
			
		}
			

		private void Window_KeyDown(object sender, KeyEventArgs e)
		{
			KeyEvent(sender, e);
		}

		public void KeyEvent(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.R)
			{
				navhud.SwitchAlt();
			}
			if (!hudCollection.ContainsKey(e.Key))
			{
				e.Handled = false;
				return;
			}
			foreach (var v in hudCollection)
			{
				v.Value.Visibility = Visibility.Hidden;
			}
			hudCollection[e.Key].Visibility = Visibility.Visible;
		}

		public void JoystickEvent(int id)
		{
            if(id==0)
                navhud.SwitchAlt();
        }
	}


}
