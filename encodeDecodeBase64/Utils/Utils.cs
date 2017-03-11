using System;
using System.Security;

namespace encodeDecodeBase64
{
	class Utils
	{
		public static string GetServerFullName()
		{
			return String.Format("{0}.{0}dom.extest.microsoft.com", Properties.Settings.Default["Servername"].ToString());
		}

		public static string GetServerShortName()
		{
			return Properties.Settings.Default["Servername"].ToString();
		}

		public static string GetFullUserName()
		{
			return String.Format(@"{0}dom\{1}", Properties.Settings.Default["Servername"].ToString(), Properties.Settings.Default["Login"].ToString());
		}

		public static string GetPassword()
		{
			return Properties.Settings.Default["Password"].ToString();
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
	}
}
