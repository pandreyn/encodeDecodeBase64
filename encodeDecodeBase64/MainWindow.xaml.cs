using System;
using System.Windows;
using encodeDecodeBase64.Windows;
using System.IO;
using System.Windows.Forms;

namespace encodeDecodeBase64
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public CustomControlViewModel CustomControlViewModel { get; set; }
		public MainWindow()
		{
			InitializeComponent();
		}
		
		private void MenuItem_Click(object sender, RoutedEventArgs e)
		{
			System.Windows.Application.Current.Shutdown();
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

		private void MenuItem_Click_2(object sender, RoutedEventArgs e)
		{
			Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
			if (openFileDialog.ShowDialog() == true)
			{
				
			}
				//encodeTxt.Text = File.ReadAllText(openFileDialog.FileName);
		}

		private void MenuItem_Click_3(object sender, RoutedEventArgs e)
		{
			LoadFiles();
		}

		private void LoadFilesBtn_Click(object sender, RoutedEventArgs e)
		{
			LoadFiles();
		}

		private void LoadFiles()
		{
			using (var fbd = new FolderBrowserDialog())
			{
				System.Windows.Forms.DialogResult result = fbd.ShowDialog();

				if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
				{
					string[] files = Directory.GetFiles(fbd.SelectedPath, "*.js");
					CustomControlViewModel = new CustomControlViewModel();
					CustomControlViewModel.LoadCustomControls(files);
					this.DataContext = CustomControlViewModel;
				}
			}
		}

		private void UploadBtn_Click(object sender, RoutedEventArgs e)
		{
			UpdateServerWindow upd = new UpdateServerWindow(CustomControlViewModel);
			upd.Show();
		}
	}
}
