﻿using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace encodeDecodeBase64
{

	public class CustomControlViewModel
	{
		public ObservableCollection<CustomControl> dt { get; set; }
		public void LoadCustomControls(string[] files)
		{
			dt = new ObservableCollection<CustomControl>();
			foreach (var file in files)
			{
				CustomControl control = new CustomControl();
				control.Name = Path.GetFileName(file);
				control.Path = file;
				control.Content = File.ReadAllText(file);
				dt.Add(control);
			}
		}

		public void ReloadContent()
		{
			foreach (var control in dt)
			{
				control.Content = File.ReadAllText(control.Path);
			}
		}

		public bool IsEmpty()
		{
			return (dt == null) || (dt.Count == 0);
		}

		public bool HasSelectedRecords()
		{
			return !this.IsEmpty() && (this.dt.Count(x => x.shouldLoad == true) > 0);
		}

	}
	public class CustomControl
	{
		public string Name { get; set; }
		public string Path { get; set; }
		public string SqlNameMyProperty { get; set; }
		public string Content { get; set; }
		public bool shouldLoad { get; set; }

	}
}
