using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace MuninNodeDotNet.Plugins
{
	class checkProcess : NodePlugin
	{
		public checkProcess()
		{
			pluginName = "checkProcess";
			NodeService.log(String.Format("{0} initialized", pluginName));
		}

		override public String[] getConfig()
		{
			List<String> config = new List<String>();
			config.Add("graph_args --base 1000 -l 0\n");
			config.Add("graph_title Processes and threads\n");
			config.Add("graph_category processes\n");
			config.Add("graph_info Graphs process and thread counts\n");
			config.Add("processes.label Processes\n");
			config.Add("processes.draw LINE2\n");
			config.Add("processes.info Processes\n");
			config.Add("threads.label Threads\n");
			config.Add("threads.draw LINE1\n");
			config.Add("threads.info Threads\n");
			config.Add(".\n");
			return config.ToArray();
		}

		override public String[] getValues()
		{
			Process[] processes = Process.GetProcesses();
			Int32 processCount = processes.Length;
			Int32 threadCount = 0;
			foreach (Process p in processes)
			{
				threadCount += p.Threads.Count;
			}
			String[] ret = new String[3];
			ret[0] = String.Format("processes.value {0}\n", processCount);
			ret[1] = String.Format("threads.value {0}\n", threadCount);
			ret[2] = ".\n";
			return ret;
		}
	}
}
