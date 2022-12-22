﻿using System;
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

namespace TextBoxTester
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
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
	}
}
