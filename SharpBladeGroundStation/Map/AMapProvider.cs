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
	public abstract class AMapProviderBase : GMapProvider
	{
		public AMapProviderBase()
		{
			MaxZoom = null;
			RefererUrl = "http://www.amap.com/";
			Copyright = string.Format("©{0} 高德 Corporation, ©{0} NAVTEQ, ©{0} Image courtesy of NASA", DateTime.Today.Year);
		}

		public override PureProjection Projection
		{
			get { return MercatorProjection.Instance; }
		}
		GMapProvider[] overlays;
		public override GMapProvider[] Overlays
		{
			get
			{
				if (overlays == null)
				{
					overlays = new GMapProvider[] { this };
				}
				return overlays;
			}
		}
	}

	public class AMapProvider : AMapProviderBase
	{
		public static readonly AMapProvider Instance;

		readonly Guid id = new Guid("c3e83acd-2149-4974-a8e2-d5566dccb22e");
		public override Guid Id
		{
			get { return id; }
		}

		readonly string name = "AMap";
		public override string Name
		{
			get
			{
				return name;
			}
		}

		static AMapProvider()
		{
			Instance = new AMapProvider();
		}

		public override PureImage GetTileImage(GPoint pos, int zoom)
		{
			string url = MakeTileImageUrl(pos, zoom, LanguageStr);

			return GetTileImageUsingHttp(url);
		}

		string MakeTileImageUrl(GPoint pos, int zoom, string language)
		{

			//http://webrd04.is.autonavi.com/appmaptile?x=5&y=2&z=3&lang=zh_cn&size=1&scale=1&style=7
			string url = string.Format(UrlFormat, pos.X, pos.Y, zoom);
			Console.WriteLine("url:" + url);
			return url;
		}

		static readonly string UrlFormat = "http://wprd04.is.autonavi.com/appmaptile?x={0}&y={1}&z={2}&lang=zh_cn&size=1&scale=1&style=7";
	}

	public class AMapSatProvider : AMapProviderBase
	{
		public static readonly AMapSatProvider Instance;

		readonly Guid id = new Guid("05d8b737-355c-43f6-b315-d0c768e3c614");
		public override Guid Id
		{
			get { return id; }
		}

		readonly string name = "AMap";
		public override string Name
		{
			get
			{
				return name;
			}
		}

		static AMapSatProvider()
		{
			Instance = new AMapSatProvider();
		}

		public override PureImage GetTileImage(GPoint pos, int zoom)
		{
			string url = MakeTileImageUrl(pos, zoom, LanguageStr);

			return GetTileImageUsingHttp(url);
		}

		string MakeTileImageUrl(GPoint pos, int zoom, string language)
		{

			//http://webrd04.is.autonavi.com/appmaptile?x=5&y=2&z=3&lang=zh_cn&size=1&scale=1&style=7
			string url = string.Format(UrlFormat, pos.X, pos.Y, zoom);
			Console.WriteLine("url:" + url);
			return url;
		}

		static readonly string UrlFormat = "http://webst04.is.autonavi.com/appmaptile?x={0}&y={1}&z={2}&lang=zh_cn&size=1&scale=1&style=6";
											
	}

	public class AMapHybirdProvider : AMapProviderBase
	{
		public static readonly AMapHybirdProvider Instance;

		readonly Guid id = new Guid("EF3DD303-3F74-4938-BF40-232D0595EE88");
		public override Guid Id
		{
			get { return id; }
		}

		readonly string name = "AMap";
		public override string Name
		{
			get
			{
				return name;
			}
		}
		GMapProvider[] overlays;
		public override GMapProvider[] Overlays
		{
			get
			{
				if (overlays == null)
				{
					overlays = new GMapProvider[] {AMapSatProvider.Instance, this};
				}
				return overlays;
			}
		}

		static AMapHybirdProvider()
		{
			Instance = new AMapHybirdProvider();
		}

		public override PureImage GetTileImage(GPoint pos, int zoom)
		{
			string url = MakeTileImageUrl(pos, zoom, LanguageStr);

			return GetTileImageUsingHttp(url);
		}

		string MakeTileImageUrl(GPoint pos, int zoom, string language)
		{

			//http://webrd04.is.autonavi.com/appmaptile?x=5&y=2&z=3&lang=zh_cn&size=1&scale=1&style=7
			//http://wprd03.is.autonavi.com/appmaptile?x=105185&y=52249&z=17&lang=zh_cn&size=1&scl=2&style=8&ltype=11
			string url = string.Format(UrlFormat, pos.X, pos.Y, zoom);
			Console.WriteLine("url:" + url);
			return url;
		}

		static readonly string UrlFormat = "http://wprd04.is.autonavi.com/appmaptile?x={0}&y={1}&z={2}&lang=zh_cn&size=1&scale=1&style=8";
	}
}
