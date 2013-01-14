using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace MuninNodeDotNet.Plugins
{
	class checkMemory : NodePlugin
	{
		public checkMemory()
		{
			pluginName = "checkMemory";
			NodeService.log(String.Format("{0} initialized", pluginName));

		}

		override public String[] getConfig()
		{
			List<String> config = new List<String>();
			config.Add("graph_args --base 1024 -l 0 --vertical-label Bytes\n");
			config.Add("graph_title Memory usage\n");
			config.Add("graph_category system\n");
			config.Add("graph_info Graphs memory usage\n");
			config.Add("graph_order apps free swap\n");
			config.Add("apps.label apps\n");
			config.Add("apps.draw AREA\n");
			config.Add("apps.info Memory used by user-space applications\n");
			config.Add("swap.label swap\n");
			config.Add("swap.draw STACK\n");
			config.Add("swap.info Swap space used\n");
			config.Add("free.label unused\n");
			config.Add("free.draw STACK\n");
			config.Add("free.info Free memory\n");
			config.Add(".\n");
			return config.ToArray();
		}

		override public String[] getValues()
		{
			NativeMethods.MEMORYSTATUSEX status = new NativeMethods.MEMORYSTATUSEX();
			status.dwLength = (uint)Marshal.SizeOf(status);
			Boolean ret = NativeMethods.GlobalMemoryStatusEx(ref status);

			if (ret)
			{
				String[] value = new String[4];
				value[0] = String.Format("apps.value {0}\n", status.ulTotalPhys - status.ulAvailPhys);
				ulong swap = ((status.ulTotalPageFile - status.ulAvailPageFile) < (status.ulTotalPhys - status.ulAvailPhys)) ? 0 : status.ulTotalPageFile - status.ulAvailPageFile - status.ulTotalPhys + status.ulAvailPhys;

				value[1] = String.Format("swap.value {0}\n", swap);
				value[2] = String.Format("free.value {0}\n", status.ulAvailPhys);
				value[3] = ".\n";
				return value;
			}
			else
			{
				return new String[] { ".\n" };
			}
		}

	}
}
