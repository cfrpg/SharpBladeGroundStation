using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Speech.Synthesis;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using FlightDisplay;
using GMap.NET;
using GMap.NET.WindowsPresentation;
using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using SharpBladeGroundStation.CommunicationLink;
using SharpBladeGroundStation.DataStructs;
using SharpBladeGroundStation.Map;
using SharpBladeGroundStation.Map.Markers;
using SharpBladeGroundStation.Configuration;
using System.MAVLink;
using System.IO.Ports;
using System.Threading;
using SharpBladeGroundStation.CommunicationLink.BootLoader;


namespace SharpBladeGroundStation
{
	public partial class MainWindow : Window
	{
		FirmwareImage firmwareImage;

		private void button2_Click(object sender, RoutedEventArgs e)
		{
			System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();			
			ofd.Filter = "飞控固件 (*.px4)|*.px4";
			var res = ofd.ShowDialog();
			if (res != System.Windows.Forms.DialogResult.Cancel)
			{
				firmwarePathText.Text = ofd.FileName;
				firmwareImage = new FirmwareImage();
				if(firmwareImage.Load(ofd.FileName))
				{
					addUpdateMsg("载入固件成功！");
					addUpdateMsg("硬件类型：" + firmwareImage.TargetBoard.ToString());
					addUpdateMsg("Firmware size:" + firmwareImage.ImageSize);
					updateFirmwareButton.IsEnabled = true;
				}
				else
				{

				}
			}
		}

		private void updateFirmwareButton_Click(object sender, RoutedEventArgs e)
		{
			Thread updateThread = new Thread(new ThreadStart(bootloaderUpdateWorker));
			updateFirmwareButton.IsEnabled = false;
			//firmwareUpdateMsgText.Text = "";
			updateThread.IsBackground = true;
			updateThread.Start();
		}

		private void firmwareMenuItem_Click(object sender, RoutedEventArgs e)
		{

		}

		private void bootloaderUpdateWorker()
		{			
			portscanner.Stop();
			Thread.Sleep(1000);
			string[] existedPort = SerialPort.GetPortNames();
			addUpdateMsg(string.Format("Existed port {0}:", existedPort.Count()));

			byte[] buf = new byte[128];

			string newPortName = "";
			bool flag = false;
			while (!flag)
			{
				Thread.Sleep(500);
				string[] currPorts = SerialPort.GetPortNames();
				addUpdateMsg(string.Format("Current port {0}:", currPorts.Count()));
				
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
			addUpdateMsg(string.Format("Find new port:{0}", newPortName));

			SerialPort port = new SerialPort(newPortName, 115200);
			port.Open();

			//Read board info.
			uint rev, bid, flashSize;			
			addUpdateMsg(string.Format("Request BL REV"));
			rev = getBoardInfo(port, BootloaderCmd.INFO_BL_REV);			
			addUpdateMsg(string.Format("BL REV:{0}", rev));
			
			addUpdateMsg(string.Format("Request BOARD ID"));
			bid = getBoardInfo(port, BootloaderCmd.INFO_BOARD_ID);
			
			addUpdateMsg(string.Format("BOARD ID:{0}", ((BoardID)bid).ToString()));		
			
			addUpdateMsg(string.Format("Request flash size"));
			flashSize = getBoardInfo(port, BootloaderCmd.INFO_FLASH_SIZE);
			
			addUpdateMsg(string.Format("Flash size:{0}", flashSize));

			//Check compatibility.
			//TBD

			//Earse board rom.
			addUpdateMsg("Erase board...");
			sendBLCmd(port, BootloaderCmd.PROTO_CHIP_ERASE);
			while (port.BytesToRead < 2) ;
			port.Read(buf, 0, port.BytesToRead);
			if(buf[0]==(byte)BootloaderCmd.PROTO_INSYNC&&buf[1]==(byte)BootloaderCmd.PROTO_OK)
			{
				addUpdateMsg("Erase complete.");
			}

			//Flash board.
			int pos = 0;
			while(pos<firmwareImage.ImageSize)
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

					addUpdateMsg2(string.Format("Flashing:{0}/{1}",pos,firmwareImage.ImageSize));
				}
				else
				{
					addUpdateMsg("Flash error!");
					break;
				}
			}
			addUpdateMsg("Flash complete.");
			
			port.Close();
		}

		private bool sendBLCmd(SerialPort port,BootloaderCmd cmd)
		{
			byte[] buff = new byte[2];
			buff[0] = (byte)cmd;
			buff[1] = (byte)BootloaderCmd.PROTO_EOC;
			port.Write(buff, 0, 2);
			return true;
		}

		private uint getBoardInfo(SerialPort port,BootloaderCmd cmd)
		{
			uint res;
			byte[] buf = new byte[128];
			buf[0] = (byte)BootloaderCmd.PROTO_GET_DEVICE;
			buf[1] = (byte)cmd;
			buf[2] = (byte)BootloaderCmd.PROTO_EOC;
			port.Write(buf, 0, 3);
			while (port.BytesToRead < 4) ;
			port.Read(buf, 0, port.BytesToRead);
			res = BitConverter.ToUInt32(buf, 0);
			return res;
		}

		private void addUpdateMsg(string str)
		{
			Action a = () => { firmwareUpdateMsgText.Text += str + Environment.NewLine; };
			Dispatcher.BeginInvoke(a);
		}

		private void addUpdateMsg2(string str)
		{
			Action a = () => { firmwareUpdateMsgText.Text = str + Environment.NewLine; };
			Dispatcher.BeginInvoke(a);
		}
	}
}
