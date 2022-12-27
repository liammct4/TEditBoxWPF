using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Shapes;
using TEditBoxWPF.LineStructure;
using TEditBoxWPF.TextStructure;

namespace TEditBoxWPF.Objects
{
	/// <summary>
	/// Represents a caret inside a <see cref="TEditBox"/> which responds
	/// to user input.
	/// </summary>
	public class TCaret
	{
		public TEditBox Parent { get; }
		internal VirtualisedTextObject<Rectangle> CaretLine;

		public TIndex Position
		{
			get => CaretLine.Position;
			set => CaretLine.Position = value;
		}

		public TCaret(TEditBox parent)
		{
			Parent = parent;

			Rectangle line = new()
			{
				Width = 1,
				Fill = new SolidColorBrush(Colors.Black)
			};
			CaretLine = new VirtualisedTextObject<Rectangle>(
				parent: parent,
				line: parent.Lines.First(),
				virtualisationPanel: parent.TextDisplay,
				control: line);
		}

		/// <summary>
		/// Moves the caret by the specified character offset, the caret can move on to the next or previous lines.
		/// </summary>
		/// <param name="charOffset">The number of characters to offset the caret by.</param>
		public void MoveChar(int charOffset)
		{
			TLine currentLine = CaretLine.Line;
			TIndex currentPosition = CaretLine.Position;
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

				if (newPosition.Character <= nextLineText.Length)
				{
					newPosition.Character = 0;
				}
				else
				{
					return;
				}
			}

			Position = newPosition;
		}

		/// <summary>
		/// Moves the caret by the specified line offset.
		/// </summary>
		/// <param name="charOffset">The number of lines to offset the caret by.</param>
		public void MoveLine(int lineOffset)
		{
			TIndex currentPosition = CaretLine.Position;

			currentPosition = currentPosition.OffsetLine(lineOffset);

			if (currentPosition.Line < 0 || currentPosition.Line >= Parent.Lines.Count)
			{
				return;
			}

			string nextLineText = Parent.Lines[currentPosition.Line].Text;

			currentPosition.Character = Math.Min(nextLineText.Length, currentPosition.Character);

			Position = currentPosition;
		}
	}
}
