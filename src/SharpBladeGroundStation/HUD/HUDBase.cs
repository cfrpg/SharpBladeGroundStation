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
    public class HUDBase:UserControl
    {
        protected Vehicle vehicle;
        
        public Vehicle Vehicle
        {
            get { return vehicle; }
            set { vehicle = value; }
        }
    }
}
