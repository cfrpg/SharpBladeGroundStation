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
using System.IO.Ports;
using System.Threading;
using SharpBladeGroundStation.CommunicationLink.BootLoader;
using SharpDX.DirectInput;

using Keyboard = Microsoft.Xna.Framework.Input.Keyboard;
using MessageBox = System.Windows.MessageBox;
using Color = System.Windows.Media.Color;

namespace SharpBladeGroundStation
{
	public partial class MainWindow : Window
	{
		private void button_Click(object sender, RoutedEventArgs e)
		{
            //noMagic();
            //replayLog();
            //triggerCamera();
            // testCamera();
            //setScreen();
            //caliLevel();
            //currentVehicle.GpsState.ForceSetHome();
            //homeMarker.Position = PositionHelper.WGS84ToGCJ02(currentVehicle.GpsState.HomePosition);
            //copyRouteData();
            //talkToBL();
            //testJoystick();
            addMission();
		}
		
        void addMission()
        {
            missionManager.MissionList.Add(new Mission() { ID = 0, Name = "Mission 1" });
            missionManager.MissionList[0].ChildItems.Add(new Waypoint(1));
            missionManager.MissionList[0].ChildItems.Add(new Waypoint(2));
            missionManager.MissionList[0].ChildItems.Add(new Waypoint(4));
			missionManager.MissionList[0].ChildItems.Add(new Waypoint(5));
			missionManager.MissionList[0].ChildItems.Add(new Waypoint(6));
			missionManager.MissionList[0].ChildItems.Add(new Waypoint(7));
			missionManager.MissionList[0].ChildItems[1].ChildItems.Add(new MissionAction() { ID = 3 });
        }
		
		void testJoystick()
		{
			DirectInput di = new DirectInput();
			var all=di.GetDevices(DeviceType.Joystick, DeviceEnumerationFlags.AttachedOnly);
			Joystick js = new Joystick(di, all[0].InstanceGuid);
			js.Acquire();
			JoystickState jss = js.GetCurrentState();
			MessageBox.Show(jss.ToString());
			
		}
        	

		void caliLevel()
		{
			MAVLinkPackage package = new MAVLinkPackage((byte)MAVLINK_MSG_ID.COMMAND_LONG,currentVehicle.Link);			
			package.System = 255;
			package.Component = 1;
			package.AddData(0f);
			package.AddData(0f);
			package.AddData(0f);
			package.AddData(0f);
			package.AddData(2f);
			package.AddData(0f);
			package.AddData(0f);
			package.AddData((ushort)241);//MAV_CMD_PREFLIGHT_CALIBRATION
			package.AddData((byte)currentVehicle.ID);
			package.AddData((byte)0);			
			package.AddData((byte)0);
			package.SetVerify();

			currentVehicle.Link.SendPackage(package, true);


		}

		void testCamera()
		{
			MessageBox.Show(currentVehicle.Camera.GetScreenPosition(new Microsoft.Xna.Framework.Vector3(0f, 0f, 1f)).ToString());
		}
		void triggerCamera()
		{
			MAVLinkPackage package = new MAVLinkPackage((byte)MAVLINK_MSG_ID.COMMAND_LONG,currentVehicle.Link);
			package.Sequence = 1;
			package.System = 255;
			package.Component = 0;
			//package.Function = (byte)MAVLINK_MSG_ID.COMMAND_LONG;
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
			//currentVehicle.Link.SendPackageQueue.Enqueue(package);
			currentVehicle.Link.SendPackage(package,true);
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

		private void CreateCircle(double lat, Double lon, double radius, Color c, double o)
		{
			PointLatLng point = new PointLatLng(lat, lon);
			int segments = 32;

			List<PointLatLng> gpollist = new List<PointLatLng>();

			for (int i = 0; i < segments; i++)
			{
				PointLatLng p = FindPointAtDistanceFrom(point, 2 * Math.PI / segments * i, radius);
				gpollist.Add(p);
			}

			GMapPolygon gpol = new GMapPolygon(gpollist);
			gpol.RegenerateShape(gmap);
			((System.Windows.Shapes.Path)gpol.Shape).StrokeThickness = 0;
			((System.Windows.Shapes.Path)gpol.Shape).Opacity = o;
			((System.Windows.Shapes.Path)gpol.Shape).Fill = new SolidColorBrush(c);
			gmap.Markers.Add(gpol);
		}

		PointLatLng FindPointAtDistanceFrom(PointLatLng startPoint, double initialBearingRadians, double distanceKilometres)
		{
			const double radiusEarthKilometres = 6371.01;
			var distRatio = distanceKilometres / radiusEarthKilometres;
			var distRatioSine = Math.Sin(distRatio);
			var distRatioCosine = Math.Cos(distRatio);

			var startLatRad = DegreesToRadians(startPoint.Lat);
			var startLonRad = DegreesToRadians(startPoint.Lng);

			var startLatCos = Math.Cos(startLatRad);
			var startLatSin = Math.Sin(startLatRad);

			var endLatRads = Math.Asin((startLatSin * distRatioCosine) + (startLatCos * distRatioSine * Math.Cos(initialBearingRadians)));

			var endLonRads = startLonRad + Math.Atan2(
						  Math.Sin(initialBearingRadians) * distRatioSine * startLatCos,
						  distRatioCosine - startLatSin * Math.Sin(endLatRads));

			return new PointLatLng(RadiansToDegrees(endLatRads), RadiansToDegrees(endLonRads));
		}

		public static double DegreesToRadians(double degrees)
		{
			const double degToRadFactor = Math.PI / 180;
			return degrees * degToRadFactor;
		}

		public static double RadiansToDegrees(double radians)
		{
			const double radToDegFactor = 180 / Math.PI;
			return radians * radToDegFactor;
		}

		void noMagic()
		{
			//MessageBox.Show("Only for developers.", "Orz");
			SpeechSynthesizer ss = new SpeechSynthesizer();
			ss.Rate = 0;
			ss.Speak("鹅鹅鹅鹅鹅鹅鹅鹅鹅嗯，鹅鹅鹅鹅鹅鹅鹅鹅鹅嗯，启动失败。");
			MessageBox.Show("黑科技启动失败", "Orz");
		}
	}


}
