using CustomTextBoxComponent.Textbox.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Globalization;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace TEditBoxWPF.Converters
{
	/// <summary>
	/// Converter for converting tab width in a textblock, <see cref="TextBlock.TextEffects"/> to
	/// a 4 space width instead of an 8 space width. Textblocks render tabs as 8 spaces by default and
	/// can only be adjusted by text effects.
	/// </summary>
	internal class TextTabWidthConverter : IValueConverter
	{
		internal TEditBox parent;

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			string text = (string)value;

			// Contains a collection of text effects which changes the tab spacing from 8 to 4.
			TextEffectCollection collection = new();

			int tabWidth = parent.measurer.MeasuringOptions.TabSize;

			// The tab positions are needed in order to select a certain segment of
			// text which is between tab characters. E.g.
			// The text between: "one \t two three four \t five".
			// The effect should only apply to "two, three four".
			List<int> tabPositions = new();
			int nextIndex =  text.IndexOf('\t');

			while (nextIndex != -1)
			{
				tabPositions.Add(nextIndex);

				nextIndex = text.IndexOf('\t', nextIndex + 1);
			}

			for (int i = 0; i < tabPositions.Count; i++)
			{
				int tabPosition = tabPositions[i];

				// Negate horizontal position of text after tabs, so the accurate 4 tab width can be calculated from scratch.
				string currentTextSegment = text[0..(tabPosition + 1)];
				double total = -parent.measurer.MeasureTextSize(currentTextSegment, false).Width;

				// Adjust the segment to the correct position with the adjusted tab width..
				total += parent.measurer.MeasureTextSize(currentTextSegment, true).Width;

				// Round up to the next tab segment.
				double b = Math.Ceiling((double)tabPosition / tabWidth) * tabWidth;
				var e = (((int)b) - (tabPosition)) + i;

				if (e == 0)
				{
					e = tabWidth;
				}

				string remainingTabWidthText = new string(Enumerable.Repeat(' ', Math.Max(tabWidth - e, 0)).ToArray());
				total -= parent.measurer.MeasureTextSize(remainingTabWidthText, false).Width;

				// Only affect the text from the tab character to the next tab character
				// or until the end of the text if it's the last character.
				int count;

				if (i < tabPositions.Count - 1)
				{
					count = tabPositions[i + 1] - tabPosition;
				}
				else
				{
					count = text.Length - tabPosition;
				}

				TextEffect adjustment = new()
				{
					Transform = new TranslateTransform()
					{
						X = total
					},
					PositionStart = tabPosition,
					PositionCount = count
				};

				collection.Add(adjustment);				
			}

			return collection;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
