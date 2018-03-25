using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPFMathHelper
{
	public class BigMatrix
	{
		double[,] value;
		int column;
		int row;

		public double[,] Value
		{
			get { return value; }
			set { this.value = value; }
		}

		public int Column
		{
			get			{				return column;			}
			set			{				column = value;			}
		}

		public int Row
		{
			get			{				return row;			}
			set			{				row = value;			}
		}

		public BigMatrix(int r,int c)
		{
			column = c;
			row = r;
			value = new double[r, c];
		}
		public BigMatrix Clone()
		{
			BigMatrix r = new BigMatrix(row, column);
			for (int i = 0; i < row; i++)
			{
				for (int j = 0; j < column; j++)
				{
					r.value[i, j] = value[i, j];
				}
			}
			return r;
		}

		public static BigMatrix Mul(BigMatrix m1,BigMatrix m2)
		{
			BigMatrix res = new BigMatrix(m1.row, m2.column);
			for(int i=0;i<m1.row;i++)
			{
				for(int j=0;j<m2.column;j++)
				{
					res.value[i, j] = 0;
					for(int k=0;k<m1.column;k++)
					{
						res.value[i, j] += m1.value[i, k] * m2.value[k, j];
					}
				}
			}
			return res;
		}

		public static BigMatrix CreateRotationX(double r)
		{
			BigMatrix res = new BigMatrix(3, 3);
			res.value[0, 0] = 1;
			res.value[1, 1] = Math.Cos(r);
			res.value[1, 2] = Math.Sin(r);
			res.value[2, 1] = -Math.Sin(r);
			res.value[2, 2] = Math.Cos(r);
			return res;
		}
		public static BigMatrix CreateRotationY(double r)
		{
			BigMatrix res = new BigMatrix(3, 3);
			res.value[1, 1] = 1;
			res.value[0, 0] = Math.Cos(r);
			res.value[2, 0] = Math.Sin(r);
			res.value[0, 2] = -Math.Sin(r);
			res.value[2, 2] = Math.Cos(r);
			return res;
		}
		public static BigMatrix CreateRotationZ(double r)
		{
			BigMatrix res = new BigMatrix(3, 3);
			res.value[2, 2] = 1;
			res.value[0, 0] = Math.Cos(r);
			res.value[0, 2] = Math.Sin(r);
			res.value[1, 0] = -Math.Sin(r);
			res.value[1, 1] = Math.Cos(r);
			return res;
		}
		public static BigMatrix CreateRotation(double x,double y,double z)
		{
			return BigMatrix.Mul(BigMatrix.Mul(BigMatrix.CreateRotationX(x), BigMatrix.CreateRotationY(y)),BigMatrix.CreateRotationZ(z));
		}
		
	}
}
