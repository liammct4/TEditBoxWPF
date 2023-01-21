using CustomTextBoxComponent.Textbox.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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
	/// A class containing useful data.
	/// </summary>
	internal static class HelperData
	{
		/// <summary>
		/// Represents a list of keys which need to be manually handled.
		/// </summary>
		internal static HashSet<Key> interceptKeys = new()
		{
			Key.Down,
			Key.Up,
			Key.Left,
			Key.Right,
			Key.Tab,
			Key.Back,
			Key.Enter
		};
	}
}