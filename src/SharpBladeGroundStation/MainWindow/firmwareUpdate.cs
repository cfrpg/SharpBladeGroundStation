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
		private void button2_Click(object sender, RoutedEventArgs e)
		{
			System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();			
			ofd.Filter = "飞控固件 (*.bin)|*.bin";
			var res = ofd.ShowDialog();
			if (res != System.Windows.Forms.DialogResult.Cancel)
			{
				firmwarePathText.Text = ofd.FileName;
			}
		}

		private void updateFirmwareButton_Click(object sender, RoutedEventArgs e)
		{
			Thread updateThread = new Thread(new ThreadStart(bootloaderUpdateWorker));
			updateFirmwareButton.IsEnabled = false;
			firmwareUpdateMsgText.Text = "";
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

			uint rev, bid, flashSize;			
			addUpdateMsg(string.Format("Request BL REV"));
			rev = getBoardInfo(port, BootloaderCmd.INFO_BL_REV);			
			addUpdateMsg(string.Format("BL REV:{0}", rev));
			
			addUpdateMsg(string.Format("Request BOARD ID"));
			bid = getBoardInfo(port, BootloaderCmd.INFO_BOARD_ID);
			
			addUpdateMsg(string.Format("BOARD ID:{0}", bid));
			
			
			addUpdateMsg(string.Format("Request flash size"));
			flashSize = getBoardInfo(port, BootloaderCmd.INFO_FLASH_SIZE);
			
			addUpdateMsg(string.Format("Flash size:{0}", flashSize));	

			port.Close();

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
	}
}
