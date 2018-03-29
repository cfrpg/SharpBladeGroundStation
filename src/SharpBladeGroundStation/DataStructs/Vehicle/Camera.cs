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

        /// <summary>
        /// 由体轴系到摄像机轴系的变换矩阵
        /// </summary>
        public Matrix MountTransfrom
        {
            get { return mountTransfrom; }
            set { mountTransfrom = value; }
        }
    }
}
