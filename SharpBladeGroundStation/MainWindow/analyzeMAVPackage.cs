using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using GMap.NET;
using SharpBladeGroundStation.CommLink;
using SharpBladeGroundStation.DataStructs;
using SharpBladeGroundStation.Map;
using System.MAVLink;

namespace SharpBladeGroundStation
{
    public partial class MainWindow : Window
    {
        private void analyzeMAVPackage(LinkPackage p)
        {
            MAVLinkPackage package = (MAVLinkPackage)p;
            package.StartRead();
            UInt32 time = 0;
            UInt64 time64 = 0;
            UInt64 dt = (ulong)GCSconfig.PlotTimeInterval * 1000;
            switch ((MAVLINK_MSG_ID)package.Function)
            {
                case MAVLINK_MSG_ID.SYS_STATUS:     //SYS_STATUS

                    break;

                case MAVLINK_MSG_ID.GPS_RAW_INT:    //GPS_RAW_INT 
                    time64 = package.NextUInt64();
                    gpsData.Latitude = package.NextInt32() * 1.0 / 1e7;
                    gpsData.Longitude = package.NextInt32() * 1.0 / 1e7;
                    int talt = package.NextInt32();
                    gpsData.Hdop = package.NextUShort();
                    if (gpsData.Hdop > 10000)
                        gpsData.Hdop = -1;
                    gpsData.Vdop = package.NextUShort();
                    if (gpsData.Vdop > 10000)
                        gpsData.Vdop = -1;
                    gpsData.Vdop /= 100f;
                    gpsData.Hdop /= 100f;
                    flightState.GroundSpeed = package.NextUShort() / 100.0f;

                    gpsData.SatelliteCount = package.NextUShort();
                    GPSPositionState gpss = (GPSPositionState)package.NextByte();//sb文档害我debug一天!



                    if (gpsData.State == GPSPositionState.NoGPS && gpss != GPSPositionState.NoGPS)
                    {
                        flightRoutePoints.Clear();
                        dataSkipCount[package.Function] = 0;
                    }
                    gpsData.State = gpss;
                    PointLatLng pos = PositionHelper.WGS84ToGCJ02(new PointLatLng(gpsData.Latitude, gpsData.Longitude));

                    if (time64 - dataSkipCount[package.Function] > (ulong)GCSconfig.CourseTimeInterval * 1000)
                    {
                        Action a241 = () => { updateFlightRoute(pos); };
                        Dispatcher.BeginInvoke(a241);
                        dataSkipCount[package.Function] = time64;

                    }
                    gpsData.SatelliteCount = package.NextByte();
                    Action a24 = () => { uavMarker.Position = pos; };
                    Dispatcher.BeginInvoke(a24);
                    break;
                case MAVLINK_MSG_ID.ATTITUDE:
                    time = package.NextUInt32();
                    flightState.Roll = -rad2deg(package.NextSingle());
                    flightState.Pitch = rad2deg(package.NextSingle());
                    flightState.Yaw = rad2deg(package.NextSingle());
                    float h = flightState.Yaw;
                    flightState.Heading = h < 0 ? 360 + h : h;
                    Action a30 = () => { uavMarker.Shape.RenderTransform = new RotateTransform(flightState.Heading, 15, 15); };
                    Dispatcher.BeginInvoke(a30);

                    if ((ulong)time * 1000 - dataSkipCount[package.Function] > dt)
                    {
                        attitudeGraphData[0].AppendAsync(this.Dispatcher, new Point(time / 1000.0, flightState.Roll));
                        attitudeGraphData[1].AppendAsync(this.Dispatcher, new Point(time / 1000.0, flightState.Pitch));
                        attitudeGraphData[2].AppendAsync(this.Dispatcher, new Point(time / 1000.0, flightState.Yaw));

                        dataSkipCount[package.Function] = (ulong)time * 1000;
                    }
                    break;

                case MAVLINK_MSG_ID.LOCAL_POSITION_NED:    //LOCAL_POSITION_NED
                    time = package.NextUInt32();
                    float vx = package.NextSingle();
                    float vy = package.NextSingle();
                    float vz = package.NextSingle();
                    vx = package.NextSingle();
                    vy = package.NextSingle();
                    vz = package.NextSingle();
                    flightState.ClimbRate = -vz;

                    break;
                case MAVLINK_MSG_ID.ALTITUDE:   //ALTITUDE 
                    time64 = package.NextUInt64();
                    flightState.Altitude = package.NextSingle();
                    if (time64 - dataSkipCount[package.Function] > dt)
                    {
                        altitudeGraphData.AppendAsync(this.Dispatcher, new Point(time64 / 1000000.0, flightState.Altitude));
                        dataSkipCount[package.Function] = time64;
                    }
                    break;
                case MAVLINK_MSG_ID.HIGHRES_IMU:   //HIGHRES_IMU
                    time64 = package.NextUInt64();
                    float[] sd = { 0, 0, 0 };
                    sd[0] = package.NextSingle();
                    sd[1] = package.NextSingle();
                    sd[2] = package.NextSingle();
                    //setSensorData("ACCEL", sd[0], sd[1], sd[2], false);
                    setVector3Data("ACCEL", sd[0], sd[1], sd[2], sensorData);
                    if (time64 - dataSkipCount[package.Function] > dt)
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            accelGraphData[i].AppendAsync(this.Dispatcher, new Point(time64 / 1000000.0, sd[i]));
                        }
                    }

                    sd[0] = package.NextSingle();
                    sd[1] = package.NextSingle();
                    sd[2] = package.NextSingle();
                    //setSensorData("GYRO", sd[0], sd[1], sd[2], false);
                    setVector3Data("GYRO", sd[0], sd[1], sd[2], sensorData);
                    if (time64 - dataSkipCount[package.Function] > dt)
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            gyroGraphData[i].AppendAsync(this.Dispatcher, new Point(time64 / 1000000.0, sd[i]));
                        }
                        dataSkipCount[package.Function] = time64;
                    }

                    sd[0] = package.NextSingle();
                    sd[1] = package.NextSingle();
                    sd[2] = package.NextSingle();
                    //setSensorData("MAG", sd[0], sd[1], sd[2], true);
                    setVector3Data("MAG", sd[0], sd[1], sd[2], sensorData);
                    break;
                default:

                    break;
            }

        }
    }
}
