using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpBladeGroundStation.Configuration
{
	/// <summary>
	/// 地图中心位置的设置
	/// </summary>
	public enum MapCenterPositionConfig
	{
		/// <summary>
		/// 固定在老校区图书馆
		/// </summary>
		Library=0,
		/// <summary>
		/// 跟随飞行器
		/// </summary>
		FollowUAV,
		/// <summary>
		/// 上一个航点
		/// </summary>
		LastWayPoint,
		/// <summary>
		/// 下一个航点
		/// </summary>
		NextWayPoint,
		/// <summary>
		/// 可以随便拖拽
		/// </summary>
		Free
	}
}
