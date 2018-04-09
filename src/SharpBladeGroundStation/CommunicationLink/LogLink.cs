using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace SharpBladeGroundStation.CommunicationLink
{
	public class LogLink
	{
		string path;
		string filename;

		public string Path
		{
			get { return path; }
		}

		public string Filename
		{
			get { return filename; }
		}
	}
}
