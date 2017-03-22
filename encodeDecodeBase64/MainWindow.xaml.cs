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

			this.DataContext = _customControlViewModel;
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
				SettingsExpd.BorderBrush = System.Windows.Media.Brushes.Transparent;
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
					_customControlViewModel.LoadCustomControls(files);
				}
			}
		}

		private void UploadCommand_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			if (!_customControlViewModel.IsEmpty()) _customControlViewModel.ReloadContent();
			ConsoleTxt.Clear();
			ConsoleTxt.AppendText("Start.");
			ConsoleTxt.AppendText(Environment.NewLine);
			ConsoleTxt.AppendText("Connecting to server " + Utils.GetServerNameFull());
			ConsoleTxt.AppendText(Environment.NewLine);

			if (CheckTrustedHosts())
			{
				if (RunPsShell())
				{
					RestartIis();
				}
			}
		}

		private void RestartIis()
		{
			try
			{
				var serverName = Utils.GetServerNameFull();

				ConnectionOptions options = new ConnectionOptions();
				options.Password = Utils.GetPassword();
				options.Username = Utils.GetUserNameFull();
				options.Impersonation = ImpersonationLevel.Impersonate;

				// Make a connection to a remote computer. 
				// Replace the "FullComputerName" section of the
				// string "\\\\FullComputerName\\root\\cimv2" with
				// the full computer name or IP address of the 
				// remote computer.
				var remoteComputer = "\\\\" + serverName + "\\root\\cimv2";
				ManagementScope scope =
					new ManagementScope(remoteComputer, options);
				scope.Connect();

				ServiceController sc = new ServiceController("IISADMIN", serverName);

				ConsoleTxt.AppendText("Status = " + sc.Status);
				ConsoleTxt.AppendText(Environment.NewLine);
				ConsoleTxt.AppendText("Stopping " + sc.DisplayName);
				ConsoleTxt.AppendText(Environment.NewLine);
				sc.Stop();
				while (sc.Status != ServiceControllerStatus.Stopped)
				{
					Task.Delay(TimeSpan.FromSeconds(1)).Wait();
					sc.Refresh();
					ConsoleTxt.AppendText("Waiting to stop... ");
					ConsoleTxt.AppendText(Environment.NewLine);
				}
				ConsoleTxt.AppendText("Status = " + sc.Status);
				ConsoleTxt.AppendText(Environment.NewLine);
				sc.Start();
				while (sc.Status == ServiceControllerStatus.Stopped)
				{
					Task.Delay(TimeSpan.FromSeconds(1)).Wait();
					sc.Refresh();
					ConsoleTxt.AppendText("Waiting to start... ");
					ConsoleTxt.AppendText(Environment.NewLine);
				}
				ConsoleTxt.AppendText("Status = " + sc.Status);
				ConsoleTxt.AppendText(Environment.NewLine);
				ConsoleTxt.AppendText(Environment.NewLine);
				ConsoleTxt.AppendText(Environment.NewLine);
			}
			catch (Exception exeption)
			{
				ConsoleTxt.AppendText(exeption.Message);
				ConsoleTxt.AppendText(Environment.NewLine);
			}
		}

		private Boolean RunPsShell()
		{
			try
			{				
				PSCredential credential = new PSCredential(Utils.GetUserNameFull(), Utils.GetSecuredPassword());

				WSManConnectionInfo connectionInfo = new WSManConnectionInfo(new Uri("http://" + Utils.GetServerNameFull() + ":5985/wsman"), "http://schemas.microsoft.com/powershell/Microsoft.PowerShell", credential);
				connectionInfo.AuthenticationMechanism = AuthenticationMechanism.Default;

				Runspace runspace = RunspaceFactory.CreateRunspace(connectionInfo);
				runspace.Open();

				using (PowerShell ps = PowerShell.Create())
				{
					string orgName = Utils.GetOrgName();

					ConsoleTxt.AppendText("Updating control in sql database...");
					ConsoleTxt.AppendText(Environment.NewLine);

					ps.Runspace = runspace;
					Pipeline pipeline = runspace.CreatePipeline();

					foreach (var control in _customControlViewModel.dt.Where(x => x.shouldLoad == true))
					{
						ConsoleTxt.AppendText("Updating file " + control.Name + "...");
						ConsoleTxt.AppendText(Environment.NewLine);
						string queryString =
							"UPDATE [" + orgName + "_MSCRM].[dbo].[WebResourceBase] SET [Content] = '" + Base64Utils.Base64Encode(control.Content) +
							"' WHERE[Name] like '%" + control.Name + "%'";
						pipeline.Commands.AddScript("Invoke-Sqlcmd -Query \"" + queryString + "\" -Verbose");

					}

					var results = pipeline.Invoke();

					foreach (var item in results)
					{
						ConsoleTxt.AppendText(String.Format("Output: {0}", item));
						ConsoleTxt.AppendText(Environment.NewLine);
					}

				}
				runspace.Close();

				ConsoleTxt.AppendText("Updated successfully.");
				ConsoleTxt.AppendText(Environment.NewLine);

				return true;
			}
			catch (Exception exeption)
			{
				ConsoleTxt.AppendText(exeption.Message);
				ConsoleTxt.AppendText(Environment.NewLine);
				return false;
			}
		}

		private Boolean UpdateTrustedNosts(string trustedHosts)
		{
			try
			{
				bool isElevated;
				WindowsIdentity identity = WindowsIdentity.GetCurrent();
				WindowsPrincipal principal = new WindowsPrincipal(identity);
				isElevated = principal.IsInRole(WindowsBuiltInRole.Administrator);

				if (isElevated)
				{
					using (PowerShell PowerShellInstance = PowerShell.Create())
					{
						var value = trustedHosts == "" ? Utils.GetServerNameFull() : (trustedHosts + ", " + Utils.GetServerNameFull());
						var script = "set-item -path WSMan:\\localhost\\Client\\TrustedHosts -Value \"" + value + "\" -Force";
						PowerShellInstance.AddScript(script);
						PowerShellInstance.Invoke();

						if (PowerShellInstance.Streams.Error.Count > 0)
						{
							foreach (var error in PowerShellInstance.Streams.Error)
							{
								ConsoleTxt.AppendText(error.ToString());
								ConsoleTxt.AppendText(Environment.NewLine);
								return false;
							}
						}

						return CheckTrustedHosts();
					}
				}
				else
				{
					ConsoleTxt.AppendText("You should restart the program with administratrion privileges to be able to add server to TrustedHosts");
					ConsoleTxt.AppendText(Environment.NewLine);
					ConsoleTxt.AppendText(Environment.NewLine);
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

		private Boolean CheckTrustedHosts()
		{
			try
			{
				using (PowerShell PowerShellInstance = PowerShell.Create())
				{
					var script = @"get-item wsman:\localhost\Client\TrustedHosts";
					PowerShellInstance.AddScript(script);
					var PSOutput = PowerShellInstance.Invoke();

					ConsoleTxt.AppendText("Checking the server in TrustedHosts...");
					ConsoleTxt.AppendText(Environment.NewLine);
					var trustedHosts = PSOutput[0].Properties["Value"].Value.ToString();
					var server = Utils.GetServerNameFull();
					if (trustedHosts.Contains(server))
					{
						ConsoleTxt.AppendText("OK. The Server is in TrustedHosts.");
						ConsoleTxt.AppendText(Environment.NewLine);
						ConsoleTxt.AppendText(Environment.NewLine);
					} 
					else
					{
						ConsoleTxt.AppendText("ERROR. The Server is NOT in TrustedHosts!");
						ConsoleTxt.AppendText(Environment.NewLine);
						ConsoleTxt.AppendText(Environment.NewLine);
						return UpdateTrustedNosts(trustedHosts);
					}

					if (PowerShellInstance.Streams.Error.Count > 0)
					{
						foreach (var error in PowerShellInstance.Streams.Error)
						{
							ConsoleTxt.AppendText(error.ToString());
							ConsoleTxt.AppendText(Environment.NewLine);
							return false;
						}				
					}
					return true;
				}
			}
			catch (Exception exeption)
			{
				ConsoleTxt.AppendText(exeption.Message);
				ConsoleTxt.AppendText(Environment.NewLine);
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
