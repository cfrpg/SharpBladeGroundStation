using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace SharpBladeGroundStation.DataStructs
{
    /// <summary>
    /// 描述飞行任务的类
    /// </summary>
    public class Mission:MissionItem
    {
        string name;
		Color color;

        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                NotifyPropertyChanged("Name");
            }
        }

		public Color Color
		{
			get { return color; }
			set
			{
				color = value;
				NotifyPropertyChanged("Color");
			}
		}
	}
}
