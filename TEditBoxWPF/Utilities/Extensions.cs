﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;
using System.Timers;

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

		/// <summary>
		/// Resets the timers total time elapsed to 0. The <see cref="Timer.Elapsed"/> event
		/// will be raised after <see cref="Timer.Interval"/> has occured after calling
		/// this method.
		/// </summary>
		public static void Reset(this Timer timer)
		{
			timer.Stop();
			timer.Start();
		}

		/// <summary>
		/// Finds the first element of the specified type <typeparamref name="T"/>
		/// in the tree above the <paramref name="element"/> provided.
		/// </summary>
		/// <param name="element">The control to search.</param>
		/// <returns>The first occurance of <typeparamref name="T"/>.</returns>
		public static T GetParentByType<T>(this DependencyObject element) where T : Visual
		{
			// Taken from: https://www.infragistics.com/community/blogs/b/blagunas/posts/find-the-parent-control-of-a-specific-type-in-wpf-and-silverlight
			DependencyObject parentObject = VisualTreeHelper.GetParent(element);

			if (parentObject == null)
			{
				return null;
			}

			if (parentObject is T parent)
			{
				return parent;
			}
			else
			{
				return GetParentByType<T>(parentObject);
			}
		}
	}
}
