﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using SharpBladeGroundStation.CommLink;
using Microsoft.Xna.Framework;
using FlightDisplay;
using Matrix = Microsoft.Xna.Framework.Matrix;

namespace SharpBladeGroundStation.DataStructs
{
    /// <summary>
    /// 描述飞行器的类
    /// </summary>
	public class Vehicle:INotifyPropertyChanged
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

        string flightModeText;
        bool isArmed;

        FlightState flightState;
        Camera camera;
        BatteryData battery;

        SerialLink link;

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
                flightState.Roll =MathHelper.ToDegrees(eulerAngle.X);
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
        /// 使用的串口连接
        /// </summary>
        public SerialLink Link
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

            flightState = FlightState.Zero;
            battery = new BatteryData();
            camera = new Camera();
            camera.CameraTransfrom = new Matrix(
                new Vector4(496.9788f, -0.4179f, 356.3610f, 0f),
                new Vector4(0f, 526.7737f, 288.5539f, 0f),
                new Vector4(0f, 0f, 1f, 0f),
                new Vector4(0f,0f,0f,1f));
            camera.K1 = -0.400138193501512f;
            camera.K2 = 0.180327815997683f;
            camera.P1 = 0.002723499733555f;
            camera.P2 = 0.003389955079601f;
            camera.ScreenSize = new Vector2(720, 576);


        }
        
    }
}
