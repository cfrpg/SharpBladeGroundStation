using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Matrix = Microsoft.Xna.Framework.Matrix;

namespace SharpBladeGroundStation.DataStructs
{
    /// <summary>
    /// 描述摄像机模型的类
    /// </summary>
    public class Camera
    {
        Matrix mountTransfrom;
        Matrix cameraTransfrom;
        Vector2 screenSize;
        float k1;
        float k2;
        float p1;
        float p2;
        /// <summary>
        /// 由体轴系到摄像机轴系的变换矩阵
        /// </summary>
        public Matrix MountTransfrom
        {
            get { return mountTransfrom; }
            set { mountTransfrom = value; }
        }

        public Matrix CameraTransfrom
        {
            get { return cameraTransfrom; }
            set { cameraTransfrom = value; }
        }

        public float K1
        {
            get { return k1; }
            set { k1 = value; }
        }

        public float K2
        {
            get { return k2; }
            set { k2 = value; }
        }

        public float P1
        {
            get { return p1; }
            set { p1 = value; }
        }

        public float P2
        {
            get { return p2; }
            set { p2 = value; }
        }

        public Vector2 ScreenSize
        {
            get { return screenSize; }
            set { screenSize = value; }
        }

        public Camera()
        {
            mountTransfrom = Matrix.Identity;
            cameraTransfrom = Matrix.Identity;
        }

        /// <summary>
        /// 体轴系坐标转换为屏幕坐标
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public Vector2 GetScreenPosition(Vector3 pos)
        {
            Vector3 vt=Vector3.Transform(pos, cameraTransfrom);
            float x0 = (vt.X - screenSize.X / 2) / screenSize.X;
            float y0 = (vt.Y - screenSize.Y / 2) / screenSize.Y;
            float t1, t2;
            float r = x0 * x0 + y0 * y0;
            t1 = x0 * (1 + k1 * r + k2 * r * r);
            t2 = y0 * (1 + k1 * r + k2 * r * r);
            x0 = t1;
            y0 = t2;
            t1 = x0 + (2 * p1 * x0 * y0 + p2 * (r + 2 * x0 * x0));
            t2 = y0 + (p1 * (r + 2 * y0 * y0) + 2 * p2 * x0 * y0);

            return new Vector2(t1,t2);
        }
    }
}
