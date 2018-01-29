using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using SlimDX.DirectInput;
using System.Collections.ObjectModel;

namespace FlightDisplay
{
	public class JoystickData:INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;
		ObservableCollection<ChannelData> channels;
		ObservableCollection<bool> keys;
		int[] calidata;
		int calicount;

		public JoystickData()
		{
			channels = new ObservableCollection<ChannelData>();
			for(int i=0;i<8;i++)
			{
				channels.Add(new ChannelData(i));
			}
			calidata = new int[8];
		}
		
		public void Update(JoystickState state)
		{
			channels[0].RawValue = state.X;
			channels[1].RawValue = state.Y;
			channels[2].RawValue = state.Z;
			channels[3].RawValue = state.TorqueX;
			channels[4].RawValue = state.TorqueY;
			channels[5].RawValue = state.TorqueZ;
			int[] s = state.GetSliders();
			channels[6].RawValue = s[0];
			channels[7].RawValue = s[1];
		}

		public void CalibrateEndPoint(JoystickState state,CalibratePhase cp)
		{
			switch (cp)
			{
				case CalibratePhase.Start:
					for(int i=0;i<8;i++)
					{
						channels[i].MaxValue = 10 + channels[i].Offset;
						channels[i].MinValue = -10 + channels[i].Offset;
					}
					break;
				case CalibratePhase.Working:
					Update(state);
					for(int i=0;i<8;i++)
					{
						channels[i].MaxValue = Math.Max(channels[i].MaxValue, channels[i].RawValue);
						channels[i].MinValue = Math.Min(channels[i].MinValue, channels[i].RawValue);							
					}
					break;				
				default:
					Update(state);
					break;
			}
			Update(state);
		}

		public void CalibrateOffset(JoystickState state,CalibratePhase cp)
		{
			switch (cp)
			{
				case CalibratePhase.Start:
					for (int i = 0; i < 8; i++)
					{
						calidata[i] = 0;
						channels[i].Offset = 0;
					}
					calicount = 0;
					break;
				case CalibratePhase.Working:
					Update(state);
					for(int i=0;i<8;i++)
					{
						calidata[i] += channels[i].RawValue;
					}
					calicount++;
					break;
				case CalibratePhase.End:
					for(int i=0;i<8;i++)
					{
						channels[i].Offset = calidata[i] / calicount;
					}
					break;
				default:
					break;
			}
		}
	}

	public enum CalibratePhase
	{
		Start,
		Working,
		End
	}
}
