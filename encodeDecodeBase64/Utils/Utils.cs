using System;
using System.Security.Principal;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

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
	}
}
