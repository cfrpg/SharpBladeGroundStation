using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GMap.NET.WindowsPresentation;

namespace SharpBladeGroundStation.DataStructs
{
    /// <summary>
    /// 描述任务项目的类,理论上为抽象类,实际上就是抽象类orz
    /// </summary>
    public abstract class MissionItem
    {
        protected int id;
        protected List<MissionItem> childItems;
        List<GMapMarker> markers;

        /// <summary>
        /// 子项
        /// </summary>
        public List<MissionItem> ChildItems
        {
            get { return childItems; }
            set { childItems = value; }
        }

        /// <summary>
        /// 顺序编号,从0开始
        /// </summary>
        public int ID
        {
            get { return id; }
            set { id = value; }
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
            childItems = new List<MissionItem>();
        }
       
    }
}
