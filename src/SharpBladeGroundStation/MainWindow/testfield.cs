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
        private void button_Click(object sender, RoutedEventArgs e)
        {
			//MessageBox.Show("Only for developers.", "Orz");
			//SpeechSynthesizer ss = new SpeechSynthesizer();
			//ss.Rate = 0;
			//ss.Speak("鹅鹅鹅鹅鹅鹅鹅鹅鹅嗯，鹅鹅鹅鹅鹅鹅鹅鹅鹅嗯，启动失败。");
			//MessageBox.Show("黑科技启动失败", "Orz");
			//replayLog();

			//triggerCamera();
			// testCamera();			
			//loadHospital();
			drawLine();
			loadCity();
			//getpos();
			//loaddist();
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
		void loaddist()
		{
			StreamReader sr = new StreamReader("E:\\temp\\distpos.txt");
			string str;
			string name;
			int lvl;
			double lon, lat;
			MapRouteData orz = new MapRouteData(gmap);

			while (!sr.EndOfStream)
			{
				str = sr.ReadLine();
				string[] strs = str.Split('\t');				
				lon = double.Parse(strs[2]);				
				lat = double.Parse(strs[1]);
				//GMapMarker m = new GMapMarker(PositionHelper.WGS84ToGCJ02(new PointLatLng(lat, lon)));
				GMapMarker m = new GMapMarker(new PointLatLng(lat, lon));
				WayPointMarker wp = new WayPointMarker(orz, m, "0",strs[0]);
				m.Shape = wp;
				m.ZIndex = 99999;
				gmap.Markers.Add(m);
				CreateCircle(lat, lon, 150);
			}
		}
		void loadHospital()
		{
			StreamReader sr = new StreamReader("E:\\temp\\hospos.txt");
			string str;
			string name;
			int lvl;
			double lon, lat;
			MapRouteData orz = new MapRouteData(gmap);

			while (!sr.EndOfStream)
			{
				name = sr.ReadLine();
				lvl = int.Parse(sr.ReadLine());
				str = sr.ReadLine();
				lon = double.Parse(str);
				str = sr.ReadLine();
				lat = double.Parse(str);
				//GMapMarker m = new GMapMarker(PositionHelper.WGS84ToGCJ02(new PointLatLng(lat, lon)));
				GMapMarker m = new GMapMarker(new PointLatLng(lat, lon));
				WayPointMarker wp = new WayPointMarker(orz, m, lvl.ToString(), name);
				m.Shape = wp;
				m.ZIndex = 99999;
				gmap.Markers.Add(m);
			}
		}

		void getpos()
		{
			StreamWriter sw = new StreamWriter("E:\\temp\\fence.txt");
			foreach(var p in newroute.Points)
			{
				PointLatLng wgs = PositionHelper.GCJ02ToWGS84(p);
				var p1 = PositionHelper.WGS84ToMercator(wgs.Lat, wgs.Lng);
				sw.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}\t{5}", p.Lat, p.Lng, wgs.Lat, wgs.Lng, p1.Item1, p1.Item2);
			}
			sw.Close();
		}
		void loadCity()
		{
			StreamReader sr = new StreamReader("E:\\temp\\cityres.txt");
			string str;
			string name;
			int lvl;
			double lon, lat;
			MapRouteData orz = new MapRouteData(gmap);

			while (!sr.EndOfStream)
			{
				str = sr.ReadLine();
				string[] strs = str.Split('\t');
				name = strs[0];
				lvl = int.Parse(strs[2]);	
				lon = double.Parse(strs[3]);
				lat = double.Parse(strs[4]);
				if (lat < fun(lon))
				{
					//GMapMarker m = new GMapMarker(PositionHelper.WGS84ToGCJ02(new PointLatLng(lat, lon)));
					GMapMarker m = new GMapMarker(new PointLatLng(lat, lon));
					WayPointMarker wp = new WayPointMarker(orz, m, strs[2], name);
					m.Shape = wp;
					m.ZIndex = 99999;
					gmap.Markers.Add(m);					
						CreateCircle(lat, lon, 150);
				}
			}
		}
		void drawLine()
		{
			List<PointLatLng> points = new List<PointLatLng>();
			
			for(double lon=94;lon<128;lon+=0.1)
			{
				points.Add(new PointLatLng(fun(lon), lon));
			}
			GMapRoute route = new GMapRoute(points);
			route.RegenerateShape(gmap);
			if (route.Shape != null)
			{
				((System.Windows.Shapes.Path)route.Shape).Stroke = new SolidColorBrush(Colors.Red);
				((System.Windows.Shapes.Path)route.Shape).StrokeThickness = 4;
				((System.Windows.Shapes.Path)route.Shape).Opacity = 1;
				((System.Windows.Shapes.Path)route.Shape).Effect = null;
			}
			gmap.Markers.Add(route);
		}

		private void CreateCircle(double lat, Double lon, double radius)
		{
			PointLatLng point = new PointLatLng(lat, lon);
			int segments = 32;

			List<PointLatLng> gpollist = new List<PointLatLng>();
			
			for (int i = 0; i < segments; i++)
			{
				PointLatLng p = FindPointAtDistanceFrom(point, 2*Math.PI/segments* i, radius);
				gpollist.Add(p);
				
			}

			GMapPolygon gpol = new GMapPolygon(gpollist);
			
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
		

		double fun(double lon)
		{
			double k = 0.763284, b = -47.095150;
			return k * lon + b;
		}
	}
}
