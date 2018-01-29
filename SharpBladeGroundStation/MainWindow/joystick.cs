using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using GMap.NET;
using GMap.NET.WindowsPresentation;
using SharpBladeGroundStation.Map;
using SharpBladeGroundStation.Map.Markers;
using SharpBladeGroundStation.Configuration;
using SharpBladeGroundStation.DataStructs;
using SlimDX.DirectInput;


namespace SharpBladeGroundStation
{
	public partial class MainWindow : Window
	{
		DirectInput dxinput;
		IList<DeviceInstance> joysticks;
		Joystick currentJoystick;
		JoystickState lastJoystickState;
		JoystickState joystickState;
		System.Timers.Timer joystickTimer;

		void getJoysticks()
		{
			dxinput = new DirectInput();
			joysticks = dxinput.GetDevices(DeviceType.Joystick, DeviceEnumerationFlags.AttachedOnly);
			lastJoystickState = new JoystickState();
			joystickState = new JoystickState();
			if(joysticks.Count>0)
			{
				currentJoystick = new Joystick(dxinput, joysticks[0].InstanceGuid);
				currentJoystick.Properties.SetRange(-1000, 1000);
				currentJoystick.Acquire();
			}
			joystickTimer = new System.Timers.Timer(20);
			joystickTimer.Elapsed += JoystickTimer_Elapsed;
			joystickTimer.Start();
		}

		private void JoystickTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			if (currentJoystick == null)
				return;
			lastJoystickState = joystickState;
			joystickState = currentJoystick.GetCurrentState();
			

		}
	}
}
