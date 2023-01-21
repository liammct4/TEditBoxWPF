using CustomTextBoxComponent.Textbox.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
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
using TEditBoxWPF.Controls.ScrollableItemsControl;
using TEditBoxWPF.Converters;
using TEditBoxWPF.LineStructure;
using TEditBoxWPF.Objects;
using TEditBoxWPF.TextStructure;
using TEditBoxWPF.Utilities;

namespace TEditBoxWPF
{
    /// <summary>
    /// A text control allowing plaintext document and file editing.
    /// </summary>
    public partial class TEditBox : UserControl, INotifyPropertyChanged
	{
		/// <summary>
		/// The text content loaded into this text box.
		/// </summary>
		public string Text
		{
			get => string.Join('\n', Lines.Select(x => x.Text));
			set
			{
				Lines = new ObservableCollection<TLine>(value
					.Replace(Environment.NewLine, "\n")
					.Split("\n")
					.Select(x => new TLine(this, x)));
				MainCaret.Position = TIndex.Start;
				MainCaret.SelectStartPosition = TIndex.Start;
			}
		}

		/// <summary>
		/// A manually adjustable collection of lines which represent the text.
		/// </summary>
		public ObservableCollection<TLine> Lines
		{
			get => _lines;
			set
			{
				_lines = value;
				OnPropertyChanged(nameof(Lines));
			}
		}
		internal ObservableCollection<TLine> _lines;

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
		private bool _showLineNumbers = true;

		public int TabSize
		{
			get => _tabSize;
			set
			{
				_tabSize = value;
				measurer.MeasuringOptions.TabSize = value;
			}
		}
		private int _tabSize = 8;
		private VirtualizedTextObject<Rectangle> TestObject;

		/// <summary>
		/// The font family which will be used to render the text.
		/// </summary>
		public new FontFamily FontFamily
		{
			get => base.FontFamily;
			set
			{
				base.FontFamily = value;
				measurer.MeasuringOptions.FontFamily = value.Source;
			}
		}

		/// <summary>
		/// The em size of the text in the text box.
		/// </summary>
		public new double FontSize
		{
			get => base.FontSize;
			set
			{
				base.FontSize = value;
				measurer.MeasuringOptions.FontSize = value;
			}
		}

		/// <summary>
		/// Determines how the line numbers will be aligned relative to the line numbers side bar.
		/// </summary>
		public HorizontalAlignment LineNumberAlignment
		{
			get => _lineNumberAlignment;
			set
			{
				_lineNumberAlignment = value;
				OnPropertyChanged(nameof(ShowLineNumbers));
			}
		}
		private HorizontalAlignment _lineNumberAlignment;

		public TCaret MainCaret { get; }

		internal readonly TextMeasurer measurer = new();
		internal readonly TextTabWidthConverter tabConverter;
		private readonly ScrollViewer TextDisplayScrollViewer; 
		private readonly ScrollViewer LineNumbersScrollViewer;

		public TEditBox()
		{
			// The text box can never be empty, it always has to have at least one line.
			_lines = new ObservableCollection<TLine>() { new TLine(this, "") };

			InitializeComponent();

			tabConverter = (TextTabWidthConverter)Resources["tabConverter"];
			tabConverter.parent = this;

			TextDisplayScrollViewer = TextDisplay.GetDescendantByType<SmoothScrollviewer>();
			LineNumbersScrollViewer = LineNumberDisplay.GetDescendantByType<SmoothScrollviewer>();

			MainCaret = new TCaret(this);

			// Handle set properties.
			Loaded += TEditBox_Loaded;
		}

		/// <summary>
		/// Loads the text measurer <see cref="measurer"/> with the specified font settings.
		/// 
		/// The font settings specified on the control (e.g. &lt;text:TEditBox FontFamily="Consolas" FontSize="15"/&gt;)
		/// are only accessable after the control has been loaded.
		/// </summary>
		private void TEditBox_Loaded(object sender, RoutedEventArgs e)
		{
			TextMeasureOptions options = new TextMeasureOptions()
			{
				FontFamily = FontFamily.Source,
				FontSize = FontSize,
				TabSize = TabSize
			};

			measurer.MeasuringOptions = options;

			MainCaret.caretLine.IsPlaced = true;
		}

