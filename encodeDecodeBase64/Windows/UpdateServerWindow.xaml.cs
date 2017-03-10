using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Management;
using System.Security.Principal;
using System.ServiceProcess;
using System.Threading.Tasks;
using System.Windows;

namespace encodeDecodeBase64.Windows
{
	/// <summary>
	/// Interaction logic for UpdateServerWindow.xaml
	/// </summary>
	public partial class UpdateServerWindow : Window
	{
		private string base64content;
		public UpdateServerWindow(string base64content)
		{
			InitializeComponent();
			this.base64content = base64content;
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			ConsoleTxt.AppendText("Start.");
			ConsoleTxt.AppendText(Environment.NewLine);
			ConsoleTxt.AppendText("Connecting to server " + Utils.GetServerFullName());
			ConsoleTxt.AppendText(Environment.NewLine);

			ConsoleTxt.AppendText("Before impersonation: " + WindowsIdentity.GetCurrent().Name + "\r\n");
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
			ConsoleTxt.AppendText("After impersonation: " + scope. + "\r\n");
			if (UpdateSql())
			{
				RestartIis();
			}
		}

		private Boolean UpdateSql()
		{
			//org1_MSCRM
			string fileName = Properties.Settings.Default["Filename"].ToString();
			string serverName = Utils.GetServerFullName();
			string orgName = Properties.Settings.Default["Orgname"].ToString();
			string connectString = "Data Source=" + serverName + 
				"; Integrated Security=SSPI;persist security info=False;Password=" + Properties.Settings.Default["Password"].ToString() + 
				";User ID = " + Utils.GetFullUserName();

			try
			{
				//SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(connectString);
				//string connectionString = ConfigurationManager.ConnectionStrings["LocalDB"].ConnectionString;

				using (SqlConnection connection = new SqlConnection(connectString))
				using (SqlCommand command = connection.CreateCommand())
				{
					//command.CommandText = "INSERT INTO Student (LastName, FirstName, Address, City)  VALUES(@ln, @fn, @add, @cit)";
					command.CommandText = "UPDATE [" + orgName + "_MSCRM].[dbo].[WebResourceBase] SET [Content] = @b64 WHERE[Name] = @fn'";

					command.Parameters.AddWithValue("@b64", this.base64content);
					command.Parameters.AddWithValue("@fn", fileName);

					connection.Open();

					command.ExecuteNonQuery();

					connection.Close();
				}

				return true;
			}
			catch (Exception exeption)
			{
				ConsoleTxt.AppendText(exeption.Message);
				ConsoleTxt.AppendText(Environment.NewLine);
				return false;
			}

		}

		private void RestartIis()
		{
			try
			{
				var serverName = Utils.GetServerFullName();

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
	}
}
