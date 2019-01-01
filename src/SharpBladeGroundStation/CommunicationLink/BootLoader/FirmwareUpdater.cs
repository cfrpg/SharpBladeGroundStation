using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.IO.Ports;
using System.Threading;

using SharpBladeGroundStation.DataStructs;

namespace SharpBladeGroundStation.CommunicationLink.BootLoader
{
	public class FirmwareUpdater : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		double progress;
		string message;		
		FirmwareImage firmwareImage;

		SerialPort port;
		Thread updateThread;
		double maxprogress;

		public delegate void FirmwareUpdaterEvent(FirmwareUpdaterErrorMsg err);
		public event FirmwareUpdaterEvent UpdateFinished;

		public double Progress
		{
			get { return progress; }
			private set
			{
				progress = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Progress"));
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("NormalizedProgress"));
			}
		}

		public double NormalizedProgress
		{
			get { return progress / maxprogress * 100; }
		}

		public string Message
		{
			get { return message; }
			set
			{
				message = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Message"));
			}
		}
				
		public FirmwareImage FirmwareImage
		{
			get { return firmwareImage; }
			set { firmwareImage = value; }
		}

		public FirmwareUpdater()
		{
			progress = 0;
			maxprogress = 1300;
			message = "";
			//state = FirmwareUpdaterState.Idle;
			updateThread = new Thread(new ThreadStart(updateWorker));
			updateThread.IsBackground = true;
		}

		public void Start()
		{
			updateThread.Start();
		}

		private void updateWorker()
		{
			byte[] buf = new byte[128];
			Progress = 0;
			string[] existedPort = SerialPort.GetPortNames();
			AddUpdateMessage("等待飞控连接...");

			string newPortName = "";
			bool flag = false;
			while (!flag)
			{
				Thread.Sleep(500);
				string[] currPorts = SerialPort.GetPortNames();
				foreach (var p in currPorts)
				{
					flag = true;
					newPortName = p;
					foreach (var s in existedPort)
					{
						if (p == s)
						{
							flag = false;
							newPortName = "";
							break;
						}
					}
					if (flag)
						break;
				}
			}
			AddUpdateMessage("已连接到飞控:{0}", newPortName);

			port = new SerialPort(newPortName, 115200);
			port.Open();

			//Read board info.
			uint rev, bid, flashSize;
			rev = getBoardInfo(BootloaderCmd.INFO_BL_REV);
			AddUpdateMessage("引导版本:{0}", rev);
			bid = getBoardInfo(BootloaderCmd.INFO_BOARD_ID);
			AddUpdateMessage("硬件类型:{0}", ((BoardID)bid).ToString());
			flashSize = getBoardInfo(BootloaderCmd.INFO_FLASH_SIZE);

			AddUpdateMessage("可用空间:{0}", flashSize);

			//Check compatibility.


			//Earse board rom.
			AddUpdateMessage("擦除旧版固件...");

			sendCmd(BootloaderCmd.PROTO_CHIP_ERASE);
			while (port.BytesToRead < 2 || progress < 200)
			{
				if (progress < 200)
				{
					Progress += 1;
					Thread.Sleep(40);
				}
			}
			port.Read(buf, 0, port.BytesToRead);
			if (buf[0] == (byte)BootloaderCmd.PROTO_INSYNC && buf[1] == (byte)BootloaderCmd.PROTO_OK)
			{
				AddUpdateMessage("擦除完成");
			}
			AddUpdateMessage("烧写新固件...");
			//Flash board.
			int pos = 0;
			while (pos < firmwareImage.ImageSize)
			{
				int bytetowrite = firmwareImage.ImageSize - pos;
				if (bytetowrite > (int)BootloaderCmd.PROG_MULTI_MAX)
					bytetowrite = (int)BootloaderCmd.PROG_MULTI_MAX;
				buf[0] = (byte)BootloaderCmd.PROTO_PROG_MULTI;
				buf[1] = (byte)bytetowrite;
				Array.Copy(firmwareImage.Image, pos, buf, 2, bytetowrite);
				buf[2 + bytetowrite] = (byte)BootloaderCmd.PROTO_EOC;
				port.Write(buf, 0, 3 + bytetowrite);
				pos += bytetowrite;
				while (port.BytesToRead < 2) ;
				port.Read(buf, 0, port.BytesToRead);
				if (buf[0] == (byte)BootloaderCmd.PROTO_INSYNC && buf[1] == (byte)BootloaderCmd.PROTO_OK)
				{
					Progress = 200 + pos * 1000 / firmwareImage.ImageSize;

				}
				else
				{
					AddUpdateMessage("Flash error!");
					break;
				}
			}
			AddUpdateMessage("烧写完成");
			AddUpdateMessage("启动飞控...");
			sendCmd(BootloaderCmd.PROTO_BOOT);
			while (progress < 1300)
			{
				Progress += 1;
				Thread.Sleep(10);
			}

			port.Close();
			AddUpdateMessage("固件更新完成");
			UpdateFinished?.Invoke(FirmwareUpdaterErrorMsg.OK);
		}

		private uint getBoardInfo(BootloaderCmd cmd)
		{
			uint res;
			byte[] buf = new byte[128];
			buf[0] = (byte)BootloaderCmd.PROTO_GET_DEVICE;
			buf[1] = (byte)cmd;
			buf[2] = (byte)BootloaderCmd.PROTO_EOC;
			port.DiscardInBuffer();
			port.Write(buf, 0, 3);
			while (port.BytesToRead < 4) ;
			port.Read(buf, 0, port.BytesToRead);
			res = BitConverter.ToUInt32(buf, 0);
			return res;
		}

		private bool sendCmd(BootloaderCmd cmd)
		{
			byte[] buff = new byte[2];
			buff[0] = (byte)cmd;
			buff[1] = (byte)BootloaderCmd.PROTO_EOC;
			port.DiscardInBuffer();
			port.Write(buff, 0, 2);
			return true;
		}

		public void AddUpdateMessage(string str, params object[] args)
		{
			Message += string.Format(str, args) + Environment.NewLine;
		}
	}
		
	public enum FirmwareUpdaterErrorMsg
	{
		OK,
		ConnectionError,
		Incompatible,
		EraseError,
		FlashError,
		VerifyError
	}
}
