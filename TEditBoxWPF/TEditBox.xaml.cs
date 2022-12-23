using CustomTextBoxComponent.Textbox.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using TEditBoxWPF.Controls;
using TEditBoxWPF.Converters;
using TEditBoxWPF.LineStructure;
using TEditBoxWPF.Utilities;

namespace TEditBoxWPF
{
	public partial class TEditBox : UserControl, INotifyPropertyChanged
	{
		public string Text
		{
			get => string.Join('\n', Lines.Select(x => x.Text));
			set
			{
				Lines = value
					.Replace(Environment.NewLine, "\n")
					.Split("\n")
					.Select(x => new TLine(x))
					.ToList();
			}
		}

		public List<TLine> Lines
		{
			get => _lines;
			set
			{
				_lines = value;
				OnPropertyChanged(nameof(Lines));
			}
		}
		internal List<TLine> _lines;

		/// <summary>
		/// Determines if the text line numbers are shown.
		/// </summary>
		public bool ShowLineNumbers
		{
			get => _showLineNumbers;
			set
			{
				_showLineNumbers = value;
				OnPropertyChanged(nameof(ShowLineNumbers));
			}
		}
		private bool _showLineNumbers = false;

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
		internal readonly TextMeasurer measurer = new();
		private ScrollViewer TextDisplayScrollViewer;
		private ScrollViewer LineNumbersScrollViewer;

		public TEditBox()
		{
			InitializeComponent();
			TextTabWidthConverter converter = (TextTabWidthConverter)Resources["tabConverter"];
			converter.parent = this;

			TextDisplayScrollViewer = TextDisplay.GetDescendantByType<SmoothScrollviewer>();
			LineNumbersScrollViewer = LineNumberDisplay.GetDescendantByType<SmoothScrollviewer>();

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

		/// <summary>
		/// Fired when the text box has been scrolled. Syncs the textbox scroll to the line number scroll.
		/// </summary>
		private void TextboxSyncScroll_Event(object sender, ScrollChangedEventArgs e) => LineNumbersScrollViewer.ScrollToVerticalOffset(e.VerticalOffset);

		/// <summary>
		/// Fired when the line number box has been scrolled using the mouse wheel.
		/// Syncs the scroll from the line number box to the textbox.
		/// </summary>
		private void LineNumbersScroll_Event(object sender, ScrollChangedEventArgs e) => TextDisplayScrollViewer.ScrollToVerticalOffset(e.VerticalOffset);

		#region Property Changed
		public event PropertyChangedEventHandler? PropertyChanged;
		
		public void OnPropertyChanged(string property)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
		}
		#endregion
	}
}