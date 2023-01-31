using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Globalization;
using System.Windows.Media;
using TEditBoxWPF.Utilities;
using System.Windows.Shapes;
using System.Diagnostics.Metrics;
using TEditBoxWPF.TextStructure;

namespace TEditBoxWPF.LineStructure
{
	/// <summary>
	/// Represents a line of text within a <see cref="TEditBox"/>.
	/// </summary>
	public class TLine
	{
		/// <summary>
		/// The owner parent of this line.
		/// </summary>
		public TEditBox Parent { get; set; }
		/// <summary>
		/// The text of this line, do not modify externally.
		/// </summary>
		public string Text
		{
			get => _text;
			set
			{
				_text = value;
				RefreshText();
			}
		}
		private string _text;
		/// <summary>
		/// The position of this line.
		/// </summary>
		public int Position => Parent.Lines.IndexOf(this);

		internal TLine(TEditBox parent, string text)
		{
			Parent = parent;
			Text = text;
		}

		/// <summary>
		/// Refreshes the text by setting the TextBlock text of the line to <see cref="Text"/> and also recalculates tab widths.
		/// </summary>
		internal void RefreshText()
		{
			// Done after text has changed.
			if (!Parent.IsLoaded || Parent.TextDisplay.ItemContainerGenerator.ContainerFromItem(this) is not ContentPresenter presenter)
			{
				return;
			}

			TextBlock textBlock = presenter.GetDescendantByType<TextBlock>();

			textBlock.Text = Text;
			
			textBlock.TextEffects = (TextEffectCollection)Parent.tabConverter.Convert(Text,
				typeof(string),
				null,
				CultureInfo.CurrentCulture);
		}

		/// <summary>
		/// Inserts text at a character position in the line.
		/// </summary>
		/// <param name="characterIndex">The character index to insert the text at.</param>
		/// <param name="text">The text to insert.</param>
		public void InsertText(int characterIndex, string text)
		{
			Parent.InsertText(new TIndex(Position, characterIndex), text);
		}

		/// <summary>
		/// Removes a certain amount of text in between two positions.
		/// </summary>
		public void DeleteText(int startPosition, int endPosition)
		{
			if (startPosition < 0 || startPosition > Text.Length)
			{
				throw new ArgumentOutOfRangeException(nameof(startPosition), "The start position was bigger or smaller than the line text length.");
			}

			if (endPosition < 0 || endPosition > Text.Length)
			{
				throw new ArgumentOutOfRangeException(nameof(endPosition), "The end position was bigger or smaller than the line text length.");
			}

			int smallestStart = Math.Min(startPosition, endPosition);
			int biggestEnd = Math.Max(startPosition, endPosition);

			string newText = Text.Remove(smallestStart, biggestEnd - smallestStart);
			Text = newText;

			RefreshText();
		}

		/// <summary>
		/// Retrieves the character index at a pixel offset from the left of the text.
		/// 
		/// Provides the length of the line text if the pixel position is bigger than the line text.
		/// </summary>
		/// <param name="line">The line to get the character from.</param>
		/// <param name="pixelPosition">The pixel offset of the character.</param>
		/// <returns>The character at the pixel position <paramref name="pixelPosition"/>.</returns>
		public int GetCharacterAtPixel(double pixelPosition)
		{
			if (Parent.measurer.MeasureTextSize(Text, useCustomFormatting: true).Width <= pixelPosition)
			{
				return Text.Length;
			}

			int character = 0;

			for (int i = 0; i <= Text.Length; i++)
			{
				int charIndex = Math.Min(i, Text.Length);

				string currentText = Text[0..charIndex];

				if (Parent.measurer.MeasureTextSize(currentText, useCustomFormatting: true).Width > pixelPosition)
				{
					string currentCharacter = Text[charIndex - 1].ToString();
					double charWidth = Parent.measurer.MeasureTextSize(currentCharacter, useCustomFormatting: true).Width;
					double currentTextWidth = Parent.measurer.MeasureTextSize(currentText, useCustomFormatting: true).Width;

					double threshold = currentTextWidth - (charWidth / 2);

					if (pixelPosition > threshold)
					{
						character = i;
					}
					else
					{
						character = i - 1;
					}

					break;
				}
			}

			return character;
		}
	}
}
