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

namespace TEditBoxWPF.Objects
{
	/// <summary>
	/// Represents a caret inside a <see cref="TEditBox"/> which responds
	/// to user input.
	/// </summary>
	public class TCaret
	{
		public TEditBox Parent { get; }

		public TIndex Position
		{
			get => caretLine.Position;
			set
			{
				if (caretLine.Position != value)
				{
					caretLine.Position = value;
					caretLine.VirtualisedObject.Visibility = Visibility.Visible;

					caretBlinkingTimer.Reset();
				}
			}
		}

		internal VirtualisedTextObject<Rectangle> caretLine;
		private Timer caretBlinkingTimer = new()
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
			caretLine = new VirtualisedTextObject<Rectangle>(
				parent: parent,
				line: parent.Lines.First(),
				virtualisationPanel: parent.TextDisplay,
				control: line);

			caretBlinkingTimer.Elapsed += CaretFlicker_Event;
			caretBlinkingTimer.Start();
		}

		/// <summary>
		/// Moves the caret by the specified character offset, the caret can move on to the next or previous lines.
		/// </summary>
		/// <param name="charOffset">The number of characters to offset the caret by.</param>
		public void MoveChar(int charOffset)
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

				string nextLineText = Parent.Lines[newPosition.Line].Text;

				newPosition.Character = 0;
			}

			Position = newPosition;
		}

		/// <summary>
		/// Moves the caret by the specified line offset.
		/// </summary>
		/// <param name="charOffset">The number of lines to offset the caret by.</param>
		public void MoveLine(int lineOffset)
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
		}

		/// <summary>
		/// Flickers the curser when inactive, event is raised by <see cref="caretBlinkingTimer"/>.
		/// </summary>
		private void CaretFlicker_Event(object? sender, ElapsedEventArgs e)
		{
			caretLine.VirtualisedObject.Dispatcher.Invoke(() =>
			{
				if (caretLine.VirtualisedObject.Visibility == Visibility.Visible)
				{
					caretLine.VirtualisedObject.Visibility = Visibility.Hidden;
				}
				else
				{
					caretLine.VirtualisedObject.Visibility = Visibility.Visible;
				}
			});
		}
	}
}
