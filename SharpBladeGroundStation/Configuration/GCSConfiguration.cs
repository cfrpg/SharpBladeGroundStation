using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SharpBladeGroundStation.Configuration
{
	[Serializable]
	public class GCSConfiguration
	{
		/// <summary>
		/// 曲线最小采样间隔(ms)
		/// </summary>
		public int PlotTimeInterval { get; set; } = 100;
		/// <summary>
		/// 航线显示最大点数
		/// </summary>
		public int MaxCoursePoint { get; set; } = 100;
		/// <summary>
		/// 航线绘制采样间隔(ms)
		/// </summary>
		public int CourseTimeInterval { get; set; } = 500;
		/// <summary>
		/// 默认波特率
		/// </summary>
		public int BaudRate { get; set; } = 115200;
		public static GCSConfiguration DefaultConfig()
		{
			return new GCSConfiguration
			{
				PlotTimeInterval = 100,
				MaxCoursePoint = 100,
				CourseTimeInterval = 500,
				BaudRate = 115200
			};
		}
	}
}
