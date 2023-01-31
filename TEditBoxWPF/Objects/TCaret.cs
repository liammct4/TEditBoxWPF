using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using TEditBoxWPF.LineStructure;
using TEditBoxWPF.TextStructure;
using System.Windows.Media.Animation;
using TEditBoxWPF.Utilities;
using System.Diagnostics;

namespace TEditBoxWPF.Objects
{
	/// <summary>
	/// Represents a caret inside a <see cref="TEditBox"/> which responds
	/// to user input.
	/// </summary>
	public class TCaret
	{
		public static SolidColorBrush SelectColour = new SolidColorBrush(Color.FromArgb(82, 64, 157, 208));

		/// <summary>
		/// The text box this caret is contained in.
		/// </summary>
		public TEditBox Parent { get; }

		/// <summary>
		/// Gets or sets the line and character position of this caret. Index is 0 based.
		/// </summary>
		public TIndex Position
		{
			get => caretLine.Position;
			set
			{
				if (caretLine.Position != value)
				{
					caretLine.Position = value;
					caretLine.VirtualizedObject.Visibility = Visibility.Visible;

					Parent.TextDisplay.ScrollIntoView(value.Line);

					caretBlinkingTimer.Reset();

					RefreshSelection();
				}
			}
		}

		/// <summary>
		/// Gets or sets the line and character position of the selection begin. This is at the opposite end of the caret line.
		/// </summary>
		public TIndex SelectStartPosition
		{
			get => _selectStartPosition;
			set
			{
				_selectStartPosition = value;
				RefreshSelection();
			}
		}
		private TIndex _selectStartPosition;

		/// <summary>
		/// Gets the text selected under this caret between <see cref="Position"/> and <see cref="SelectStartPosition"/>.
		/// </summary>
		public string SelectionText => Parent.GetText(Position, SelectStartPosition);

		private VirtualizedTextObject<Rectangle> SelectFirst => selection.First();
		private VirtualizedTextObject<Rectangle> SelectLast => selection.Last();

		internal VirtualizedTextObject<Rectangle> caretLine;
		internal readonly List<VirtualizedTextObject<Rectangle>> selection = new();
		private readonly Timer caretBlinkingTimer = new()
		{
			Interval = 700,
		};

		public TCaret(TEditBox parent)
		{
			Parent = parent;

			Rectangle line = new()
			{
				Width = 2,
				Fill = new SolidColorBrush(Colors.Black)
			};
			caretLine = new VirtualizedTextObject<Rectangle>(
				parent: parent,
				line: parent.Lines.First(),
				virtualizationPanel: parent.TextDisplay,
				control: line);

			caretBlinkingTimer.Elapsed += CaretFlicker_Event;
			
			caretBlinkingTimer.Start();
		}

