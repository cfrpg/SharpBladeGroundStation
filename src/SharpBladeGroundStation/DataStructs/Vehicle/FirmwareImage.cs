using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;
using SharpBladeGroundStation.CommunicationLink.BootLoader;

namespace SharpBladeGroundStation.DataStructs
{ 
	public class FirmwareImage
	{
		Dictionary<string, string> fields;
		byte[] image;
		int imageSize;
		BoardID targetBoard;


		public Dictionary<string, string> Fields
		{
			get { return fields; }
			set { fields = value; }
		}

		/// <summary>
		/// 固件本体的缓存，不要使用Length！
		/// </summary>
		public byte[] Image
		{
			get { return image; }
			set { image = value; }
		}

		/// <summary>
		/// 固件的实际长度
		/// </summary>
		public int ImageSize
		{
			get { return imageSize; }
			set { imageSize = value; }
		}

		/// <summary>
		/// 目标硬件
		/// </summary>
		public BoardID TargetBoard
		{
			get { return targetBoard; }
			set { targetBoard = value; }
		}

		public FirmwareImage()
		{
			fields = new Dictionary<string, string>();
		}

		public bool Load(string path)
		{
			StreamReader file=null;
			
			try
			{
				file = new StreamReader(path);				
				while (!file.EndOfStream)
				{
					string line = file.ReadLine();
					if (line == "{" || line == "}")
					{
						continue;
					}
					string[] strs = line.Split(":".ToCharArray());
					strs[0] = strs[0].Trim().Trim("\"".ToCharArray());
					strs[1] = strs[1].Trim().TrimEnd(",".ToCharArray()).Trim().Trim("\"".ToCharArray());
					fields.Add(strs[0], strs[1]);
				}
				file.Close();
				string imagestr = fields["image"];
				byte[] buff = Convert.FromBase64String(imagestr);
				int maxsize = int.Parse(fields["image_maxsize"]);
				using (DeflateStream ds = new DeflateStream(new MemoryStream(buff, 2, buff.Length - 2), CompressionMode.Decompress))
				{
					image = new byte[maxsize];
					imageSize = ds.Read(image, 0, maxsize);
				}
				targetBoard = (BoardID)int.Parse(fields["board_id"]);
			}
			catch
			{
				file?.Close();
				return false;
			}
			return true;
		}
	}
}
