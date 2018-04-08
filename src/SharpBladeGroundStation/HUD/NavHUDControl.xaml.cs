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

namespace SharpBladeGroundStation.HUD
{
    /// <summary>
    /// NavHUDControl.xaml 的交互逻辑
    /// </summary>
    public partial class NavHUDControl : HUDBase
    {
        public NavHUDControl()
        {
            InitializeComponent();
            
        }

        private void heading_Loaded(object sender, RoutedEventArgs e)
        {
            heading.DataContext = vehicle;
			if (DesignerProperties.GetIsInDesignMode(this))
				return;
			vehicle.PropertyChanged += Vehicle_PropertyChanged;
			Canvas.SetTop(tgtind, 245);
			Canvas.SetLeft(tgtind, 245);
        }

		private void Vehicle_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			switch(e.PropertyName)
			{


				default:

					break;
			}
		}
	}
}
