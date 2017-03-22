using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace encodeDecodeBase64.Commands
{
	public class AppCommands
	{
		public static RoutedUICommand Upload = new RoutedUICommand("Upload js files to server DB",
																	  "UploadCmd",
																	  typeof(AppCommands));
	}
}
