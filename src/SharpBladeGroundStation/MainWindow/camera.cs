using System;
using System.Windows;
using FlightDisplay;
using SharpBladeGroundStation.CommLink;
using SharpBladeGroundStation.DataStructs;
using WPFVideoLib.CLEye;

namespace SharpBladeGroundStation
{
	public partial class MainWindow : Window
	{
		bool camearEnabled = false;
		private void initCamera()
		{
			int a = CLEyeCameraDevice.CameraCount;
			if(a>0)
			{
				camera.Device.Create(CLEyeCameraDevice.CameraUUID(0));
				camera.Device.Zoom = -50;
				camera.Device.Start();
				camearEnabled = true;
				pfd.Transparent = true;
			}
		}

		private void closeCamera()
		{
			if(camearEnabled)
			{
				camera.Device.Stop();
				camera.Device.Destroy();
			}
		}
	}
}
