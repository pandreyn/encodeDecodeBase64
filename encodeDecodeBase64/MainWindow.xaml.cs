using System;
using System.Windows;
using System.IO;
using System.Windows.Forms;
using System.Management.Automation;
using System.Management;
using System.ServiceProcess;
using System.Threading.Tasks;
using System.Management.Automation.Runspaces;
using System.Linq;
using System.Deployment.Application;
using System.Security.Principal;
using System.Windows.Controls;
using System.Windows.Input;

namespace encodeDecodeBase64
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private AppSettings _settings = new AppSettings();
		private CustomControlViewModel _customControlViewModel = new CustomControlViewModel();
		private int _noOfErrorsOnScreen = 0;

		public MainWindow()
		{
			InitializeComponent();
			try
			{
				if (ApplicationDeployment.IsNetworkDeployed)
				{
					this.Title = String.Format("{0} - v{1}", this.Title, ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString(4));
				}
			}
			catch (Exception exeption)
			{
				ConsoleTxt.AppendText(exeption.Message);
				ConsoleTxt.AppendText(Environment.NewLine);
			}

			this.DataContext = _customControlViewModel.dt;
			SettingsGrig.DataContext = _settings;
		}

		private void Validation_Error(object sender, ValidationErrorEventArgs e)
		{
			if (e.Action == ValidationErrorEventAction.Added)
				_noOfErrorsOnScreen++;
			else
				_noOfErrorsOnScreen--;

			UpdateExpanderStyle();
		}

		private void UpdateExpanderStyle()
		{
			if (_noOfErrorsOnScreen > 0)
			{
				SettingsExpd.BorderBrush = System.Windows.Media.Brushes.Red;
			}
			else
			{
				SettingsExpd.BorderBrush = System.Windows.Media.Brushes.DarkGray;
			}
		}

		private void Window_Closed(object sender, EventArgs e)
		{
			_settings.SaveSettings();
		}

		private void LoadFilesBtn_Click(object sender, RoutedEventArgs e)
		{
			LoadFiles();
		}

		private void LoadFiles()
		{
			using (var fbd = new FolderBrowserDialog())
			{
				var lastPath = Utils.GetLastPath();
				if (!String.IsNullOrEmpty(lastPath)) {
					fbd.SelectedPath = lastPath;
				}

				DialogResult result = fbd.ShowDialog();

				if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
				{
					if (lastPath != fbd.SelectedPath)
					{
						Utils.SetLastPath(fbd.SelectedPath);
					}
					string[] files = Directory.GetFiles(fbd.SelectedPath, "*.js");
					
					_customControlViewModel = new CustomControlViewModel();
					_customControlViewModel.LoadCustomControls(files);
					this.DataContext = _customControlViewModel;
				}
			}
		}

		private void OnProgress(IProgress<TaskAsyncProgress> progress, string text)
		{
			if (progress != null)
			{
				var args = new TaskAsyncProgress();
				args.Text = text;

				progress.Report(args);
			}
		}

		private async void UploadCommand_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			if (!_customControlViewModel.IsEmpty()) _customControlViewModel.ReloadContent();
			ConsoleTxt.Clear();
			ConsoleTxt.AppendText("Start.");
			ConsoleTxt.AppendText(Environment.NewLine);
			ConsoleTxt.AppendText("Connecting to server " + Utils.GetServerNameFull());
			ConsoleTxt.AppendText(Environment.NewLine);

			UploadBtn.IsEnabled = false;
			var progress = new Progress<TaskAsyncProgress>();
			progress.ProgressChanged += (p, s) =>
			{
				ConsoleTxt.AppendText(s.Text);
				ConsoleTxt.AppendText(Environment.NewLine);
			};

			var chk = await CheckTrustedHosts(progress);
			if (chk)
			{
				var psh = await RunPsShell(progress);
				if (psh)
				{
					var restart = await RestartIis(progress);
				}
			}
			
			ConsoleTxt.AppendText("OK. Update completed!");
			ConsoleTxt.AppendText(Environment.NewLine);
			UploadBtn.IsEnabled = true;
		}

		private async Task<Boolean> RestartIis(IProgress<TaskAsyncProgress> progress)
		{
			try
			{
				OnProgress(progress, "Restarting IIS...");
				await Task.Run(() =>
				{
					var serverName = Utils.GetServerNameFull();

					ConnectionOptions options = new ConnectionOptions();
					options.Password = Utils.GetPassword();
					options.Username = Utils.GetUserNameFull();
					options.Impersonation = ImpersonationLevel.Impersonate;

					var remoteComputer = "\\\\" + serverName + "\\root\\cimv2";
					ManagementScope scope = new ManagementScope(remoteComputer, options);
					scope.Connect();

					ServiceController sc = new ServiceController("IISADMIN", serverName);

					OnProgress(progress, "IIS Status = " + sc.Status);
					OnProgress(progress, "Stopping " + sc.DisplayName);
					sc.Stop();
					while (sc.Status != ServiceControllerStatus.Stopped)
					{
						Task.Delay(TimeSpan.FromSeconds(1)).Wait();
						sc.Refresh();
						OnProgress(progress, "Waiting to stop... ");
					}
					OnProgress(progress, "IIS Stopped. Status = " + sc.Status);
					OnProgress(progress, "IIS starting...");
					sc.Start();
					while (sc.Status == ServiceControllerStatus.Stopped)
					{
						Task.Delay(TimeSpan.FromSeconds(1)).Wait();
						sc.Refresh();
						OnProgress(progress, "Waiting to start... ");
					}
					OnProgress(progress, "IIS Started. Status = " + sc.Status);
				});

				return true;
			}
			catch (Exception exeption)
			{
				ConsoleTxt.AppendText(exeption.Message);
				ConsoleTxt.AppendText(Environment.NewLine);
				return false;
			}
		}

		private async Task<Boolean> RunPsShell(IProgress<TaskAsyncProgress> progress)
		{
			try
			{
				OnProgress(progress, "Updating control in sql database...");
				await Task.Run(() =>
				{
					PSCredential credential = new PSCredential(Utils.GetUserNameFull(), Utils.GetSecuredPassword());

					WSManConnectionInfo connectionInfo = new WSManConnectionInfo(new Uri("http://" + Utils.GetServerNameFull() + ":5985/wsman"), "http://schemas.microsoft.com/powershell/Microsoft.PowerShell", credential);
					connectionInfo.AuthenticationMechanism = AuthenticationMechanism.Default;

					Runspace runspace = RunspaceFactory.CreateRunspace(connectionInfo);
					runspace.Open();

					using (PowerShell ps = PowerShell.Create())
					{
						string orgName = Utils.GetOrgName();						

						ps.Runspace = runspace;
						Pipeline pipeline = runspace.CreatePipeline();

						foreach (var control in _customControlViewModel.dt.Where(x => x.shouldLoad == true))
						{
							OnProgress(progress, "Updating file " + control.Name + "...");
							string queryString =
								"UPDATE [" + orgName + "_MSCRM].[dbo].[WebResourceBase] SET [Content] = '" + Base64Utils.Base64Encode(control.Content) +
								"' WHERE[Name] like '%" + control.Name + "%'";
							pipeline.Commands.AddScript("Invoke-Sqlcmd -Query \"" + queryString + "\" -Verbose");

						}

						var results = pipeline.Invoke();

						foreach (var item in results)
						{
							OnProgress(progress, String.Format("Output: {0}", item));
						}

					}
					runspace.Close();

					OnProgress(progress, "Updated successfully.");
				});

				return true;
			}
			catch (Exception exeption)
			{
				OnProgress(progress, exeption.Message);
				return false;
			}
		}

		private async Task<Boolean> UpdateTrustedHosts(IProgress<TaskAsyncProgress> progress, string trustedHosts)
		{
			try
			{
				OnProgress(progress, "Updating TrustedHosts...");
				bool isElevated;
				WindowsIdentity identity = WindowsIdentity.GetCurrent();
				WindowsPrincipal principal = new WindowsPrincipal(identity);
				isElevated = principal.IsInRole(WindowsBuiltInRole.Administrator);

				if (isElevated)
				{
					using (PowerShell PowerShellInstance = PowerShell.Create())
					{
						await Task.Run(() =>
						{
							var value = trustedHosts == "" ? Utils.GetServerNameFull() : (trustedHosts + ", " + Utils.GetServerNameFull());
							var script = "set-item -path WSMan:\\localhost\\Client\\TrustedHosts -Value \"" + value + "\" -Force";
							PowerShellInstance.AddScript(script);
							PowerShellInstance.Invoke();
						});
						
						if (PowerShellInstance.Streams.Error.Count > 0)
						{
							foreach (var error in PowerShellInstance.Streams.Error)
							{
								OnProgress(progress, error.ToString());
								return false;
							}
						}

						var result = await CheckTrustedHosts(progress);
						return result;
					}
				}
				else
				{
					OnProgress(progress, "You should restart the program with administratrion privileges to be able to add server to TrustedHosts");
					return false;
				}

				
			}
			catch (Exception exeption)
			{
				ConsoleTxt.AppendText(exeption.Message);
				ConsoleTxt.AppendText(Environment.NewLine);
				return false;
			}
		}

		private async Task<Boolean> CheckTrustedHosts(IProgress<TaskAsyncProgress> progress)
		{
			try
			{
				OnProgress(progress, "Checking the server in TrustedHosts...");
				using (PowerShell PowerShellInstance = PowerShell.Create())
				{
					var script = @"get-item wsman:\localhost\Client\TrustedHosts";
					PowerShellInstance.AddScript(script);
					var PSOutput = PowerShellInstance.Invoke();
					
					var trustedHosts = PSOutput[0].Properties["Value"].Value.ToString();
					var server = Utils.GetServerNameFull();
					if (trustedHosts.Contains(server))
					{
						OnProgress(progress, "OK. The Server is in TrustedHosts.");
					} 
					else
					{
						OnProgress(progress, "ERROR. The Server is NOT in TrustedHosts!");
						var result = await UpdateTrustedHosts(progress, trustedHosts);
						return result;
					}

					if (PowerShellInstance.Streams.Error.Count > 0)
					{
						foreach (var error in PowerShellInstance.Streams.Error)
						{
							OnProgress(progress, error.ToString());
							return false;
						}				
					}
					return true;
				}
			}
			catch (Exception exeption)
			{
				OnProgress(progress, exeption.Message);
				return false;
			}
		}

		private void btnSave_Click(object sender, RoutedEventArgs e)
		{
			_settings.SaveSettings();
		}

		private void UploadCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			var canExecute = !_settings.IsNotFull() && _customControlViewModel.HasSelectedRecords();
			e.CanExecute = canExecute && (_noOfErrorsOnScreen == 0);
			e.Handled = true;
		}
	}
}
