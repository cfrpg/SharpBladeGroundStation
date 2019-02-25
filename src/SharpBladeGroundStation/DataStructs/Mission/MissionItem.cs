using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using GMap.NET.WindowsPresentation;

namespace SharpBladeGroundStation.DataStructs
{
    /// <summary>
    /// 描述任务项目的类,理论上为抽象类,实际上就是抽象类orz
    /// </summary>
    public abstract class MissionItem :INotifyPropertyChanged
    {
        protected int id;
        protected ObservableCollection<MissionItem> childItems;
        List<GMapMarker> markers;

        public event PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// 子项
        /// </summary>
        public ObservableCollection<MissionItem> ChildItems
        {
            get { return childItems; }
            set
            {
                childItems = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ChildItems"));
            }
        }

        /// <summary>
        /// 顺序编号,从0开始
        /// </summary>
        public int ID
        {
            get { return id; }
            set
            {
                id = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ChildItems"));
            }
        }

        /// <summary>
        /// 子项是否可见
        /// </summary>
        public virtual bool ChildItemVisible
        {
            get { return true; }
        }

        /// <summary>
        /// 显示在地图上的标识
        /// </summary>
        public List<GMapMarker> Markers
        {
            get { return markers; }
            set { markers = value; }
        }

        public MissionItem()
        {
            id = -1;
            childItems = new ObservableCollection<MissionItem>();
        }

        protected void NotifyPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }


    }
}
