using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using TEditBoxWPF.Utilities;

namespace TEditBoxWPF.Controls.ScrollableItemsControl
{
	/// <summary>
	/// Provides an items control which has support for scrolling and scroll options.
	/// 
	/// The items panel for this items control needs to be <see cref="ScrollingVirtualizationPanel"/>.
	/// </summary>
	internal class ScrollableItemsControl : ItemsControl
	{
		private ScrollingVirtualizationPanel panel;
		private bool panelIsPresent;

		public ScrollableItemsControl()
		{
			Loaded += ScrollableItemsControl_Loaded;
		}

		/// <summary>
		/// Fetches the <see cref="ScrollingVirtualizationPanel"/> located in this items control.
		/// </summary>
		private void ScrollableItemsControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
		{
			panel = this.GetDescendantByType<ScrollingVirtualizationPanel>();
			panelIsPresent = panel is not null;
		}

		/// <summary>
		/// Scrolls the item at index <paramref name="itemIndex"/> into view.
		/// </summary>
		/// <param name="itemIndex">The index of the item.</param>
		/// <exception cref="InvalidOperationException">If the panel has not been initialised.</exception>
		public void ScrollIntoView(int itemIndex)
		{
			if (panelIsPresent)
			{
				panel.ScrollIntoView(itemIndex);
			}
			else
			{
				throw new InvalidOperationException($"The {typeof(ScrollingVirtualizationPanel)} panel for this {typeof(ScrollableItemsControl)} does not exist.");
			}
		}

		/// <summary>
		/// Scrolls the panel to the item <paramref name="item"/> into view.
		/// </summary>
		/// <param name="item">The item to scroll to.</param>
		/// <exception cref="InvalidOperationException">If the panel has not been initialised.</exception>
		public void ScrollIntoView(object item)
		{
			int index = Items.IndexOf(item);

			if (index != -1)
			{
				ScrollIntoView(index);
			}
		}
	}
}
