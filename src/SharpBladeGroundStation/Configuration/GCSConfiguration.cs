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
		string logPath;
		bool autoLog;
		string cameraName;
		bool autoRecord;

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
		/// <summary>
		/// 保存日志的路径
		/// </summary>
		public string LogPath
		{
			get { return logPath; }
			set
			{
				logPath = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("LogPath"));
			}
		}
		/// <summary>
		/// 是否自动保存日志
		/// </summary>
		public bool AutoLog
		{
			get { return autoLog; }
			set
			{
				autoLog = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("AutoLog"));
			}
		}

		public string CameraName
		{
			get { return cameraName; }
			set
			{
				cameraName = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CameraName"));
			}
		}

		public bool AutoRecord
		{
			get { return autoRecord; }
			set
			{
				autoRecord = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("AutoRecord"));
			}
		}

		public static GCSConfiguration DefaultConfig()
		{
			return new GCSConfiguration
			{
				PlotTimeInterval = 100,
				MaxCoursePoint = 500,
				CourseTimeInterval = 500,
				BaudRate = 115200,
				LogPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\SharpBladeGS",
				autoLog = true,
				cameraName = "",
				autoRecord = false
			};
		}
	}
}
