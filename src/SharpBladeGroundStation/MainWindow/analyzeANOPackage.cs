using System;
using System.Windows;
using FlightDisplay;
using SharpBladeGroundStation.CommunicationLink;
using SharpBladeGroundStation.DataStructs;
using Microsoft.Xna.Framework;
using Point = System.Windows.Point;



namespace SharpBladeGroundStation
{
    public partial class MainWindow : Window
    {
        private void analyzeANOPackage(LinkPackage p)
        {
            ANOLinkPackage package = (ANOLinkPackage)p;
            package.StartRead();
            switch (package.Function)
            {
                case 0x00://VER

                    break;
                case 0x01://STATUS
                    float rx = package.NextShort() / 100f;
                    float ry = package.NextShort() / 100f;
                    float rz = package.NextShort() / 100f;
                    currentVehicle.EulerAngle = new Vector3(MathHelper.ToRadians(rx), MathHelper.ToRadians(ry), MathHelper.ToRadians(rz));
                    currentVehicle.Altitude = package.NextInt32() / 100f;
                    currentVehicle.FlightModeText = getANOFlightModeText(package.NextByte());
                    currentVehicle.IsArmed = package.NextByte() == 1;
                    attitudeGraphData[0].AppendAsync(this.Dispatcher, new Point(package.TimeStamp / 1000, rx));
                    attitudeGraphData[1].AppendAsync(this.Dispatcher, new Point(package.TimeStamp / 1000, ry));
                    attitudeGraphData[2].AppendAsync(this.Dispatcher, new Point(package.TimeStamp / 1000, rz));
                    altitudeGraphData.AppendAsync(this.Dispatcher, new Point(package.TimeStamp / 1000, currentVehicle.Altitude / 100.0));
                    break;
                case 0x02://SENSER
                    short[] sd = { 0, 0, 0 };
                    sd[0] = package.NextShort();
                    sd[1] = package.NextShort();
                    sd[2] = package.NextShort();                   
                    for (int i = 0; i < 3; i++)
                    {
                        accelGraphData[i].AppendAsync(this.Dispatcher, new Point(package.TimeStamp / 1000, sd[i]));
                    }

                    sd[0] = package.NextShort();
                    sd[1] = package.NextShort();
                    sd[2] = package.NextShort();                    
                    for (int i = 0; i < 3; i++)
                    {
                        gyroGraphData[i].AppendAsync(this.Dispatcher, new Point(package.TimeStamp / 1000, sd[i]));
                    }

                    sd[0] = package.NextShort();
                    sd[1] = package.NextShort();
                    sd[2] = package.NextShort();                  
                    break;
                case 0x03://RCDATA
                    for (int i = 0; i < 10; i++)
                    {
                        short rc = package.NextShort();                        
                    }
                    break;
                case 0x04://GPSDATA
                    gpsData.State = (GPSPositionState)package.NextByte();
                    gpsData.SatelliteCount = package.NextByte();
                    gpsData.Longitude = package.NextInt32() / 10000000.0f;
                    gpsData.Latitude = package.NextInt32() / 10000000.0f;
                    gpsData.HomingAngle = package.NextShort() / 10.0f;

                    break;
                case 0x05://POWER
                    ushort v = package.NextUShort();
                    ushort c = package.NextUShort();

                    Action a05 = () => { battText.Text = string.Format("{0:F2}V {1:F2}A", (double)v / 100.0, (double)c / 100.0); };
                    battText.Dispatcher.Invoke(a05);
                    break;
                case 0x06://MOTO
                    for (int i = 1; i <= 8; i++)
                    {
                        short pwm = package.NextShort();
                        
                    }
                    break;
                case 0x07://SENSER2
                    int altbar = package.NextInt32(); 
                    altbar = package.NextUShort();
                    
                    break;
                case 0x0A://FLY MODEL

                    break;
                case 0x0B://
                    short sr = package.NextShort();
                    short sp = package.NextShort();
                    currentVehicle.ClimbRate = package.NextShort() / 100.0f;
                    
                    break;
                case 0x20://FP_NUMBER

                    break;
                case 0x21://FP

                    break;
                case 0xEF://CHECK

                    break;
                default:
                    if (package.Function >= 0x10 && package.Function <= 0x15)
                    {
                        int id = (package.Function - 0x10) * 3;
                        for (int i = 0; i < 3; i++)
                        {
                            short P = package.NextShort();
                            short I = package.NextShort();
                            short D = package.NextShort();     
                        }
                    }
                    break;
            }
        }
		private string getANOFlightModeText(int id)
		{
			switch (id)
			{
				case 0:
					return "未知";
				case 1:
					return "姿态";
				case 2:
					return "定高";
				case 3:
					return "定点";
				case 11:
					return "航线";
				case 20:
					return "降落";
				case 21:
					return "返航";
				default:
					return "未知";
			}
		}
	}
}
