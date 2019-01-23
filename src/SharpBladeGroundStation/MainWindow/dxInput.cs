using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using System.Windows.Controls;
using System.Timers;
using GMap.NET;
using SharpBladeGroundStation.CommunicationLink;
using SharpBladeGroundStation.DataStructs;
using SharpBladeGroundStation.Map;
using System.MAVLink;
using System.Diagnostics;
using SharpDX.DirectInput;

using Timer = System.Timers.Timer;


namespace SharpBladeGroundStation
{
	public partial class MainWindow : Window
	{
		DirectInput directInput;
		IList<DeviceInstance> joystickList;
		Joystick joystick;
		JoystickState lastJoystickstate;

		Timer dxInputListener;


		void initDxInput()
		{
			directInput = new DirectInput();
			joystickList = directInput.GetDevices(DeviceType.Joystick, DeviceEnumerationFlags.AttachedOnly);
			joystickComboBox.Items.Clear();
			int id = -1;
			if (joystickList.Count > 0)
			{
				for (int i = 0; i < joystickList.Count; i++)
				{
					joystickComboBox.Items.Add(joystickList[i].ProductName);
					if (GCSconfig.JoystickName != "" && joystickList[i].ProductName == GCSconfig.JoystickName)
					{
						joystickComboBox.SelectedIndex = i;
						id = i;
					}
				}
			}
			if (id >= 0)
			{
				joystick = new Joystick(directInput, joystickList[id].InstanceGuid);
				joystick.Acquire();
			}
			else
			{
				joystick = null;
			}
			joystickComboBox.SelectionChanged += JoystickComboBox_SelectionChanged;

			dxInputListener = new Timer(33);
			dxInputListener.Elapsed += DxInputListener_Elapsed;
			if(joystick!=null)
			{
				lastJoystickstate = null;
				dxInputListener.Start();
			}
		}

		private void DxInputListener_Elapsed(object sender, ElapsedEventArgs e)
		{
			if(lastJoystickstate==null)
			{
				lastJoystickstate = joystick.GetCurrentState();
				return;
			}
			JoystickState currState = joystick.GetCurrentState();
			int n = joystick.Capabilities.ButtonCount;
			if (n > 16)
				n = 16;
			for(int i=0;i<n;i++)
			{
				if(currState.Buttons[i]==true&&lastJoystickstate.Buttons[i]==false)
				{
					if(GCSconfig.JoystickKeyMapping[i]!=-1)
					{
						Action a = () => { JoystickEvent(GCSconfig.JoystickKeyMapping[i]); hudWindow?.JoystickEvent(GCSconfig.JoystickKeyMapping[i]); };
						Dispatcher.BeginInvoke(a);
						break;
					}
				}
			}
			lastJoystickstate = currState;
		}

		public void JoystickEvent(int id)
		{
			
		}

		private void JoystickComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			string str = joystickComboBox.SelectedItem.ToString();
			if (str == "")
				return;
			GCSconfig.JoystickName = str;
		}
	}
}
