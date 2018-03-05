using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpBladeGroundStation.DataStructs
{
    /// <summary>
    /// 描述任务项目的类,理论上为抽象类
    /// </summary>
    public class MissionItem
    {
        protected int id;
        protected List<MissionItem> childItems;

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

        public MissionItem()
        {
            id = -1;
            childItems = new List<MissionItem>();
        }
       
    }
}
