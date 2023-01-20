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

		internal void RefreshText()
		{
			// Done after text has changed.
			ContentPresenter presenter = Parent.TextDisplay.ItemContainerGenerator.ContainerFromItem(this) as ContentPresenter;
			TextBlock textBlock = presenter.GetDescendantByType<TextBlock>();

			textBlock.Text = Text;
			
			textBlock.TextEffects = (TextEffectCollection)Parent.tabConverter.Convert(Text, typeof(string), null, CultureInfo.CurrentCulture);
		}

		public void InsertText(int character, string text)
		{
			string newText = Text.Insert(character, text);
			Text = newText;

			RefreshText();
		}
	}
}
