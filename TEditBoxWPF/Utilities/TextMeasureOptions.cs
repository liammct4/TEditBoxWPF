using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomTextBoxComponent.Textbox.Utilities
{
	/// <summary>
	/// Contains options for a <see cref="TextMeasurer"/>.
	/// </summary>
	public class TextMeasureOptions
	{
		/// <summary>
		/// Returns a new default set of text measuring options which uses font family Segoe UI and font size 12
		/// </summary>
		public static TextMeasureOptions Default => new()
		{
			FontFamily = "Segoe UI",
			FontSize = 12,
			TabSize = 8
		};

		/// <summary>
		/// The name of the font family which will be used to measure text.
		/// </summary>
		public string FontFamily { get; set; }
		/// <summary>
		/// The size of the font which will be used to measure text.
		/// </summary>
		public double FontSize { get; set; }
		/// <summary>
		/// The space-based width of a tab character.
		/// </summary>
		public int TabSize { get; set; }
	}
}