		private void RefreshSelection()
		{
			selection.ForEach(x => x.IsPlaced = false);
			selection.Clear();

			TIndex indDifference = SelectStartPosition - Position;
			double difference = Math.Abs(indDifference.Line);

			if (difference == 0)
			{
				TLine line = Parent.Lines[Position.Line];
				int startChar = Math.Min(Position.Character, SelectStartPosition.Character);
				int endChar = Math.Max(Position.Character, SelectStartPosition.Character);

				Rectangle rect = new()
				{
					Fill = SelectColour,
					Width = Parent.measurer.MeasureTextSize(line.Text[startChar..endChar], true).Width
				};
				VirtualizedTextObject<Rectangle> select = new(Parent, line, Parent.TextDisplay, rect)
				{
					IsPlaced = true,
				};
				select.Position = select.Position.OffsetCharacter(startChar);

				selection.Add(select);
			}
			else
			{
				// Prevent the selection from overflowing the text size.
				TIndex firstIndex = TIndex.Min(TIndex.Min(SelectStartPosition, Position), Parent.TextEnd);
				TIndex lastIndex = TIndex.Min(TIndex.Max(SelectStartPosition, Position), Parent.TextEnd);

				TLine firstLine = Parent.Lines[firstIndex.Line];
				TLine lastLine = Parent.Lines[lastIndex.Line];

				VirtualizedTextObject<Rectangle> firstRect = new VirtualizedTextObject<Rectangle>(
					parent: Parent,
					line: firstLine,
					virtualizationPanel: Parent.TextDisplay,
					control: new Rectangle()
					{
						Fill = SelectColour,
						Width = Parent.measurer.MeasureTextSize(firstLine.Text[firstIndex.Character..firstLine.Text.Length] + ' ', true).Width
					}
				)
				{
					IsPlaced = true
				};
				VirtualizedTextObject<Rectangle> lastRect = new VirtualizedTextObject<Rectangle>(
					parent: Parent,
					line: lastLine,
					virtualizationPanel: Parent.TextDisplay,
					control: new Rectangle()
					{
						Fill = SelectColour,
						Width = Parent.measurer.MeasureTextSize(lastLine.Text[0..lastIndex.Character], true).Width
					}
				)
				{
					IsPlaced = true
				};

				firstRect.Position = new(firstRect.Position.Line, firstIndex.Character);
				lastRect.Position = new(lastRect.Position.Line, 0);

				selection.Add(firstRect);
				selection.Add(lastRect);


				for (int i = firstIndex.Line + 1; i < lastIndex.Line; i++)
				{
					VirtualizedTextObject<Rectangle> rect = new VirtualizedTextObject<Rectangle>(
						parent: Parent,
						line: Parent.Lines[i],
						virtualizationPanel: Parent.TextDisplay,
						control: new Rectangle()
						{
							Fill = SelectColour,
							Width = Parent.measurer.MeasureTextSize(Parent.Lines[i].Text + ' ', true).Width
						}
					)
					{
						IsPlaced = true
					};

					selection.Add(rect);
				}
			}
		}

		/// <summary>
		/// Moves the caret by the specified character offset, the caret can move on to the next or previous lines.
		/// </summary>
		/// <param name="charOffset">The number of characters to offset the caret by.</param>
		public void MoveChar(int charOffset, bool changeSelect)
		{
			TLine currentLine = caretLine.Line;
			TIndex currentPosition = caretLine.Position;
			string text = currentLine.Text;

			TIndex newPosition = currentPosition.OffsetCharacter(charOffset);

			if (newPosition.Character < 0)
			{
				newPosition = newPosition.OffsetLine(-1);

				if (newPosition.Line >= 0)
				{
					string previousLineText = Parent.Lines[newPosition.Line].Text;

					newPosition.Character = previousLineText.Length;
				}
				else
				{
					newPosition.Line += 1;
					newPosition.Character = 0;
				}
			}
			else if (newPosition.Character > text.Length)
			{
				newPosition = newPosition.OffsetLine(1);

				if (newPosition.Line >= Parent.Lines.Count)
				{
					return;
				}

				newPosition.Character = 0;
			}

			Position = newPosition;

			if (changeSelect)
			{
				SelectStartPosition = newPosition;
			}
		}

		/// <summary>
		/// Moves the caret by the specified line offset.
		/// </summary>
		/// <param name="lineOffset">The number of lines to offset the caret by.</param>
		public void MoveLine(int lineOffset, bool changeSelect)
		{
			TIndex currentPosition = caretLine.Position;

			currentPosition = currentPosition.OffsetLine(lineOffset);

			if (currentPosition.Line < 0 || currentPosition.Line >= Parent.Lines.Count)
			{
				return;
			}

			string nextLineText = Parent.Lines[currentPosition.Line].Text;

			currentPosition.Character = Math.Min(nextLineText.Length, currentPosition.Character);

			Position = currentPosition;

			if (changeSelect)
			{
				SelectStartPosition = currentPosition;
			}
		}

