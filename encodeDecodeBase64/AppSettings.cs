using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace encodeDecodeBase64
{
	public class AppSettings : IDataErrorInfo, INotifyPropertyChanged
	{
		private string _serverName;
		private string _orgName;
		private string _userName;
		private string _password;
		private string _lastPath;

		[field: NonSerialized()]
		public event PropertyChangedEventHandler PropertyChanged;

		public string ServerName {
			get { return _serverName; }
			set
			{
				_serverName = value;
				OnPropertyChanged("ServerName");
			}
		}
		public string OrgName
		{
			get { return _orgName; }
			set
			{
				_orgName = value;
				OnPropertyChanged("OrgName");
			}
		}
		public string UserName
		{
			get { return _userName; }
			set
			{
				_userName = value;
				OnPropertyChanged("UserName");
			}
		}
		public string Password
		{
			get { return _password; }
			set
			{
				_password = value;
				OnPropertyChanged("Password");
			}
		}
		public string LastPath
		{
			get { return _lastPath; }
			set
			{
				_lastPath = value;
				OnPropertyChanged("Password");
			}
		}

		public AppSettings()
		{
			//this.ServerName = Utils.GetServerName();
			//this.OrgName = Utils.GetOrgName();
			//this.UserName = Utils.GetUserName();
			//this.Password = Utils.GetPassword();
		}

		public string GetServerNameFull()
		{
			return String.Format("{0}.{0}dom.extest.microsoft.com", this.ServerName);
		}

		public string GetUserNameFull()
		{
			return String.Format(@"{0}dom\{1}", this.ServerName, this.UserName);
		}

		public SecureString GetSecuredPassword()
		{
			var secure = new SecureString();
			foreach (char c in this.Password)
			{
				secure.AppendChar(c);
			}

			return secure;
		}

		public Boolean IsNotFull()
		{
			return String.IsNullOrEmpty(this.ServerName)
				|| String.IsNullOrEmpty(this.UserName)
				|| String.IsNullOrEmpty(this.OrgName)
				|| String.IsNullOrEmpty(this.Password);
		}

		public void SaveSettings(string jsonFileName)
		{
			string data = JsonConvert.SerializeObject(this, Formatting.Indented);
			File.WriteAllText(jsonFileName, data);
		}

		#region IDataErrorInfo Members

		[JsonIgnore]
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
