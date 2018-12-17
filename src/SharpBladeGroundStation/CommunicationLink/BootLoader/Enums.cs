using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpBladeGroundStation.CommunicationLink.BootLoader
{
	/// <summary>
	/// Supported bootloader board ids,as from USB PID.
	/// </summary>
	public enum BoardID : int
	{
		/// <summary>
		/// PX4 V1 board
		/// </summary>
		PX4FMUV1 = 5,
		/// <summary>
		/// PX4 V2 board
		/// </summary>
		PX4FMUV2 = 9,
		/// <summary>
		/// PX4 V4 board
		/// </summary>
		PX4FMUV4 = 11,
		/// <summary>
		/// PX4 V4PRO board
		/// </summary>
		PX4FMUV4PRO = 13,
		/// <summary>
		/// PX4 V5 board
		/// </summary>
		PX4FMUV5 = 50,
		/// <summary>
		/// PX4 Flow board
		/// </summary>
		PX4Flow = 6,
		/// <summary>
		/// Gumstix AeroCore board
		/// </summary>
		AeroCore = 98,
		/// <summary>
		/// AUAV X2.1 board
		/// </summary>
		AUAVX2_1 = 33,
		/// <summary>
		/// 3DR Radio. This is an arbitrary value unrelated to the PID
		/// </summary>
		_3DRRadio = 78,
		/// <summary>
		/// MindPX V2 board
		/// </summary>
		MINDPXFMUV2 = 88,
		/// <summary>
		/// TAP V1 board
		/// </summary>
		TAPV1 = 64,
		/// <summary>
		/// ASC V1 board
		/// </summary>
		ASCV1 = 65,
		/// <summary>
		///  Crazyflie 2.0 board
		/// </summary>
		Crazyflie2 = 12,
		/// <summary>
		/// NXPHliteV3 board
		/// </summary>
		NXPHliteV3 = 28
	}


	public enum BootloaderCmd : byte
	{
		// protocol bytes
		/// <summary>
		///  'in sync' byte sent before status
		/// </summary>
		PROTO_INSYNC = 0x12,
		/// <summary>
		///  device is using silicon not suitable for the target the bootloader was used for
		/// </summary>
		PROTO_BAD_SILICON_REV = 0x14,
		/// <summary>
		/// end of command
		/// </summary>
		PROTO_EOC = 0x20,

		// Reply bytes
		/// <summary>
		/// INSYNC/OK      - 'ok' response
		/// </summary>
		PROTO_OK = 0x10,
		/// <summary>
		/// INSYNC/FAILED  - 'fail' response
		/// </summary>
		PROTO_FAILED = 0x11,
		/// <summary>
		/// INSYNC/INVALID - 'invalid' response for bad commands
		/// </summary>
		PROTO_INVALID = 0x13,

		// Command bytes
		/// <summary>
		/// NOP for re-establishing sync
		/// </summary>
		PROTO_GET_SYNC = 0x21,
		/// <summary>
		///  get device ID bytes
		/// </summary>
		PROTO_GET_DEVICE = 0x22,
		/// <summary>
		/// erase program area and reset program address
		/// </summary>
		PROTO_CHIP_ERASE = 0x23,
		/// <summary>
		/// set next programming address
		/// </summary>
		PROTO_LOAD_ADDRESS = 0x24,
		/// <summary>
		/// write bytes at program address and increment
		/// </summary>
		PROTO_PROG_MULTI = 0x27,
		/// <summary>
		/// compute & return a CRC
		/// </summary>
		PROTO_GET_CRC = 0x29,
		/// <summary>
		/// boot the application
		/// </summary>
		PROTO_BOOT = 0x30,

		// Command bytes - Rev 2 boootloader only
		/// <summary>
		///  begin verify mode
		/// </summary>
		PROTO_CHIP_VERIFY = 0x24,
		/// <summary>
		/// read bytes at programm address and increment
		/// </summary>
		PROTO_READ_MULTI = 0x28,

		/// <summary>
		/// bootloader protocol revision
		/// </summary>
		INFO_BL_REV = 1,
		/// <summary>
		/// Minimum supported bootlader protocol
		/// </summary>
		BL_REV_MIN = 2,
		/// <summary>
		/// Maximum supported bootloader protocol
		/// </summary>
		BL_REV_MAX = 5,
		/// <summary>
		/// board type
		/// </summary>
		INFO_BOARD_ID = 2,
		/// <summary>
		/// board revision
		/// </summary>
		INFO_BOARD_REV = 3,
		/// <summary>
		/// max firmware size in bytes
		/// </summary>
		INFO_FLASH_SIZE = 4,

		/// <summary>
		/// write size for PROTO_PROG_MULTI, must be multiple of 4
		/// </summary>
		PROG_MULTI_MAX = 64,
		/// <summary>
		/// read size for PROTO_READ_MULTI, must be multiple of 4. Sik Radio max size is 0x28
		/// </summary>
		READ_MULTI_MAX = 0x28
	}


}
