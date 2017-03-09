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
using encodeDecodeBase64.Utils;
using Microsoft.Win32;
using System.IO;
using encodeDecodeBase64.Windows;

namespace encodeDecodeBase64
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

		private void CommonCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = true;
		}

		private void EncodeBtn_Click(object sender, RoutedEventArgs e)
		{
			decodeTxt.Text = base64Utils.Base64Encode(encodeTxt.Text);
		}

		private void DecodeBtn_Click(object sender, RoutedEventArgs e)
		{
			encodeTxt.Text = base64Utils.Base64Decode(decodeTxt.Text);
		}

		private void ClearLeftBtn_Click(object sender, RoutedEventArgs e)
		{
			encodeTxt.Text = String.Empty;
		}

		private void ClearRightBtn_Click(object sender, RoutedEventArgs e)
		{
			decodeTxt.Text = String.Empty;
		}

		private void OpenLeftBtn_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog openFileDialog = new OpenFileDialog();
			if (openFileDialog.ShowDialog() == true)
				encodeTxt.Text = File.ReadAllText(openFileDialog.FileName);
		}

		private void ClearAllBtn_Click(object sender, RoutedEventArgs e)
		{
			encodeTxt.Text = String.Empty;
			decodeTxt.Text = String.Empty;
		}

		private void CopyToClipboardBtn_Click(object sender, RoutedEventArgs e)
		{
			Clipboard.SetText(decodeTxt.Text);
		}

		private void MenuItem_Click(object sender, RoutedEventArgs e)
		{
			Application.Current.Shutdown();
		}

		private void Window_Closed(object sender, EventArgs e)
		{
			Properties.Settings.Default.Save();
		}

		private void MenuItem_Click_1(object sender, RoutedEventArgs e)
		{
			SettingsWindow win2 = new SettingsWindow();
			win2.Show();
		}
	}
}
