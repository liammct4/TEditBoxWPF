using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
		public string Text { get; internal set; }
		/// <summary>
		/// The position of this line.
		/// </summary>
		public int Position => Parent.Lines.IndexOf(this);

		internal TLine(TEditBox parent, string text)
		{
			Parent = parent;
			Text = text;
		}
	}
}
