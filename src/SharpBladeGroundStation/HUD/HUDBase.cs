using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using SharpBladeGroundStation.DataStructs;

namespace SharpBladeGroundStation.HUD
{
	public class HUDBase : UserControl
	{
		protected Vehicle vehicle;
		protected double xScale;
		protected double yScale;

		public Vehicle Vehicle
		{
			get { return vehicle; }
			set { vehicle = value; }
		}

		public double XScale
		{
			get { return xScale; }
			set { xScale = value; }
		}

		public double YScale
		{
			get { return yScale; }
			set { yScale = value; }
		}
		public HUDBase()
		{
			this.Loaded += HUDBase_Loaded;
		}

		private void HUDBase_Loaded(object sender, RoutedEventArgs e)
		{
			this.DataContext = vehicle;
		}
	}
}
