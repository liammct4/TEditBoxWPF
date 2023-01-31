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

				value.CollectionChanged += LinesChanged_Event;
				UpdateLineCount();

				OnPropertyChanged(nameof(Lines));
			}
		}
		internal ObservableCollection<TLine> _lines;

		public TIndex TextEnd => new(Lines.Count - 1, Lines.Last().Text.Length);

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
			InitializeComponent();

			// The text box can never be empty, it always has to have at least one line.
			Lines = new ObservableCollection<TLine>() { new TLine(this, "") };

			tabConverter = (TextTabWidthConverter)Resources["tabConverter"];
			tabConverter.parent = this;

			TextDisplayScrollViewer = TextDisplay.GetDescendantByType<SmoothScrollviewer>();
			LineNumbersScrollViewer = LineNumberDisplay.GetDescendantByType<SmoothScrollviewer>();

			MainCaret = new TCaret(this);

			// Handle set properties.
			Loaded += TEditBox_Loaded;
		}

		/// <summary>
		/// Updates the line numbers whenever a line is added or removed.
		/// </summary>
		private void LinesChanged_Event(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			UpdateLineCount();
		}

		/// <summary>
		/// Updates the current line numbers.
		/// </summary>
		private void UpdateLineCount()
		{
			int currentLineCount = Lines.Count;
			int previousLineCount = LineNumberDisplay.Items.Count;

			if (currentLineCount > previousLineCount)
			{
				for (int i = previousLineCount; i < currentLineCount; i++)
				{
					LineNumberDisplay.Items.Add(i + 1);
				}

				return;
			}

			for (int i = previousLineCount; i > currentLineCount; i--)
			{
				LineNumberDisplay.Items.RemoveAt(i - 1);
			}
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
		/// Retrieves a line at a set position, if the line does not exist, a new line is created.
		/// </summary>
		/// <returns>A new or already existing line.</returns>
		private TLine ResolveLine(int line)
		{
			if (line > Lines.Count - 1)
			{
				for (int i = Lines.Count; i < line; i++)
				{
					Lines.Add(new TLine(this, string.Empty));
				}

				TLine endLine = new(this, string.Empty);
				Lines.Add(endLine);

				return endLine;
			}
			
			return Lines[line];
		}

		/// <summary>
		/// Inserts a string of text at the position <paramref name="position"/>.
		/// </summary>
		/// <param name="position">The position of the text.</param>
		/// <param name="text">The text to insert.</param>
		public TIndex InsertText(TIndex position, string text)
		{
			// String.Split cannot tell the difference between an empty string or a newline. So handle manually.
			if (text == Environment.NewLine)
			{
				TLine currentLine = ResolveLine(position.Line);
				TLine newLine = new(this, currentLine.Text[position.Character..]);

				currentLine.Text = currentLine.Text[0..position.Character];

				Lines.Insert(position.Line + 1, newLine);
				
				return new TIndex(position.Line + 1, 0);
			}
			
			string[] lines = text.Split(Environment.NewLine, StringSplitOptions.None);

			// No extra lines at all.
			if (lines.Length == 1)
			{
				TLine line = ResolveLine(position.Line);
				line.Text = line.Text.Insert(position.Character, lines.First());

				return new TIndex(position.Line, position.Character + lines.First().Length);
			}

			// Inserting multiple lines.
			TLine firstLine = ResolveLine(position.Line);
			string endText = firstLine.Text[position.Character..];

			firstLine.Text = firstLine.Text[0..(position.Character)] + lines.First();

			TLine lastLine = new(this, lines.Last() + endText);
			Lines.Insert(position.Line + 1, lastLine);

			for (int i = 1; i < lines.Length - 1; i++)
			{
				TLine newLine = new(this, lines[i]);

				if (position.Line + i > Lines.Count)
				{
					Lines.Add(newLine);
					continue;
				}
				
				Lines.Insert(position.Line + i, newLine);
			}

			return new TIndex(lastLine.Position, lines.Last().Length);
		}

		/// <summary>
		/// Gets a selection of text from the textbox at <paramref name="startPosition"/> and <paramref name="endPosition"/>.
		/// </summary>
		/// <param name="startPosition"></param>
		/// <param name="endPosition"></param>
		/// <returns></returns>
		public string GetText(TIndex startPosition, TIndex endPosition)
		{
			TIndex start = TIndex.Min(startPosition, endPosition);
			TIndex end = TIndex.Max(startPosition, endPosition);

			if (start.Line == end.Line)
			{
				TLine line = Lines[start.Line];

				return line.Text[start.Character..end.Character];
			}

			TLine firstLine = Lines[start.Line];
			TLine lastLine = Lines[end.Line];

			StringBuilder joinSb = new StringBuilder()
				.AppendLine(firstLine.Text[start.Character..]);

			for (int i = start.Line + 1; i < end.Line; i++)
			{
				TLine line = Lines[i];
				joinSb.AppendLine(line.Text);
			}

			joinSb.Append(lastLine.Text[..end.Character]);

			return joinSb.ToString();
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
				case Key.Tab:
					MainCaret.InputText('\t');
					break;
				case Key.Enter:
					MainCaret.InputText(Environment.NewLine);
					break;
				// Copying. (Ctrl + C)
				case Key.C when ctrlModifer:
					{
						string copy = MainCaret.SelectionText;
						Clipboard.SetText(copy);
					}
					break;
				// Pasting. (Ctrl + V)
				case Key.V when ctrlModifer:
					{
						string clipboard = Clipboard.GetText();
						MainCaret.InputText(clipboard);
					}
					break;
				// Cutting. (Ctrl + X)
				case Key.X when ctrlModifer:
					{
						string text = MainCaret.SelectionText;
						
						MainCaret.DeleteSelectedText();
						Clipboard.SetText(text);
					}
					break;
				// Select All. (Ctrl + A)
				case Key.A when ctrlModifer:
					{
						MainCaret.SelectStartPosition = TIndex.Start;
						MainCaret.Position = TextEnd;

						e.Handled = true;
					}
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
			// These keys need to be handled manually, setting handled = true will stop the TextBox_TextInput event firing,
			// which will stop inserting bad characters such as \b.
			if (HelperData.interceptKeys.Contains(e.Key))
			{
				e.Handled = true;
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