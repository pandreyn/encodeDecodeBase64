using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace encodeDecodeBase64
{
	public class AppSettings : IDataErrorInfo, INotifyPropertyChanged
	{
		private string _serverName;
		private string _orgName;
		private string _userName;
		private string _password;

		public event PropertyChangedEventHandler PropertyChanged;

		public string ServerName {
			get { return _serverName; }
			set
			{
				if (_serverName != null && _serverName != value)
				{
					Utils.SetServerName(value);
				}
				_serverName = value;
				OnPropertyChanged("ServerName");
			}
		}
		public string OrgName
		{
			get { return _orgName; }
			set
			{
				if (_orgName != null && _orgName != value)
				{
					Utils.SetOrgName(value);
				}
				_orgName = value;
				OnPropertyChanged("OrgName");
			}
		}
		public string UserName
		{
			get { return _userName; }
			set
			{
				if (_userName != null && _userName != value)
				{
					Utils.SetUserName(value);
				}
				_userName = value;
				OnPropertyChanged("UserName");
			}
		}
		public string Password
		{
			get { return _password; }
			set
			{
				if (_password != null && _password != value)
				{
					Utils.SetPassword(value);
				}
				_password = value;
				OnPropertyChanged("Password");
			}
		}

		public AppSettings()
		{
			this.ServerName = Utils.GetServerName();
			this.OrgName = Utils.GetOrgName();
			this.UserName = Utils.GetUserName();
			this.Password = Utils.GetPassword();
		}

		public Boolean IsNotFull()
		{
			return String.IsNullOrEmpty(this.ServerName)
				|| String.IsNullOrEmpty(this.UserName)
				|| String.IsNullOrEmpty(this.OrgName)
				|| String.IsNullOrEmpty(this.Password);
		}

		public void SaveSettings()
		{
			Properties.Settings.Default.Save();
		}

		#region IDataErrorInfo Members

		public string Error
		{
			get { throw new NotImplementedException(); }
		}

		public string this[string columnName]
		{
			get
			{
				string result = null;
				if (columnName == "ServerName")
				{
					if (string.IsNullOrEmpty(ServerName))
						result = "Please enter a Server Name";
				}
				if (columnName == "OrgName")
				{
					if (string.IsNullOrEmpty(OrgName))
						result = "Please enter a Org Name";
				}
				if (columnName == "UserName")
				{
					if (string.IsNullOrEmpty(UserName))
						result = "Please enter a User Name";
				}
				if (columnName == "Password")
				{
					if (string.IsNullOrEmpty(Password))
						result = "Please enter a Password";
				}
				return result;
			}
		}

		#endregion


		#region Property Change Notification

		protected void OnPropertyChanged(string name)
		{
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null)
			{
				handler(this, new PropertyChangedEventArgs(name));
			}
		}

		#endregion
	}
}
