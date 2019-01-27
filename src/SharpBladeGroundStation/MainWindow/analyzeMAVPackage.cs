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
			float tfloat,lat,lon,alt;
			PointLatLng pos;

			packageFlags[package.Function] = true;
			switch ((MAVLINK_MSG_ID)package.Function)
			{
				case MAVLINK_MSG_ID.HEARTBEAT://#0		
					package.NextShort();
					byte mainmode = package.NextByte();
					byte submode = package.NextByte();
					currentVehicle.Type = (MAVLink.MAV_TYPE)package.NextByte();
					currentVehicle.Autopilot=(MAVLink.MAV_AUTOPILOT)package.NextByte();
					currentVehicle.BaseMode=(MAVLink.MAV_MODE_FLAG)package.NextByte();
					currentVehicle.SystemStatus=(MAVLink.MAV_STATE)package.NextByte();
					currentVehicle.LinkVersion = package.NextByte();
					currentVehicle.FlightModeText = getPX4FlightModeText(mainmode, submode);
					heartbeatCounter = 0;
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
					GPSPositionState gpss = (GPSPositionState)package.NextByte();
					currentVehicle.GpsState.SatelliteCount = package.NextByte();
                    if (currentVehicle.GpsState.State == GPSPositionState.NoGPS && gpss != GPSPositionState.NoGPS)
					{
						currentVehicle.GpsState.SetHome();
						a3 = () => { flightRoute.Clear(); homeMarker.Position = PositionHelper.WGS84ToGCJ02(currentVehicle.GpsState.HomePosition); };
						Dispatcher.BeginInvoke(a3);
						dataSkipCount[package.Function] = 0;
					}
					currentVehicle.GpsState.State = gpss;                         	
					break;
				case MAVLINK_MSG_ID.ATTITUDE://#30
					time = package.NextUInt32();
					float rx = package.NextSingle();
					float ry = package.NextSingle();
					float rz = package.NextSingle();
					//rad
					currentVehicle.EulerAngle = new Vector3(rx, ry, rz);					
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
					
					break;
				case MAVLINK_MSG_ID.TIMESYNC://#111

					break;
				case MAVLINK_MSG_ID.ACTUATOR_CONTROL_TARGET://#140

					break;
				case MAVLINK_MSG_ID.ALTITUDE: //#141
					time64 = package.NextUInt64();
					alt = package.NextSingle();				
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
					if (currentVehicle.Battery.Remaining < 20)
						currentVehicle.SubsystemStatus.Battery = 2;
					else if (currentVehicle.Battery.Remaining < 10)
						currentVehicle.SubsystemStatus.Battery = 3;
					else if (currentVehicle.Battery.Remaining < 5)
						currentVehicle.SubsystemStatus.Battery = 4;
					else
						currentVehicle.SubsystemStatus.Battery = 0;
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
				case MAVLINK_MSG_ID.MISSION_CURRENT:
					break;

				case MAVLINK_MSG_ID.ATTITUDE_TARGET:
					break;
				case MAVLINK_MSG_ID.RADIO_STATUS:
					package.NextInt32();
					byte lrssi = package.NextByte();
					byte rrssi = package.NextByte();
					//Debug.WriteLine("[mavlink]:RSSI {0}", lrssi / 1.9 - 127);
					if (lrssi < 20)
						currentVehicle.SubsystemStatus.Telemetey = 4;
					if (lrssi < 40)
						currentVehicle.SubsystemStatus.Telemetey = 3;
					if (lrssi < 60 && heartbeatCounter < 4)
						currentVehicle.SubsystemStatus.Telemetey = 2;
					if (lrssi < 80 && heartbeatCounter < 4)
						currentVehicle.SubsystemStatus.Telemetey = 1;
					//lrssi=Math.Min(lrssi, rrssi);
					currentVehicle.TelemetryRSSI = lrssi / 1.9f - 127;
					currentVehicle.TelemetryPercentage = lrssi / 255f;
					if (currentVehicle.TelemetryPercentage > 100)
						currentVehicle.TelemetryPercentage = 100;
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
					missionManager.HandleMissionRequest(s2);
					break;
				case MAVLINK_MSG_ID.MISSION_ACK:
					package.NextShort();
					missionManager.HandleMissionAck();
					Debug.WriteLine("[MAVLink]:Finished.");
					break;				
				case MAVLINK_MSG_ID.MISSION_COUNT:
					missionManager.WaypointCount = package.NextUShort();
					Debug.WriteLine("[MAVLink]:Receive {0} waypoint.", missionManager.WaypointCount);
					missionManager.SendMissionRequest();
					break;
				case MAVLINK_MSG_ID.MISSION_ITEM:
					if(missionManager.AddMissionItem(package))
					{
						a1 = () => { missionManager.UnpackMission(); };
						Dispatcher.BeginInvoke(a1);
					}
					
					break;
				case MAVLINK_MSG_ID.STATUSTEXT:
					byte sev = package.NextByte();
					string str = package.NextASCIIString(50);
					a1 = () => { currentVehicle.HandleMessage(sev,str);if(sev<=4) mainSpeech.SpeakAsync(str); };
					Dispatcher.BeginInvoke(a1);
					Debug.WriteLine("[MAVLink]:{0}.", str);
					break;
				default:
					Debug.WriteLine("[MAVLink]:Unhandled package {0}.", package.Function);
					break;
			}
		}

		private string getPX4FlightModeText(byte mainmode, byte submode)
		{
			string res = "";
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
				default:
					res = "NK";
					break;
			}
			return res;
		}
	}
}
