using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Controls;

namespace CustomTextBoxComponent.Textbox.Utilities
{
	/// <summary>
	/// Measures WPF text in pixels using the specified <see cref="TextMeasureOptions"/> provided.
	/// </summary>
	public class TextMeasurer
	{
		/// <summary>
		/// Specifies how the text measurer will measure text provided.
		/// </summary>
		public TextMeasureOptions MeasuringOptions
		{
			get => _measuringOptions;
			set
			{
				measuringTextBlock.FontFamily = new FontFamily(value.FontFamily);
				measuringTextBlock.FontSize = value.FontSize;

				_measuringOptions = value;
			}
		}
		private TextMeasureOptions _measuringOptions;

		private readonly TextBlock measuringTextBlock = new();

		/// <summary>
		/// Creates a text measurer with a default setting of <see cref="TextMeasureOptions.Default"/>
		/// </summary>
		public TextMeasurer()
		{
			MeasuringOptions = TextMeasureOptions.Default;
		}

		/// <summary>
		/// Creates a text measurer with the options using the provided font family and font size.
		/// </summary>
		public TextMeasurer(string fontFamily, double emSize)
		{
			MeasuringOptions = new TextMeasureOptions()
			{
				FontFamily = fontFamily,
				FontSize = emSize
			};
		}

		/// <summary>
		/// Measures a string of provided text in pixels.
		/// </summary>
		/// <param name="text">The text to measure.</param>
		/// <returns>The size of the text in pixels.</returns>
		public Size MeasureTextSize(string text, bool useCustomFormatting)
		{
			if (useCustomFormatting)
			{
				string tab = new(Enumerable.Repeat(' ', MeasuringOptions.TabSize).ToArray());
				text = text.Replace("\t", tab);
			}

			measuringTextBlock.Text = text;

			measuringTextBlock.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
			measuringTextBlock.Arrange(new Rect(measuringTextBlock.DesiredSize));

			return new Size(measuringTextBlock.ActualWidth, measuringTextBlock.ActualHeight);
		}
	}
}
