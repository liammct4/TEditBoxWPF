using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using TEditBoxWPF.LineStructure;

namespace TEditBoxWPF
{
	public partial class TEditBox : UserControl
	{
		public string Text
		{
			get => _text;
			set
			{
				_text = value;
				Lines = _text.Split(Environment.NewLine).ToList();
			}
		}
		private string _text;
		internal List<string> Lines
		{
			get => _lines;
			set
			{
				_lines = value;
				TextDisplay.ItemsSource = value;
			}
		}
		internal List<string> _lines;

		public TEditBox()
		{
			InitializeComponent();
		}
	}
}