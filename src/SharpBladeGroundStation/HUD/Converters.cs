using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.Windows.Data;

namespace SharpBladeGroundStation.HUD
{
	public class SovietHeadingConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return (float)value * -10f;
		}
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return (float)value * -0.1f;
		}
	}
}
