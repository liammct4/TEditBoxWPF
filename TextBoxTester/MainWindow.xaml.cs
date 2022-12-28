using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
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
using TEditBoxWPF.Objects;
using TEditBoxWPF.TextStructure;

namespace TextBoxTester
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private VirtualizedTextObject<Rectangle> testObject;

		public MainWindow()
		{
			InitializeComponent();
		}

		private void LoadSampleText_Event(object sender, RoutedEventArgs e)
		{
			textBox.Text = TestData.SAMPLE_TEXT;
		}

		private void LoadLongSampleText_Event(object sender, RoutedEventArgs e)
		{
			textBox.Text = TestData.LONG_SAMPLE_TEXT;
		}

		private void LoadTabMeasureText_Event(object sender, RoutedEventArgs e)
		{
			textBox.Text = TestData.TAB_MEASURE_TEXT;
		}

		private void LoadFileButtonClick_Event(object sender, RoutedEventArgs e)
		{
			OpenFileDialog dialog = new()
			{
				Title = "Open text file",
				Multiselect = false,
				CheckFileExists = true
			};

			if (dialog.ShowDialog() is true)
			{
				using FileStream fs = File.OpenRead(dialog.FileName);
				using StreamReader reader = new(fs);

				textBox.Text = reader.ReadToEnd();
			}
		}

		private void UpdateButtonClick_Event(object sender, RoutedEventArgs e)
		{
			textBox.MainCaret.Position = new TIndex(int.Parse(LineBox.Text), int.Parse(CharBox.Text));
		}
	}
}