		/// <summary>
		/// Removes a selection of text in between two points.
		/// </summary>
		/// <param name="startPosition">The first position.</param>
		/// <param name="endPosition">The second position.</param>
		public void DeleteText(TIndex startPosition, TIndex endPosition)
		{
			TIndex beginningIndex = TIndex.Min(startPosition, endPosition);
			TIndex endIndex = TIndex.Max(startPosition, endPosition);

			if (beginningIndex.Line == endIndex.Line)
			{
				Lines[beginningIndex.Line].DeleteText(beginningIndex.Character, endIndex.Character);
				return;
			}

			// Remove the text after the start of the first position.
			TLine startLine = Lines[beginningIndex.Line];
			startLine.DeleteText(beginningIndex.Character, startLine.Text.Length);

			// Store the end text to merge.
			TLine endLine = Lines[endIndex.Line];
			string endText = endLine.Text[endIndex.Character..endLine.Text.Length];

			// Merge the text from the last line onto the first line.
			startLine.Text += endText;
			
			Lines.RemoveAt(endIndex.Line);
			startLine.RefreshText();

			// Remove in between lines.
			for (int i = endIndex.Line - 1; i > beginningIndex.Line; i--)
			{
				Lines.RemoveAt(i);
			}
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

		/// <summary>
		/// Occurs whenever a key has been pressed inside the control.
		/// </summary>
		private void ControlInput_Event(object sender, KeyEventArgs e)
		{
			bool ctrlModifer = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
			bool shiftModifer = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);

			switch (e.Key)
			{
				case Key.Left:
					if (ctrlModifer)
					{
						MainCaret.SkipLeft(!shiftModifer);
					}
					else
					{
						MainCaret.MoveChar(-1, !shiftModifer);
					}
					break;
				case Key.Right:
					if (ctrlModifer)
					{
						MainCaret.SkipRight(!shiftModifer);
					}
					else
					{
						MainCaret.MoveChar(1, !shiftModifer);
					}
					break;
				case Key.Up:
					MainCaret.MoveLine(-1, !shiftModifer);
					break;
				case Key.Down:
					MainCaret.MoveLine(1, !shiftModifer);
					break;
				case Key.Back:
					MainCaret.Backspace();
					break;
			}
		}

		/// <summary>
		/// Moves the caret to the clicked line and character position.
		/// </summary>
		private void TextClick_Event(object sender, MouseButtonEventArgs e)
		{
			Grid source = (Grid)sender;
			ContentPresenter item = source.GetParentByType<ContentPresenter>();
			TLine line = (TLine)TextDisplay.ItemContainerGenerator.ItemFromContainer(item);

			int clickedCharIndex = line.GetCharacterAtPixel(e.GetPosition(item).X);

			MainCaret.Position = new TIndex(line.Position, clickedCharIndex);
			MainCaret.SelectStartPosition = new TIndex(line.Position, clickedCharIndex);
		}

		/// <summary>
		/// Handles caret select dragging.
		/// </summary>
		private void TextMouseMove_Event(object sender, MouseEventArgs e)
		{
			if (Mouse.LeftButton == MouseButtonState.Pressed)
			{
				Grid source = (Grid)sender;
				ContentPresenter item = source.GetParentByType<ContentPresenter>();
				TLine line = (TLine)TextDisplay.ItemContainerGenerator.ItemFromContainer(item);

				int clickedCharIndex = line.GetCharacterAtPixel(e.GetPosition(item).X);

				MainCaret.Position = new TIndex(line.Position, clickedCharIndex);
			}
		}

		/// <summary>
		/// Adds the keyboard input at the main caret position.
		/// </summary>
		private void TextBox_TextInput(object sender, TextCompositionEventArgs e)
		{
			MainCaret.InputText(e.Text);
		}

		/// <summary>
		/// Stops the scrollviewer from adjusting when the arrow keys are pressed.
		/// </summary>
		private void TextDisplay_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			switch (e.Key)
			{
				case Key.Down:
				case Key.Up:
				case Key.Left:
				case Key.Right:
				case Key.Back:
					e.Handled = true;
					break;
			}
		}

		#region Property Changed
		public event PropertyChangedEventHandler? PropertyChanged;

		private void OnPropertyChanged(string property)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
		}
		#endregion
	}
}