using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GMap.NET;
using System.ComponentModel;
using SharpBladeGroundStation.CommunicationLink;
using Microsoft.Xna.Framework;
using FlightDisplay;
using System.MAVLink;

using Matrix = Microsoft.Xna.Framework.Matrix;

namespace SharpBladeGroundStation.DataStructs
{
	/// <summary>
	/// 描述飞行器的类
	/// </summary>
	public class Vehicle : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		int id;

		Vector3 position;
		Vector3 velocity;
		Vector3 angleVelocity;
		Vector3 eulerAngle;
		GPSData gpsState;
		float heading;
		float groundSpeed;
		float airSpeed;
		float climbRate;
		float altitude;
		float relativeAltitude;

		MAVLink.MAV_TYPE type;
		MAVLink.MAV_AUTOPILOT autopilot;
		MAVLink.MAV_MODE_FLAG baseMode;
		MAVLink.MAV_STATE systemStatus;

		float distanceSensor;

		string flightModeText;
		bool isArmed;

		FlightState flightState;
		Camera camera;
		BatteryData battery;

		CommLink link;

		VehicleSystemStatus subsystemStatus;

		byte linkVersion;

		/// <summary>
		/// 位置(pn,pe,pd)
		/// </summary>
		public Vector3 Position
		{
			get { return position; }
			set
			{
				position = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Position"));
				//PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("FlightState"));
			}
		}
		/// <summary>
		/// 速度,(u,v,w)
		/// </summary>
		public Vector3 Velocity
		{
			get { return velocity; }
			set
			{
				velocity = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Velocity"));
				//PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("FlightState"));
			}
		}
		/// <summary>
		/// 角速度,(p,q,r)
		/// </summary>
		public Vector3 AngleVelocity
		{
			get { return angleVelocity; }
			set
			{
				angleVelocity = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("AngleVelocity"));
				//PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("FlightState"));
			}
		}
		/// <summary>
		/// 欧拉角,(phi,theta,psi)
		/// </summary>
		public Vector3 EulerAngle
		{
			get { return eulerAngle; }
			set
			{
				eulerAngle = value;
				flightState.Roll = MathHelper.ToDegrees(eulerAngle.X);
				flightState.Pitch = MathHelper.ToDegrees(eulerAngle.Y);
				flightState.Yaw = MathHelper.ToDegrees(eulerAngle.Z);
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("EulerAngle"));
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("FlightState"));
			}
		}
		/// <summary>
		/// GPS信息
		/// </summary>
		public GPSData GpsState
		{
			get { return gpsState; }
			set
			{
				gpsState = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("GpsState"));
				//PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("FlightState"));
			}
		}
		/// <summary>
		/// 航向,deg
		/// </summary>
		public float Heading
		{
			get { return heading; }
			set
			{
				heading = value;
				while (heading < 0)
					heading += 360;
				while (heading > 360)
					heading -= 360;
				flightState.Heading = heading;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Heading"));
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("FlightState"));
			}
		}
		/// <summary>
		/// 地速
		/// </summary>
		public float GroundSpeed
		{
			get { return groundSpeed; }
			set
			{
				groundSpeed = value;
				flightState.GroundSpeed = groundSpeed;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("GroundSpeed"));
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("FlightState"));
			}
		}
		/// <summary>
		/// 空速
		/// </summary>
		public float AirSpeed
		{
			get { return airSpeed; }
			set
			{
				airSpeed = value;
				flightState.AirSpeed = airSpeed;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("AirSpeed"));
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("FlightState"));
			}
		}
		/// <summary>
		/// 爬升率
		/// </summary>
		public float ClimbRate
		{
			get { return climbRate; }
			set
			{
				climbRate = value;
				flightState.ClimbRate = climbRate;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ClimbRate"));
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("FlightState"));
			}
		}
		/// <summary>
		/// 海拔高度
		/// </summary>
		public float Altitude
		{
			get { return altitude; }
			set
			{
				altitude = value;
				flightState.Altitude = altitude;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Altitude"));
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("FlightState"));
			}
		}
		/// <summary>
		/// 飞行模式文字信息
		/// </summary>
		public string FlightModeText
		{
			get { return flightModeText; }
			set
			{
				flightModeText = value;
				flightState.FlightModeText = flightModeText;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("FlightModeText"));
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("FlightState"));
			}
		}
		/// <summary>
		/// 是否解锁
		/// </summary>
		public bool IsArmed
		{
			get { return isArmed; }
			set
			{
				isArmed = value;
				flightState.IsArmed = isArmed;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsArmed"));
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("FlightState"));
			}
		}
		/// <summary>
		/// 适配器
		/// </summary>
		public FlightState FlightState
		{
			get { return flightState; }
		}
		/// <summary>
		/// 唯一标识
		/// </summary>
		public int ID
		{
			get { return id; }
			set { id = value; }
		}
		/// <summary>
		/// 使用的连接
		/// </summary>
		public CommLink Link
		{
			get { return link; }
			set { link = value; }
		}
		/// <summary>
		/// 摄像机
		/// </summary>
		public Camera Camera
		{
			get { return camera; }
			set { camera = value; }
		}

		/// <summary>
		/// 电池状态
		/// </summary>
		public BatteryData Battery
		{
			get { return battery; }
			set
			{
				battery = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Battery"));				
			}
		}

		/// <summary>
		/// 相对高度
		/// </summary>
		public float RelativeAltitude
		{
			get { return relativeAltitude; }
			set
			{
				relativeAltitude = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("RelativeAltitude"));
			}
		}

		/// <summary>
		/// 测距仪
		/// </summary>
		public float DistanceSensor
		{
			get { return distanceSensor; }
			set
			{
				distanceSensor = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("DistanceSensor"));
			}
		}

		public MAVLink.MAV_TYPE Type
		{
			get { return type; }
			set
			{
				type = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Type"));
			}
		}

		public MAVLink.MAV_AUTOPILOT Autopilot
		{
			get { return autopilot; }
			set
			{
				autopilot = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Autopilot"));
			}
		}

		public MAVLink.MAV_MODE_FLAG BaseMode
		{
			get { return baseMode; }
			set
			{
				baseMode = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("BaseMode"));
				IsArmed = (baseMode & MAVLink.MAV_MODE_FLAG.SAFETY_ARMED) != 0;
			}
		}

		public MAVLink.MAV_STATE SystemStatus
		{
			get { return systemStatus; }
			set
			{
				systemStatus = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SystemStatus"));
			}
		}

		public VehicleSystemStatus SubsystemStatus
		{
			get { return subsystemStatus; }
			set
			{
				subsystemStatus = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SubsystemStatus"));
			}
		}

		public byte LinkVersion
		{
			get
			{
				return linkVersion;
			}

			set
			{
				linkVersion = value;
			}
		}

		/// <summary>
		/// 构造函数
		/// </summary>
		/// <param name="i">ID</param>
		/// <param name="l">SerialLink</param>
		public Vehicle(int i)
		{
			id = i;
			position = Vector3.Zero;
			velocity = Vector3.Zero;
			angleVelocity = Vector3.Zero;
			eulerAngle = Vector3.Zero;
			gpsState = new GPSData();
			heading = 0;
			groundSpeed = 0;
			airSpeed = 0;
			climbRate = 0;
			altitude = 0;
			flightModeText = "";
			isArmed = false;
			subsystemStatus = new VehicleSystemStatus();

			flightState = FlightState.Zero;
			battery = new BatteryData();
			camera = new Camera();
			camera.CameraTransfrom = new Matrix(
				new Vector4(496.9788f, 0f, 0f, 0f),
				new Vector4(0f, 526.7737f, 0f, 0f),
				new Vector4(0f, 0f, 1f, 0f),
				new Vector4(360f, 288f, 0f, 1f));
			camera.CameraTransfrom = Matrix.CreateRotationX(MathHelper.ToRadians(-11f)) * camera.CameraTransfrom;
			camera.K1 = -0.400138193501512f;
			camera.K2 = 0.180327815997683f;
			camera.P1 = 0.002723499733555f;
			camera.P2 = 0.003389955079601f;
			camera.ScreenSize = new Vector2(720, 576);
		}

		public void HandleMessage(int lv,string str)
		{
			if (lv <= 2)
				lv = 4;
			else if (lv <= 3)
				lv = 3;
			else if (lv == 4)
				lv = 2;
			else
				lv = 1;
			string upper = str.ToUpper();
			if(upper.Contains("ACCEL"))
			{
				subsystemStatus.Accelerometer = lv;
			}
			if (upper.Contains("GYRO"))
			{
				subsystemStatus.Gyroscope = lv;
			}
			if (upper.Contains("MAG"))
			{
				subsystemStatus.Compass = lv;
			}
			if (upper.Contains("EKF"))
			{
				subsystemStatus.Ekf = lv;
			}
			if (upper.Contains("GPS"))
			{
				subsystemStatus.Gps = lv;
			}
		}
	}
}
