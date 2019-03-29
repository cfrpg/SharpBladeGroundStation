using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using GMap.NET.WindowsPresentation;
using SharpBladeGroundStation.CommunicationLink;

namespace SharpBladeGroundStation.DataStructs
{
    /// <summary>
    /// 描述任务项目的类,理论上为抽象类,实际上就是抽象类orz
    /// </summary>
    public abstract class MissionItem :INotifyPropertyChanged
    {
        protected int id;
        protected ObservableCollection<MissionItem> childItems;
        //List<GMapMarker> markers;

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
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ID"));
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
        //public List<GMapMarker> Markers
        //{
        //    get { return markers; }
        //    set { markers = value; }
        //}

        public MissionItem()
        {
            id = -1;
            childItems = new ObservableCollection<MissionItem>();
        }

        public virtual void GenerateMissionItems(Vehicle v, List<LinkPackage> packageList)
        {

        }

        /// <summary>
        /// 在下一层第ID元素处插入元素
        /// </summary>
        /// <param name="item">要插入的元素</param>
        /// <param name="pos">插入位置</param>
        protected virtual void insertMissionItem(MissionItem item,int pos)
        {
			if(pos>=childItems.Count)
			{
				childItems.Add(item);
				return;
			}
            for (int i = childItems.Count-1; i >= 0; i--)
            {
                if (childItems[i].ID <= pos)
                {
                    childItems.Insert(i, item);
                    break;
                }
            }
        }

        /// <summary>
        /// 移除指定的元素
        /// </summary>
        /// <param name="id">要移除的元素ID</param>
        protected virtual void removeMissionItemAt(int pos)
        {
			for (int i = childItems.Count - 1; i >= 0; i--)
			{
				if (childItems[i].ID == pos)
				{
					childItems.RemoveAt(i);
					break;
				}
				if (childItems[i].ID < pos)
				{
					childItems[i].removeMissionItemAt(pos);
					break;
				}
			}
        }

        protected virtual int rebuildID(int pos)
        {
            ID = pos;
            pos++;
            for(int i=0;i<childItems.Count;i++)
            {
                pos = childItems[i].rebuildID(pos);
            }
            return pos;
        }

        protected void NotifyPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
