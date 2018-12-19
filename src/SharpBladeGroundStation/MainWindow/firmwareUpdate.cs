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
		FirmwareUpdater firmwareUpdater;

		private void initFirmwareUpdater()
		{
			firmwareUpdater = new FirmwareUpdater();
			firmwareUpdateProgbar.DataContext = firmwareUpdater;
			firmwareUpdateMsgText.DataContext = firmwareUpdater;
			firmwareUpdater.UpdateFinished += FirmwareUpdater_UpdateFinished;
		}

		private void FirmwareUpdater_UpdateFinished(FirmwareUpdaterErrorMsg err)
		{
			if (err == FirmwareUpdaterErrorMsg.OK)
			{
				if (!isReplay)
				{
					portscanner.Start();
				}
			}
		}

		private void button2_Click(object sender, RoutedEventArgs e)
		{
			System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();
			ofd.Filter = "飞控固件 (*.px4)|*.px4";
			var res = ofd.ShowDialog();
			if (res != System.Windows.Forms.DialogResult.Cancel)
			{
				firmwareUpdater.Message = "";
				firmwarePathText.Text = ofd.FileName;
				firmwareUpdater.FirmwareImage = new FirmwareImage();
				if (firmwareUpdater.FirmwareImage.Load(ofd.FileName))
				{
					firmwareUpdater.AddUpdateMessage("载入固件成功！");
					firmwareUpdater.AddUpdateMessage("硬件类型：{0}", firmwareUpdater.FirmwareImage.TargetBoard);
					firmwareUpdater.AddUpdateMessage("固件大小：{0}", firmwareUpdater.FirmwareImage.ImageSize);

					updateFirmwareButton.IsEnabled = true;
				}
				else
				{
					firmwareUpdater.AddUpdateMessage("载入固件失败！");
				}
			}
		}

		private void updateFirmwareButton_Click(object sender, RoutedEventArgs e)
		{
			updateFirmwareButton.IsEnabled = false;
			portscanner.Stop();
			Thread.Sleep(1000);
			firmwareUpdater.Start();
		}
	}
}
