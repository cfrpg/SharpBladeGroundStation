using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpBladeGroundStation.CommunicationLink
{
	public class MissionSender
	{
		
	}

	public enum MissionSenderState
	{
		/// <summary>
		/// 空闲
		/// </summary>
		Idle=0,
		/// <summary>
		/// 先占坑，配置参数
		/// </summary>
		Configing,
		/// <summary>
		/// 正在一个发送阶段
		/// </summary>
		Sending,
		/// <summary>
		/// 正在一个等待阶段
		/// </summary>
		Waiting,
		/// <summary>
		/// 发送完毕
		/// </summary>
		Finished
	}
}
