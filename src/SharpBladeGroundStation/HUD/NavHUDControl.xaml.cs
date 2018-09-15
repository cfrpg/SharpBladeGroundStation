using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using Microsoft.Xna.Framework;

namespace SharpBladeGroundStation.HUD
{
	/// <summary>
	/// NavHUDControl.xaml 的交互逻辑
	/// </summary>
	public partial class NavHUDControl : HUDBase
	{

		public static readonly DependencyProperty HorizonTranlationProperty =
			DependencyProperty.Register("HorizonTranlation", typeof(float), typeof(NavHUDControl), new PropertyMetadata(0f, NavHUDControl.OnHorizonTranlationPropertyChanged));
		public float HorizonTranlation
		{
			get { return (float)GetValue(HorizonTranlationProperty); }
			set { SetValue(HorizonTranlationProperty, value); }
		}

		public static readonly DependencyProperty ClimbRateTranlationProperty =
			DependencyProperty.Register("ClimbRateTranlation", typeof(float), typeof(NavHUDControl), new PropertyMetadata(0f, NavHUDControl.OnClimbRateTranlationPropertyChanged));
		public float ClimbRateTranlation
		{
			get { return (float)GetValue(ClimbRateTranlationProperty); }
			set { SetValue(ClimbRateTranlationProperty, value); }
		}
		public static void OnClimbRateTranlationPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
		{
			NavHUDControl s = sender as NavHUDControl;
			s.OnClimbRateTranlationChanged(e);
		}
		public void OnClimbRateTranlationChanged(DependencyPropertyChangedEventArgs e)
		{


		}
		public static void OnHorizonTranlationPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
		{
			NavHUDControl s = sender as NavHUDControl;
			s.OnHorizonTranlationChanged(e);
		}
		public void OnHorizonTranlationChanged(DependencyPropertyChangedEventArgs e)
		{
			
		}

		public NavHUDControl() : base()
		{
			InitializeComponent();
		}

		private void heading_Loaded(object sender, RoutedEventArgs e)
		{
			if (DesignerProperties.GetIsInDesignMode(this))
				return;
			vehicle.PropertyChanged += Vehicle_PropertyChanged;

		}

		private void Vehicle_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			switch (e.PropertyName)
			{
				case "FlightState":
					Dispatcher.Invoke(() => {
						HorizonTranlation = MathHelper.Clamp(vehicle.FlightState.Pitch * 6.2f, -120, 120);
						ClimbRateTranlation = MathHelper.Clamp(vehicle.FlightState.ClimbRate * -120f/10f, -120, 120);
					});

					break;

				default:

					break;
			}
		}
	}
}
