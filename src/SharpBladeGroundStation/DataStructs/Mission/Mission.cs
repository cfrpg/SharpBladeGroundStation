using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpBladeGroundStation.DataStructs
{
    /// <summary>
    /// 描述飞行任务的类
    /// </summary>
    public class Mission:MissionItem
    {
        string name;

        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                NotifyPropertyChanged("Name");
            }
        }        
    }
}
