using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.ComponentModel;

namespace SharpBladeGroundStation.Configuration
{
	[Serializable]
	public class GCSConfiguration : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		int plotTimeInterval;
		int maxCoursePoint;
		int courseTimeInterval;
		int baudRate;

		/// <summary>
		/// 曲线最小采样间隔(ms)
		/// </summary>
		public int PlotTimeInterval
		{
			get { return plotTimeInterval; }
			set
			{
				plotTimeInterval = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("PlotTimeInterval"));
			}
		}
		/// <summary>
		/// 航线显示最大点数
		/// </summary>
		public int MaxCoursePoint
		{
			get { return maxCoursePoint; }
			set
			{
				maxCoursePoint = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("MaxCoursePoint"));
			}
		}
		/// <summary>
		/// 航线绘制采样间隔(ms)
		/// </summary>
		public int CourseTimeInterval
		{
			get { return courseTimeInterval; }
			set
			{
				courseTimeInterval = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CourseTimeInterval"));
			}
		}
		/// <summary>
		/// 默认波特率
		/// </summary>
		public int BaudRate
		{
			get { return baudRate; }
			set
			{
				baudRate = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("BaudRate"));
			}
		}

		public static GCSConfiguration DefaultConfig()
		{
			return new GCSConfiguration
			{
				PlotTimeInterval = 100,
				MaxCoursePoint = 500,
				CourseTimeInterval = 500,
				BaudRate = 115200
			};
		}
	}
}
