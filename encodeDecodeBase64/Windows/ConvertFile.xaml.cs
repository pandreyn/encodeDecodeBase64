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
using System.Windows.Shapes;
using System.IO;
using Microsoft.Win32;

namespace encodeDecodeBase64.Windows
{
	/// <summary>
	/// Interaction logic for ConvertFile.xaml
	/// </summary>
	public partial class ConvertFile : Window
	{
		public ConvertFile()
		{
			InitializeComponent();
		}

		private void EncodeBtn_Click(object sender, RoutedEventArgs e)
		{
			decodeTxt.Text = Base64Utils.Base64Encode(encodeTxt.Text);
		}

		private void DecodeBtn_Click(object sender, RoutedEventArgs e)
		{
			encodeTxt.Text = Base64Utils.Base64Decode(decodeTxt.Text);
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
			this.Close();
		}
	}
}
