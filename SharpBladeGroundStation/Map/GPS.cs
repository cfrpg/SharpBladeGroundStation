using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Ports;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.Projections;

namespace SharpBladeGroundStation.Map
{
	public abstract class PositionHelper
	{
		private static double Offset = 0.00669342162296594323;
		private static double Axis = 6378245.0;

		public static PointLatLng WGS84ToGCJ02(PointLatLng p)
		{
			if (IsOutOfChina(p))
				return p;
			double dLat = transformLat(p.Lng - 105.0, p.Lat - 35.0);
			double dLon = transformLon(p.Lng - 105.0, p.Lat - 35.0);
			double radLat = p.Lat / 180.0 * Math.PI;
			double magic = Math.Sin(radLat);
			magic = 1 - Offset * magic * magic;
			double sqrtMagic = Math.Sqrt(magic);
			dLat = (dLat * 180.0) / ((Axis * (1 - Offset)) / (magic * sqrtMagic) * Math.PI);
			dLon = (dLon * 180.0) / (Axis / sqrtMagic * Math.Cos(radLat) * Math.PI);
			double mgLat = p.Lat + dLat;
			double mgLon = p.Lng + dLon;
			return new PointLatLng(mgLat, mgLon);
		}

		//https://gist.github.com/anonymous/e7c6f67555099180ce1ae8da4ba2c513
		public static PointLatLng GCJ02ToWGS84(PointLatLng p)
		{
			PointLatLng wgs = p;
			PointLatLng cur, d = new PointLatLng();
			const double err = 1e-7;
			for (int i = 0; i < 5; i++)
			{
				cur = WGS84ToGCJ02(wgs);
				d.Lat = p.Lat - cur.Lat;
				d.Lng = p.Lng - cur.Lng;
				if (Math.Abs(d.Lat) < err && Math.Abs(d.Lng) < err)
					break;
				wgs.Lat += d.Lat;
				wgs.Lng += d.Lng;
			}
			return wgs;
		}
		private static double transformLat(double x, double y)
		{
			double ret = -100.0 + 2.0 * x + 3.0 * y + 0.2 * y * y + 0.1 * x * y
				+ 0.2 * Math.Sqrt(Math.Abs(x));
			ret += (20.0 * Math.Sin(6.0 * x * Math.PI) + 20.0 * Math.Sin(2.0 * x * Math.PI)) * 2.0 / 3.0;
			ret += (20.0 * Math.Sin(y * Math.PI) + 40.0 * Math.Sin(y / 3.0 * Math.PI)) * 2.0 / 3.0;
			ret += (160.0 * Math.Sin(y / 12.0 * Math.PI) + 320 * Math.Sin(y * Math.PI / 30.0)) * 2.0 / 3.0;
			return ret;
		}

		public static double transformLon(double x, double y)
		{
			double ret = 300.0 + x + 2.0 * y + 0.1 * x * x + 0.1 * x * y + 0.1
					* Math.Sqrt(Math.Abs(x));
			ret += (20.0 * Math.Sin(6.0 * x * Math.PI) + 20.0 * Math.Sin(2.0 * x * Math.PI)) * 2.0 / 3.0;
			ret += (20.0 * Math.Sin(x * Math.PI) + 40.0 * Math.Sin(x / 3.0 * Math.PI)) * 2.0 / 3.0;
			ret += (150.0 * Math.Sin(x / 12.0 * Math.PI) + 300.0 * Math.Sin(x / 30.0 * Math.PI)) * 2.0 / 3.0;
			return ret;
		}

		private static bool IsOutOfChina(PointLatLng p)
		{
			if (p.Lat < 0.8293 || p.Lat > 55.8271)
				return true;
			if (p.Lng < 72.004 || p.Lng > 137.8347)
				return true;
			return false;
		}


	}
}
