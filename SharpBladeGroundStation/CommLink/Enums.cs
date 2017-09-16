using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpBladeGroundStation.CommLink
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

	public enum LinkProtocol
	{
		MAVLink,
		ANOLink
	}
}
