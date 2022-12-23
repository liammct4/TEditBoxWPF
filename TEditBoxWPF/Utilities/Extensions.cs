using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;

namespace TEditBoxWPF.Utilities
{
	/// <summary>
	/// Provides generic utility extension methods for a variety of objects.
	/// </summary>
	internal static class Extensions
	{
		/// <summary>
		/// Finds the first element of the specified type <typeparamref name="T"/>
		/// in the tree of the <paramref name="element"/> provided.
		/// </summary>
		/// <param name="element">The control to search.</param>
		/// <returns>The first occurance of <typeparamref name="T"/> in the visual tree of <paramref name="element"/>.</returns>
		public static T GetDescendantByType<T>(this Visual element) where T : Visual
		{
			// Taken from: https://social.msdn.microsoft.com/Forums/vstudio/en-US/693abf03-5042-4ab6-86e8-76aa24a496e9/finding-all-controls-of-a-given-type-across-a-tabcontrol?forum=wpf
			if (element == null)
			{
				return null;
			}
			if (element.GetType() == typeof(T))
			{
				return (T)element;
			}
			Visual foundElement = null;
			if (element is FrameworkElement)
			{
				(element as FrameworkElement).ApplyTemplate();
			}
			for (int i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
			{
				Visual visual = VisualTreeHelper.GetChild(element, i) as Visual;
				foundElement = GetDescendantByType<T>(visual);
				if (foundElement != null)
				{
					break;
				}
			}
			return (T)foundElement;
		}
	}
}
