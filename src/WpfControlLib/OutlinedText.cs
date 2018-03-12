using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfControlLib
{
	/// <summary>
	/// OutlinedText by 楚人Leo
	/// https://www.cnblogs.com/leolion/p/4965885.html
	/// </summary>
	public class OutlinedText : FrameworkElement, IAddChild
	{
		/// <summary>
		/// 静态构造函数
		/// </summary>
		static OutlinedText()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(OutlinedText), new FrameworkPropertyMetadata(typeof(OutlinedText)));
		}


		#region Private Fields

		/// <summary>
		/// 文字几何形状
		/// </summary>
		private Geometry m_TextGeometry;

		#endregion


		#region Private Methods

		/// <summary>     
		/// 当依赖项属性改变文字无效时，创建新的空心文字对象来显示。
		/// </summary>     
		/// <param name="d"></param>     
		/// <param name="e"></param>     
		private static void OnOutlineTextInvalidated(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (Convert.ToString(e.NewValue) != Convert.ToString(e.OldValue))
			{
				((OutlinedText)d).CreateText();
			}
		}

		#endregion


		#region FrameworkElement Overrides

		/// <summary>     
		/// 重写绘制文字的方法。   
		/// </summary>     
		/// <param name="drawingContext">空心文字控件的绘制上下文。</param>     
		protected override void OnRender(DrawingContext drawingContext)
		{
			//CreateText();
			// 基于设置的属性绘制空心文字控件。         
			drawingContext.DrawGeometry(Fill, new Pen(Stroke, StrokeThickness), m_TextGeometry);
		}

		/// <summary>     
		/// 基于格式化文字创建文字的几何轮廓。    
		/// </summary>     
		public void CreateText()
		{
			FontStyle fontStyle = FontStyles.Normal;
			FontWeight fontWeight = FontWeights.Medium;
			if (Bold == true)
				fontWeight = FontWeights.Bold;
			if (Italic == true)
				fontStyle = FontStyles.Italic;
			// 基于设置的属性集创建格式化的文字。        
			FormattedText formattedText = new FormattedText(
				Text, CultureInfo.GetCultureInfo("en-us"), FlowDirection.LeftToRight,
				new Typeface(Font, fontStyle, fontWeight, FontStretches.Normal),
				FontSize, Brushes.Black);
			formattedText.MaxTextWidth = this.MaxTextWidth;
			formattedText.MaxTextHeight = this.MaxTextHeight;
			// 创建表示文字的几何对象。        
			m_TextGeometry = formattedText.BuildGeometry(new Point(0, 0));
			// 基于格式化文字的大小设置空心文字的大小。         
			this.MinWidth = formattedText.Width;
			this.MinHeight = formattedText.Height;
		}

		#endregion


		#region DependencyProperties

		/// <summary>
		/// 指定将文本约束为特定宽度
		/// </summary>
		public double MaxTextWidth
		{
			get { return (double)GetValue(MaxTextWidthProperty); }
			set { SetValue(MaxTextWidthProperty, value); }
		}
		/// <summary>
		/// 指定将文本约束为特定宽度依赖属性
		/// </summary>
		public static readonly DependencyProperty MaxTextWidthProperty =
			DependencyProperty.Register("MaxTextWidth", typeof(double), typeof(OutlinedText),
				new FrameworkPropertyMetadata(1000.0, FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(OnOutlineTextInvalidated), null));

		/// <summary>
		/// 指定将文本约束为特定高度
		/// </summary>
		public double MaxTextHeight
		{
			get { return (double)GetValue(MaxTextHeightProperty); }
			set { SetValue(MaxTextHeightProperty, value); }
		}
		/// <summary>
		/// 指定将文本约束为特定高度依赖属性
		/// </summary>
		public static readonly DependencyProperty MaxTextHeightProperty =
			DependencyProperty.Register("MaxTextHeight", typeof(double), typeof(OutlinedText),
				 new FrameworkPropertyMetadata(1000.0, FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(OnOutlineTextInvalidated), null));

		/// <summary>     
		/// 指定字体是否加粗。   
		/// </summary>     
		public bool Bold
		{
			get { return (bool)GetValue(BoldProperty); }
			set { SetValue(BoldProperty, value); }
		}
		/// <summary>     
		/// 指定字体是否加粗依赖属性。    
		/// </summary>     
		public static readonly DependencyProperty BoldProperty = DependencyProperty.Register(
			"Bold", typeof(bool), typeof(OutlinedText),
			new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(OnOutlineTextInvalidated), null));

		/// <summary>     
		/// 指定填充字体的画刷颜色。    
		/// </summary>     
		/// 
		public Brush Fill
		{
			get { return (Brush)GetValue(FillProperty); }
			set { SetValue(FillProperty, value); }
		}
		/// <summary>     
		/// 指定填充字体的画刷颜色依赖属性。    
		/// </summary>     
		public static readonly DependencyProperty FillProperty = DependencyProperty.Register(
			"Fill", typeof(Brush), typeof(OutlinedText),
			new FrameworkPropertyMetadata(new SolidColorBrush(Colors.LightSteelBlue), FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(OnOutlineTextInvalidated), null));

		/// <summary>     
		/// 指定文字显示的字体。   
		/// </summary>    
		public FontFamily Font
		{
			get { return (FontFamily)GetValue(FontProperty); }
			set { SetValue(FontProperty, value); }
		}
		/// <summary>     
		/// 指定文字显示的字体依赖属性。     
		/// </summary>     
		public static readonly DependencyProperty FontProperty = DependencyProperty.Register(
			"Font", typeof(FontFamily), typeof(OutlinedText),
			new FrameworkPropertyMetadata(new FontFamily("Arial"), FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(OnOutlineTextInvalidated), null));

		/// <summary>     
		/// 指定字体大小。
		/// </summary>     
		public double FontSize
		{
			get { return (double)GetValue(FontSizeProperty); }
			set { SetValue(FontSizeProperty, value); }
		}
		/// <summary>     
		/// 指定字体大小依赖属性。     
		/// </summary>     
		public static readonly DependencyProperty FontSizeProperty = DependencyProperty.Register(
			"FontSize", typeof(double), typeof(OutlinedText),
			new FrameworkPropertyMetadata((double)48.0, FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(OnOutlineTextInvalidated), null));

		/// <summary>     
		/// 指定字体是否显示斜体字体样式。  
		/// </summary>     
		public bool Italic
		{
			get { return (bool)GetValue(ItalicProperty); }
			set { SetValue(ItalicProperty, value); }
		}
		/// <summary>     
		/// 指定字体是否显示斜体字体样式依赖属性。   
		/// </summary>     
		public static readonly DependencyProperty ItalicProperty = DependencyProperty.Register(
			"Italic", typeof(bool), typeof(OutlinedText),
			new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(OnOutlineTextInvalidated), null));

		/// <summary>     
		/// 指定绘制空心字体边框画刷的颜色。    
		/// </summary>     
		public Brush Stroke
		{
			get { return (Brush)GetValue(StrokeProperty); }
			set { SetValue(StrokeProperty, value); }
		}
		/// <summary>     
		/// 指定绘制空心字体边框画刷的颜色依赖属性。    
		/// </summary>     
		public static readonly DependencyProperty StrokeProperty = DependencyProperty.Register(
			"Stroke", typeof(Brush), typeof(OutlinedText),
			new FrameworkPropertyMetadata(new SolidColorBrush(Colors.Teal), FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(OnOutlineTextInvalidated), null));

		/// <summary>     
		/// 指定空心字体边框大小。  
		/// </summary>     
		public double StrokeThickness
		{
			get { return (double)GetValue(StrokeThicknessProperty); }
			set { SetValue(StrokeThicknessProperty, value); }
		}
		/// <summary>     
		/// 指定空心字体边框大小依赖属性。      
		/// </summary>     
		public static readonly DependencyProperty StrokeThicknessProperty =
			DependencyProperty.Register("StrokeThickness",
			typeof(double), typeof(OutlinedText),
			new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(OnOutlineTextInvalidated), null));

		/// <summary>    
		/// 指定要显示的文字字符串。  
		/// </summary>     
		public string Text
		{
			get { return (string)GetValue(TextProperty); }
			set { SetValue(TextProperty, value); }
		}
		/// <summary>     
		/// 指定要显示的文字字符串依赖属性。  
		///  </summary>    
		public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
			"Text", typeof(string), typeof(OutlinedText),
			new FrameworkPropertyMetadata("",
				FrameworkPropertyMetadataOptions.AffectsRender,
				new PropertyChangedCallback(OnOutlineTextInvalidated),
				null));

		#endregion


		#region Public Methods

		/// <summary>
		/// 添加子对象。
		/// </summary>
		/// <param name="value">要添加的子对象。</param>
		public void AddChild(Object value)
		{ }

		/// <summary>
		/// 将节点的文字内容添加到对象。
		/// </summary>
		/// <param name="value">要添加到对象的文字。</param>
		public void AddText(string value)
		{
			Text = value;
		}

		#endregion
	}
}
