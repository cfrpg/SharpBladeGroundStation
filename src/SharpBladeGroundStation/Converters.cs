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
			return ((bool)value)?"解锁":"锁定";
		}
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return ((string)value)=="锁定";
		}
	}
}
