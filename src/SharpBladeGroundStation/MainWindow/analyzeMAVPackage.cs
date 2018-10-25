using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using GMap.NET;
using SharpBladeGroundStation.CommunicationLink;
using SharpBladeGroundStation.DataStructs;
using SharpBladeGroundStation.Map;
using System.MAVLink;
using Microsoft.Xna.Framework;
using System.Diagnostics;
using Point = System.Windows.Point;

namespace SharpBladeGroundStation
{
	public partial class MainWindow : Window
	{
		private void analyzeMAVPackage(LinkPackage p)
		{
			MAVLinkPackage package = (MAVLinkPackage)p;
			package.StartRead();
			currentVehicle.ID = package.System;
			uint time = 0;
			ulong time64 = 0;
			ulong dt = (ulong)GCSconfig.PlotTimeInterval * 1000;
			Action a1, a2,a3;
			int tint;
			float tfloat;
			PointLatLng pos;

			packageFlags[package.Function] = true;
			switch ((MAVLINK_MSG_ID)package.Function)
			{
				case MAVLINK_MSG_ID.HEARTBEAT://#0		
					package.NextShort();
					byte mainmode = package.NextByte();
					byte submode = package.NextByte();
					currentVehicle.FlightModeText = getPX4FlightModeText(mainmode, submode);
					break;
				case MAVLINK_MSG_ID.SYS_STATUS://#1

					break;
				case MAVLINK_MSG_ID.SYSTEM_TIME://#2

					break;
				case MAVLINK_MSG_ID.GPS_RAW_INT: //#24                    
					time64 = package.NextUInt64();
					if (!packageFlags[33])
					{
						currentVehicle.GpsState.Latitude = package.NextInt32() * 1.0 / 1e7;
						currentVehicle.GpsState.Longitude = package.NextInt32() * 1.0 / 1e7;
						pos = PositionHelper.WGS84ToGCJ02(new PointLatLng(currentVehicle.GpsState.Latitude, currentVehicle.GpsState.Longitude));

						if (time64 - dataSkipCount[package.Function] > (ulong)GCSconfig.CourseTimeInterval * 1000)
						{
							a1 = () => { updateFlightRoute(pos); };
							Dispatcher.BeginInvoke(a1);
							dataSkipCount[package.Function] = time64;
						}
						a2 = () => { uavMarker.Position = pos; };
						Dispatcher.BeginInvoke(a2);
					}
					else
					{
						tint = package.NextInt32();
						tint = package.NextInt32();						
					}
					int talt = package.NextInt32();
					currentVehicle.GpsState.Hdop = package.NextUShort();
					if (currentVehicle.GpsState.Hdop > 10000)
						currentVehicle.GpsState.Hdop = -1;
					currentVehicle.GpsState.Vdop = package.NextUShort();
					if (currentVehicle.GpsState.Vdop > 10000)
						currentVehicle.GpsState.Vdop = -1;
					currentVehicle.GpsState.Vdop /= 100f;
					currentVehicle.GpsState.Hdop /= 100f;
					currentVehicle.GroundSpeed = package.NextUShort() / 100.0f;
					package.NextUShort();//cog					
					GPSPositionState gpss = (GPSPositionState)package.NextByte();//sb文档害我debug一天!
					currentVehicle.GpsState.SatelliteCount = package.NextByte();
                    if (currentVehicle.GpsState.State == GPSPositionState.NoGPS && gpss != GPSPositionState.NoGPS)
					{
						currentVehicle.GpsState.SetHome();
						a3 = () => { flightRoute.Clear(); homeMarker.Position = PositionHelper.WGS84ToGCJ02(currentVehicle.GpsState.HomePosition); };
						Dispatcher.BeginInvoke(a3);
						dataSkipCount[package.Function] = 0;
					}
					currentVehicle.GpsState.State = gpss;
                    if(package.Version==2)
                    {
                        int altell = package.NextInt32();
                        Debug.WriteLine(altell);
                    }                   	
					break;
				case MAVLINK_MSG_ID.ATTITUDE://#30
					time = package.NextUInt32();
					float rx = package.NextSingle();
					float ry = package.NextSingle();
					float rz = package.NextSingle();
					//rad
					currentVehicle.EulerAngle = new Vector3(rx, ry, rz);
					if ((ulong)time * 1000 - dataSkipCount[package.Function] > dt)
					{
						//attitudeGraphData[0].AppendAsync(this.Dispatcher, new Point(time / 1000.0, MathHelper.ToDegrees(rx)));
						//attitudeGraphData[1].AppendAsync(this.Dispatcher, new Point(time / 1000.0, MathHelper.ToDegrees(ry)));
						//attitudeGraphData[2].AppendAsync(this.Dispatcher, new Point(time / 1000.0, MathHelper.ToDegrees(rz)));
						dataSkipCount[package.Function] = (ulong)time * 1000;
					}
					break;
				case MAVLINK_MSG_ID.ATTITUDE_QUATERNION://#31

					break;
				case MAVLINK_MSG_ID.LOCAL_POSITION_NED: //#32                    
                    time = package.NextUInt32();
					float vx = package.NextSingle();
					float vy = package.NextSingle();
					float vz = package.NextSingle();
					currentVehicle.Position = new Vector3(vx, vy, vz);
					vx = package.NextSingle();
					vy = package.NextSingle();
					vz = package.NextSingle();
					currentVehicle.Velocity = new Vector3(vx, vy, vz);
                    if((!packageFlags[74])&&(!packageFlags[33]))
                    {
                        currentVehicle.ClimbRate = -vz;
                    }					
					break;
				case MAVLINK_MSG_ID.GLOBAL_POSITION_INT://#33                   
                    time = package.NextUInt32();
					time64 = (ulong)time * 1000;
					currentVehicle.GpsState.Latitude = package.NextInt32() * 1.0 / 1e7;
					currentVehicle.GpsState.Longitude = package.NextInt32() * 1.0 / 1e7;
					tint = package.NextInt32();
					currentVehicle.RelativeAltitude = package.NextInt32() / 1000f;

					package.NextShort();//vx
					package.NextShort();//vy
					currentVehicle.ClimbRate= -package.NextShort()/100.0f;

					pos = PositionHelper.WGS84ToGCJ02(new PointLatLng(currentVehicle.GpsState.Latitude, currentVehicle.GpsState.Longitude));

					if (time64 - dataSkipCount[package.Function] > (ulong)GCSconfig.CourseTimeInterval * 1000)
					{
						a1 = () => { updateFlightRoute(pos); };
						Dispatcher.BeginInvoke(a1);
						dataSkipCount[package.Function] = time64;						
					}
					
					a2 = () => {
						uavMarker.Position = pos;
						if (mapCenterConfig == Configuration.MapCenterPositionConfig.FollowUAV)
							gmap.Position = pos;
					};
					Dispatcher.BeginInvoke(a2);
					break;
				case MAVLINK_MSG_ID.SERVO_OUTPUT_RAW://#36

					break;
				case MAVLINK_MSG_ID.VFR_HUD://#74
					currentVehicle.AirSpeed = package.NextSingle();
					currentVehicle.GroundSpeed = package.NextSingle();
					currentVehicle.Altitude = package.NextSingle();
					if(!packageFlags[33])
					{
						currentVehicle.ClimbRate = package.NextSingle();
					}
					else
					{
						package.NextSingle();
					}					
					currentVehicle.Heading = package.NextShort();
					a1 = () => { uavMarker.Shape.RenderTransform = new RotateTransform(currentVehicle.Heading, 15, 15); };
					Dispatcher.BeginInvoke(a1);
					ushort thro = package.NextUShort();//unused
					break;
				case MAVLINK_MSG_ID.HIGHRES_IMU://#105
					//time64 = package.NextUInt64();
					//float[] sd = { 0, 0, 0 };
					//sd[0] = package.NextSingle();
					//sd[1] = package.NextSingle();
					//sd[2] = package.NextSingle();

					//if (time64 - dataSkipCount[package.Function] > dt)
					//{
					//	for (int i = 0; i < 3; i++)
					//	{
					//		accelGraphData[i].AppendAsync(this.Dispatcher, new Point(time64 / 1000000.0, sd[i]));
					//	}
					//}

					//sd[0] = package.NextSingle();
					//sd[1] = package.NextSingle();
					//sd[2] = package.NextSingle();
					//if (time64 - dataSkipCount[package.Function] > dt)
					//{
					//	for (int i = 0; i < 3; i++)
					//	{
					//		gyroGraphData[i].AppendAsync(this.Dispatcher, new Point(time64 / 1000000.0, sd[i]));
					//	}
					//	dataSkipCount[package.Function] = time64;
					//}

					//sd[0] = package.NextSingle();
					//sd[1] = package.NextSingle();
					//sd[2] = package.NextSingle();
					break;
				case MAVLINK_MSG_ID.TIMESYNC://#111

					break;
				case MAVLINK_MSG_ID.ACTUATOR_CONTROL_TARGET://#140

					break;
				case MAVLINK_MSG_ID.ALTITUDE: //#141
					time64 = package.NextUInt64();
					float alt = package.NextSingle();

					if (time64 - dataSkipCount[package.Function] > dt)
					{
						//altitudeGraphData.AppendAsync(this.Dispatcher, new Point(time64 / 1000000.0, alt));
						dataSkipCount[package.Function] = time64;
					}
					break;
				case MAVLINK_MSG_ID.BATTERY_STATUS://#147												   
					tint = package.NextInt32();
					if (tint > 0)
					{
						currentVehicle.Battery.CurrentConsumed = tint;
					}
					else
					{
						currentVehicle.Battery.CurrentConsumed = -1;
					}
					tint = package.NextInt32();
					if (tint > 0)
					{
						currentVehicle.Battery.EnergyConsumed = (float)tint / 10f;
					}
					else
					{
						currentVehicle.Battery.EnergyConsumed = -1;
					}
					currentVehicle.Battery.Temperature = package.NextShort();
					float battv = 0;
					int cellv = 0;
					for (int i = 0; i < 10; i++)
					{
						cellv = package.NextUShort();
						if (cellv > 30000)
						{
							cellv = -1;
							currentVehicle.Battery.CellVoltage[i] = -1;
						}
						else
						{
							currentVehicle.Battery.CellVoltage[i] = ((float)cellv) / 1000f;
							battv += currentVehicle.Battery.CellVoltage[i];
						}
					}
					currentVehicle.Battery.Voltage = battv;
					tint = package.NextShort();
					if (tint >= 0)
					{
						currentVehicle.Battery.Current = (float)tint / 100f;
					}
					else
					{
						currentVehicle.Battery.Current = -1;
					}
					currentVehicle.Battery.ID = package.NextByte();
					currentVehicle.Battery.Function = package.NextByte();
					currentVehicle.Battery.Type = package.NextByte();
					currentVehicle.Battery.Remaining = package.NextSByte();
					break;
				case MAVLINK_MSG_ID.ESTIMATOR_STATUS://#230

					break;
				case MAVLINK_MSG_ID.WIND_COV://#231

					break;
				case MAVLINK_MSG_ID.VIBRATION://#241

					break;
				case MAVLINK_MSG_ID.EXTENDED_SYS_STATE://#245

					break;
				case MAVLINK_MSG_ID.COMMAND_ACK://#77

					break;
				case MAVLINK_MSG_ID.DISTANCE_SENSOR://#132
					time = package.NextUInt32();
					package.NextUShort();//min
					package.NextUShort();//max
					tfloat = package.NextUShort() / 100f;//distance
					if(Math.Abs(tfloat-currentVehicle.DistanceSensor)>0.2f)
					{
						currentVehicle.DistanceSensor = tfloat;
					}
					else
					{
						currentVehicle.DistanceSensor = currentVehicle.DistanceSensor * 0.8f + tfloat * 0.2f;
					}
					break;
				case MAVLINK_MSG_ID.MISSION_REQUEST:
					ushort s2 = package.NextUShort();
					short s1 = package.NextShort();
					Debug.WriteLine("[MAVLink]:Request mission item {0} {1}.", s2, s1);
					missionSender.NextRequest = s2;
					break;
				case MAVLINK_MSG_ID.MISSION_ACK:
					package.NextShort();
					missionSender.NextRequest = 32769;
					Debug.WriteLine("[MAVLink]:Finished.");
					break;
				case MAVLINK_MSG_ID.MISSION_CURRENT:

					break;
				case MAVLINK_MSG_ID.ATTITUDE_TARGET:

					break;
				case MAVLINK_MSG_ID.MISSION_COUNT:
					Debug.WriteLine("[MAVLink]:Count {0}.", package.NextUShort());
					MAVLinkPackage pkg = new MAVLinkPackage((byte)MAVLINK_MSG_ID.MISSION_REQUEST,currentVehicle.Link);
					pkg.Sequence = 0;
					pkg.System = 255;
					pkg.Component = 190;					
					pkg.AddData((ushort)0);
					pkg.AddData(package.System);
					pkg.AddData((byte)190);
					pkg.SetVerify();
					currentVehicle.Link.SendPackage(pkg);
					break;
				default:
					Debug.WriteLine("[MAVLink]:Unhandled package {0}.", package.Function);
					break;
			}
		}

		private string getPX4FlightModeText(byte mainmode, byte submode)
		{
			string res = "NK";
			switch (mainmode)
			{
				case 1:
					res = "MAN";
					break;
				case 2:
					res = "ALTC";
					break;
				case 3:
					res = "POSC";
					break;
				case 4:
					switch (submode)
					{
						case 1:
							res += "RDY";
							break;
						case 2:
							res += "TKOF";
							break;
						case 3:
							res += "LOIT";
							break;
						case 4:
							res += "MISS";
							break;
						case 5:
							res += "RTL";
							break;
						case 6:
							res += "LAND";
							break;
						case 7:
							res += "RTGS";
							break;
						case 8:
							res += "FOLW";
							break;
					}
					break;
				case 5:
					res = "ACRO";
					break;
				case 6:
					res = "OFBD";
					break;
				case 7:
					res = "STAB";
					break;
				case 8:
					res = "RATT";
					break;
				case 9:
					res = "SIMP";
					break;
			}
			return res;
		}
	}
}
