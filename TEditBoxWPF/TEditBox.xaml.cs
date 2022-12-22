using CustomTextBoxComponent.Textbox.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using TEditBoxWPF.Converters;
using TEditBoxWPF.LineStructure;

namespace TEditBoxWPF
{
	public partial class TEditBox : UserControl
	{
		public string Text
		{
			get => _text;
			set
			{
				_text = value;
				Lines = _text.Replace(Environment.NewLine, "\n").Split("\n").Select(x => new TLine(x)).ToList();
			}
		}
		private string _text;
		internal List<TLine> Lines
		{
			get => _lines;
			set
			{
				_lines = value;
				TextDisplay.ItemsSource = value;
			}
		}
		internal List<TLine> _lines;
		public new FontFamily FontFamily
		{
			get => base.FontFamily;
			set
			{
				base.FontFamily = value;
				measurer.MeasuringOptions.FontFamily = value.Source;
			}
		}
		public new double FontSize
		{
			get => base.FontSize;
			set
			{
				base.FontSize = value;
				measurer.MeasuringOptions.FontSize = value;
			}
		}
		internal readonly TextMeasurer measurer = new TextMeasurer();

		public TEditBox()
		{
			InitializeComponent();
			TextTabWidthConverter converter = (TextTabWidthConverter)Resources["tabConverter"];
			converter.parent = this;

			Loaded += TEditBox_Loaded;
		}

		private void TEditBox_Loaded(object sender, RoutedEventArgs e)
		{
			TextMeasureOptions options = new TextMeasureOptions()
			{
				FontFamily = FontFamily.Source,
				FontSize = FontSize
			};

			measurer.MeasuringOptions = options;
		}
	}
}