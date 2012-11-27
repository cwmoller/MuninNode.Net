using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace MuninNodeDotNet.Plugins
{
	class checkNetwork : NodePlugin
	{
		private List<PerformanceCounter> counters = new List<PerformanceCounter>();
		private String instance = "*";
		private const String perfObject = "Network Interface";
		private readonly String[] perfCounters = { "Bytes Sent/sec", "Bytes Received/sec" };

		public checkNetwork()
		{
			pluginName = "checkNetwork";
			foreach (module m in ((AppConfig)NodeService.config).moduleConfigs)
			{
				if (m.name.Equals(pluginName))
				{
					instance = m.networkInterface;
					NodeService.log(String.Format("Using interface {0}", instance));
					break;
				}
			}

			String tmp = new Regex("[^a-z]").Replace(instance.ToLower(), "");
			pluginName = String.Format("net_{0}", (tmp.Length == 0) ? "all" : tmp);

			openCounters();

			NodeService.log(String.Format("{0} initialized", pluginName));
		}

		~checkNetwork()
		{
			foreach (PerformanceCounter pc in counters)
			{
				pc.Close();
				NodeService.log(String.Format("{0}\\{1} closed", pc.CounterName, pc.InstanceName));
			}
		}

		override public String[] getConfig()
		{
			List<String> config = new List<String>(); 
			config.Add("graph_order down up\n");
			config.Add(String.Format("graph_title Network traffic ({0})\n", instance));
			config.Add("graph_args --base 1000\n");
			config.Add("graph_vlabel bytes in (-) / out (+) per ${graph_period}\n");
			config.Add("graph_category network\n");
			config.Add("graph_info Graphs network traffic in bytes per second\n");
			config.Add("down.label Bps\n");
			config.Add("down.type GAUGE\n");
			config.Add("down.graph no\n");
			config.Add("up.label Bps\n");
			config.Add("up.type GAUGE\n");
			config.Add("up.negative down\n");
			config.Add(".\n");
			return config.ToArray();
		}

		override public String[] getValues()
		{
			List<String> ret = new List<String>();
			Double up = 0;
			Double down = 0;
			PerformanceCounter pc;
			try
			{
				for (int i = 0; i < counters.Count; i++)
				{
					pc = counters.ElementAt(i);
					NodeService.log(String.Format("Retrieving for {0}\\{1}", pc.InstanceName, pc.CounterName));
					if (pc.CounterName.Equals(perfCounters[0]))
					{
						up += pc.NextValue();
					}
					else
					{
						down += pc.NextValue();
					}
				}
			}
			catch (Exception ex)
			{
				NodeService.log(ex.Message);
			}

			ret.Add(String.Format("down.value {0}\n", (Int32)down));
			ret.Add(String.Format("up.value {0}\n", (Int32)up));
			ret.Add(".\n");

			return ret.ToArray();
		}

		private void openCounters()
		{
			try
			{
				switch (instance)
				{
					case "*":
						{
							PerformanceCounterCategory category = new PerformanceCounterCategory(perfObject);
							foreach (String s in category.GetInstanceNames())
							{
								foreach (String c in perfCounters)
								{
									NodeService.log(String.Format("Adding instance {0}\\{1}\\{2}", category.CategoryName, s, c));
									PerformanceCounter pc = new PerformanceCounter(perfObject, c, s, true);
									pc.NextValue();
									counters.Add(pc);
								}
							}
						}
						break;
					default:
						{
							foreach (String c in perfCounters)
							{
								NodeService.log(String.Format("Adding counter {0}\\{1}\\{2}", perfObject, instance, c));
								PerformanceCounter pc = new PerformanceCounter(perfObject, c, instance, true);
								pc.NextValue();
								counters.Add(pc);
							}
						}
						break;
				}
			}
			catch (Exception ex)
			{
				NodeService.log(ex.Message);
			}
		}

	}
}
