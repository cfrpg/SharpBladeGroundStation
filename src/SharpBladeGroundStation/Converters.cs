using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.Windows.Data;

namespace SharpBladeGroundStation
{
	public class FloatTextConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return ((float)value).ToString("F2");
		}
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return float.Parse((string)value);
		}
	}
	public class ArmConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return ((bool)value) ? "解锁" : "锁定";
		}
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return ((string)value) == "锁定";
		}
	}

	public class MilliSecondsConvert : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			int s = (int)((double)value / 1000);
			return (new TimeSpan(0, 0, s)).ToString();
		}
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return 0.0;
		}
	}

	public class ColorBrushConvert:IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			System.Windows.Media.Color c = (System.Windows.Media.Color)value;
			return new System.Windows.Media.SolidColorBrush(c);
		}
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var b = (System.Windows.Media.SolidColorBrush)value;
			return b.Color;
		}
	}
}
