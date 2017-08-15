using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.IO;
using System.Configuration;

namespace MuninNodeDotNet.Plugins
{
	class checkDisk : NodePlugin
	{
		List<DriveInfo> drives = null;
		NameValueCollection settings = null;
		Int32 warning = 80;
		Int32 critical = 90;

		public checkDisk()
		{
			pluginName = "checkDisk";
			NodeService.log(String.Format("{0} initialized", pluginName));

			foreach (module m in ((AppConfig)NodeService.config).moduleConfigs)
			{
				if (m.name.Equals(pluginName)) {
					if (m.warning != Int32.MinValue)
					{
						warning = m.warning;
						NodeService.log(String.Format("checkDisk warning level set to {0}", warning));
					}
					if (m.warning != Int32.MinValue)
					{
						critical = m.critical;
						NodeService.log(String.Format("checkDisk critical level set to {0}", critical));
					}
					break;
				}
			}

			drives = getAllDisks();
		}

		override public String[] getConfig()
		{
			List<String> config = new List<String>();

			config.Add("graph_title Filesystem usage (in %)\n");
			config.Add("graph_category disk\n");
			config.Add("graph_info Graphs disk usage on all fixed disks\n");
			config.Add("graph_args --upper-limit 100 -l 0\n");
			config.Add("graph_vlabel %\n");

			for (int i = 0; i < drives.Count; i++)
			{
				config.Add(String.Format("_dev_{0}_.label {1}\n", i, drives[i].Name.Replace("\\", ""))); 
				config.Add(String.Format("_dev_{0}_.warning {1}\n", i, warning));
				config.Add(String.Format("_dev_{0}_.critical {1}\n", i, critical));
			}

			config.Add(".\n");
			return config.ToArray();
		}

		public void setConfig(NameValueCollection config)
		{
			settings = config;
		}

		private List<DriveInfo> getAllDisks() {
			List<DriveInfo> list = new List<DriveInfo>();
			foreach(DriveInfo d in DriveInfo.GetDrives()) {
				if (d.IsReady && (d.DriveType.Equals(DriveType.Fixed))) {
					list.Add(d);
				}
			}
			return list;
		}

		override public String[] getValues()
		{
			List<String> values = new List<String>();

			for (int i = 0; i < drives.Count; i++)
			{
				values.Add(String.Format("_dev_{0}_.value {1:F3}\n", i, 100.0 - (100.0 / drives[i].TotalSize * drives[i].TotalFreeSpace)).Replace(',', '.'));
			}

			values.Add(".\n");
			return values.ToArray();
		}
	}
}
