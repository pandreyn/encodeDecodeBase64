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
using System.Windows.Shapes;

namespace encodeDecodeBase64.Windows
{
	/// <summary>
	/// Interaction logic for Settings.xaml
	/// </summary>
	public partial class SettingsWindow : Window
	{
		public SettingsWindow()
		{
			InitializeComponent();
		}

		private void btnCancel_Click(object sender, RoutedEventArgs e)
		{
			this.Close();			
		}

		private void btnOk_Click(object sender, RoutedEventArgs e)
		{
			Properties.Settings.Default.Save();
			this.Close();
		}
	}
}
