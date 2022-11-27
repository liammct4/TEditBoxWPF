using System;
using System.Collections.Generic;
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

namespace TEditBoxWPF.Controls
{
	/// <summary>
	/// Because <see cref="ScrollViewer"/> works on a row by row basis for scrolling (with no option for pixel scrolling),
	/// the scrollviewer has to be overidden.
	/// </summary>
	public class SmoothScrollviewer : ScrollViewer
	{
		static SmoothScrollviewer()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(SmoothScrollviewer), new FrameworkPropertyMetadata(typeof(SmoothScrollviewer)));
		}

		protected override void OnMouseWheel(MouseWheelEventArgs e) => ScrollToVerticalOffset(VerticalOffset - e.Delta);
	}
}
