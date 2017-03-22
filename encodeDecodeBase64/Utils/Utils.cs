using System;
using System.Security;

namespace encodeDecodeBase64
{
	class Utils
	{
		public static string GetServerNameFull()
		{
			return String.Format("{0}.{0}dom.extest.microsoft.com", Properties.Settings.Default["Servername"].ToString());
		}

		public static string GetServerName()
		{
			return Properties.Settings.Default["Servername"].ToString();
		}

		public static void SetServerName(string value)
		{
			Properties.Settings.Default["Servername"] = value;
		} 

		public static string GetUserName()
		{
			return Properties.Settings.Default["Login"].ToString();
		}

		public static string GetUserNameFull()
		{
			return String.Format(@"{0}dom\{1}", Properties.Settings.Default["Servername"].ToString(), Properties.Settings.Default["Login"].ToString());
		}

		public static void SetUserName(string value)
		{
			Properties.Settings.Default["Login"] = value;
		}

		public static string GetPassword()
		{
			return Properties.Settings.Default["Password"].ToString();
		}

		public static void SetPassword(string value)
		{
			Properties.Settings.Default["Password"] = value;
		}

		public static SecureString GetSecuredPassword()
		{
			var secure = new SecureString();
			foreach (char c in Properties.Settings.Default["Password"].ToString())
			{
				secure.AppendChar(c);
			}

			return secure;
		}

		public static string GetOrgName()
		{
			return Properties.Settings.Default["Orgname"].ToString();
		}

		public static void SetOrgName(string value)
		{
			Properties.Settings.Default["Orgname"] = value;
		}

		public static string GetLastPath()
		{
			return Properties.Settings.Default["LastPath"].ToString();
		}

		public static void SetLastPath(string path)
		{
			Properties.Settings.Default["LastPath"] = path;
		}
	}
}