		/// <summary>
		/// Moves the caret to the left of the current word.
		/// </summary>
		public void SkipLeft(bool changeSelect)
		{
			TIndex position = Position;
			string lineText = Parent.Lines[position.Line].Text;

			int index = -1;
			bool characterVisited = false;

			for (int i = position.Character - 1; i > 0; i--)
			{
				bool isStop = char.IsPunctuation(lineText[i]) || char.IsWhiteSpace(lineText[i]);

				if (!isStop)
				{
					characterVisited = true;
				}

				if (isStop && characterVisited)
				{
					index = i;
					break;
				}
			}

			if (index == -1)
			{
				if (position.Character != 0)
				{
					position.Character = 0;
				}
				else
				{
					MoveChar(-1, changeSelect);
					return;
				}
			}
			else
			{
				position.Character = index + 1;
			}

			Position = position;

			if (changeSelect)
			{
				SelectStartPosition = position;
			}
		}

		/// <summary>
		/// Moves the caret to the right of the current word.
		/// </summary>
		public void SkipRight(bool changeSelect)
		{
			TIndex position = Position;
			string lineText = Parent.Lines[position.Line].Text;

			int index = -1;
			bool characterVisited = false;

			for (int i = position.Character; i < lineText.Length; i++)
			{
				bool isStop = char.IsPunctuation(lineText[i]) || char.IsWhiteSpace(lineText[i]);

				if (!isStop)
				{
					characterVisited = true;
				}

				if (isStop && characterVisited)
				{
					index = i;
					break;
				}
			}

			if (index == -1)
			{
				if (position.Character != lineText.Length)
				{
					position.Character = lineText.Length;
				}
				else
				{
					MoveChar(1, changeSelect);
					return;
				}
			}
			else
			{
				position.Character = index;
			}

			Position = position;

			if (changeSelect)
			{
				SelectStartPosition = position;
			}
		}

		/// <summary>
		/// Inserts a character before the caret position.
		/// </summary>
		/// <param name="character">The character to insert.</param>
		public void InputText(char character) => InputText(character.ToString());

		/// <summary>
		/// Inserts a string of text before the caret position.
		/// </summary>
		/// <param name="text">The text to insert.</param>
		public void InputText(string text)
		{
			Position = Parent.InsertText(Position, text);
			SelectStartPosition = Position;
		}

		/// <summary>
		/// Performs a backspace.
		/// </summary>
		public void Backspace()
		{
			if (Position == SelectStartPosition)
			{
				if (Position.Character == 0)
				{
					int previousIndex = Position.Line - 1;

					if (previousIndex >= 0)
					{
						TLine previousLine = Parent.Lines[previousIndex];
						int previousWidth = previousLine.Text.Length;

						string text = caretLine.Line.Text;
						Parent.Lines.Remove(caretLine.Line);

						previousLine.Text += text;
						previousLine.RefreshText();

						Position = new TIndex(previousIndex, previousWidth);
						SelectStartPosition = new TIndex(previousIndex, previousWidth);
					}

					return;
				}

				Position = Position.OffsetCharacter(-1);
				SelectStartPosition = Position;

				caretLine.Line.DeleteText(Position.Character, Position.Character + 1);

				return;
			}

			DeleteSelectedText();
		}

		/// <summary>
		/// Removes the text in between <see cref="Position"/> and <see cref="SelectStartPosition"/>.
		/// </summary>
		public void DeleteSelectedText()
		{
			TIndex start = TIndex.Min(Position, SelectStartPosition);
			Parent.DeleteText(Position, SelectStartPosition);

			Position = start;
			SelectStartPosition = start;
		}

		/// <summary>
		/// Flickers the curser when inactive, event is raised by <see cref="caretBlinkingTimer"/>.
		/// </summary>
		private void CaretFlicker_Event(object? sender, ElapsedEventArgs e)
		{
			caretLine.VirtualizedObject.Dispatcher.Invoke(() =>
			{
				if (caretLine.VirtualizedObject.Visibility == Visibility.Visible)
				{
					caretLine.VirtualizedObject.Visibility = Visibility.Hidden;
				}
				else
				{
					caretLine.VirtualizedObject.Visibility = Visibility.Visible;
				}
			});
		}
	}
}
