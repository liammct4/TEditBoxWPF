using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using TEditBoxWPF.LineStructure;

namespace TEditBoxWPF.Converters
{
	/// <summary>
	/// Converts a list of text lines to a list of incrementing numbers.
	/// </summary>
	public class LineCountConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is List<TLine> lines)
			{
				return Enumerable.Range(1, lines.Count);
			}
			else
			{
				return DependencyProperty.UnsetValue;
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
