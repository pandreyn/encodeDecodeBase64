using System;
using System.Management;
using System.ServiceProcess;
using System.Threading.Tasks;
using System.Windows;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Linq;

namespace encodeDecodeBase64.Windows
{
	/// <summary>
	/// Interaction logic for UpdateServerWindow.xaml
	/// </summary>
	public partial class UpdateServerWindow : Window
	{
		public CustomControlViewModel Controls { get; set; }
		public UpdateServerWindow(CustomControlViewModel controls)
		{
			InitializeComponent();
			Controls = controls;
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			ConsoleTxt.AppendText("Start.");
			ConsoleTxt.AppendText(Environment.NewLine);
			ConsoleTxt.AppendText("Connecting to server " + Utils.GetServerFullName());
			ConsoleTxt.AppendText(Environment.NewLine);

			if (RunPsShell())
			{
				RestartIis();
			}
		}

		private void RestartIis()
		{
			try
			{
				var serverName = Utils.GetServerFullName();

				ConnectionOptions options = new ConnectionOptions();
				options.Password = Properties.Settings.Default["Password"].ToString();
				options.Username = Utils.GetFullUserName();
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
					ConsoleTxt.AppendText("Waiting to stop... ");
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
				PSCredential credential = new PSCredential(Utils.GetFullUserName(), Utils.GetSecuredPassword());

				WSManConnectionInfo connectionInfo = new WSManConnectionInfo(new Uri("http://" + Utils.GetServerFullName() + ":5985/wsman"), "http://schemas.microsoft.com/powershell/Microsoft.PowerShell", credential);
				connectionInfo.AuthenticationMechanism = AuthenticationMechanism.Default;

				Runspace runspace = RunspaceFactory.CreateRunspace(connectionInfo);
				runspace.Open();

				using (PowerShell ps = PowerShell.Create())
				{
					string fileName = Properties.Settings.Default["Filename"].ToString();
					string orgName = Properties.Settings.Default["Orgname"].ToString();

					ConsoleTxt.AppendText("Updating control in sql database...");
					ConsoleTxt.AppendText(Environment.NewLine);

					ps.Runspace = runspace;
					Pipeline pipeline = runspace.CreatePipeline();

					foreach (var control in Controls.dt.Where(x => x.shouldLoad == true))
					{
						ConsoleTxt.AppendText("Updating file " + control.Name + "...");
						ConsoleTxt.AppendText(Environment.NewLine);
						string queryString = 
							"UPDATE [" + orgName + "_MSCRM].[dbo].[WebResourceBase] SET [Content] = '" + Base64Utils.Base64Encode(control.Content) + 
							"' WHERE[Name] like '%" + control.Name + "%'";
						pipeline.Commands.AddScript("Invoke-Sqlcmd -Query \""  + queryString + "\" -Verbose");

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

	}
}
