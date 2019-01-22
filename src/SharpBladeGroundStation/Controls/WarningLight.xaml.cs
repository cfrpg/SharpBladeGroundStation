using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SharpBladeGroundStation
{
	/// <summary>
	/// WarningLight.xaml 的交互逻辑
	/// </summary>
	public partial class WarningLight : UserControl
	{
		public static readonly DependencyProperty TextProperty =
			DependencyProperty.Register("Text", typeof(string), typeof(WarningLight), new PropertyMetadata("", WarningLight.OnTextPropertyChanged));
		public string Text
		{
			get { return (string)GetValue(TextProperty); }
			set { SetValue(TextProperty, value); }
		}

		public static readonly DependencyProperty LevelProperty =
			DependencyProperty.Register("Level", typeof(int), typeof(WarningLight), new PropertyMetadata(0, WarningLight.OnLevelPropertyChanged));
		public int Level
		{
			get { return (int)GetValue(LevelProperty); }
			set { SetValue(LevelProperty, value); }
		}

		public static readonly DependencyProperty MessageProperty =
			DependencyProperty.Register("Message", typeof(string), typeof(WarningLight), new PropertyMetadata("", WarningLight.OnMessagePropertyChanged));
		public string Message
		{
			get { return (string)GetValue(MessageProperty); }
			set { SetValue(MessageProperty, value); }
		}
		public static void OnMessagePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
		{
			WarningLight s = sender as WarningLight;
			s.OnMessageChanged(e);
		}
		public void OnMessageChanged(DependencyPropertyChangedEventArgs e)
		{


		}

		public static void OnLevelPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
		{
			WarningLight s = sender as WarningLight;
			s.OnLevelChanged(e);
		}
		public void OnLevelChanged(DependencyPropertyChangedEventArgs e)
		{


		}

		public static void OnTextPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
		{
			WarningLight s = sender as WarningLight;
			s.OnTextChanged(e);
		}
		public void OnTextChanged(DependencyPropertyChangedEventArgs e)
		{


		}



		public WarningLight()
		{
			InitializeComponent();
		}

		private void light_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			Level = 0;
		}
	}
}
