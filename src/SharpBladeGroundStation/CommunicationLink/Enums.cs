using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpBladeGroundStation.CommunicationLink
{
	public enum PackageParseResult
	{
		/// <summary>
		/// 正确读取
		/// </summary>
		Yes,
		/// <summary>
		/// 未找到头
		/// </summary>
		NoSTX,
		/// <summary>
		/// 校验失败
		/// </summary>
		BadCheckSum,
		/// <summary>
		/// 长度不足
		/// </summary>
		NoEnoughData
	}

	public enum ANOFunction
	{
		VER=0,
		STATUS=1
	}

	public enum LinkState
	{
		Disconnected,
		Connected,
		Connecting
	}

	public enum LinkProtocol : byte
	{
		/// <summary>
		/// 无协议的普通串口
		/// </summary>
		NoLink = 0,
		/// <summary>
		/// MAVLink1.0协议
		/// </summary>
		MAVLink = 1,
		/// <summary>
		/// MAVLink2.0协议
		/// </summary>
		MAVLink2 = 2,		
		/// <summary>
		/// 匿名飞控协议
		/// </summary>
		ANOLink = 3,
		/// <summary>
		/// SharpBlade协议,敬请期待
		/// </summary>
		SBLink
	}
}
