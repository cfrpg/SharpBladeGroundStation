using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace SharpBladeGroundStation
{
	public delegate void MFDButtonEvent();

	public class MFDFunction : INotifyPropertyChanged
	{
		string name;


		public event PropertyChangedEventHandler PropertyChanged;

		public string Name
		{
			get { return name; }
			set
			{
				name = value;
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Name"));
			}
		}
	}
}
