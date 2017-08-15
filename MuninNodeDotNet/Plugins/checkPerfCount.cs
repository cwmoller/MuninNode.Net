using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace MuninNodeDotNet.Plugins
{
	class checkPerfCount : NodePlugin
	{
		private performanceCounter config = null;
		private List<PerformanceCounter> counters = new List<PerformanceCounter>();

		public checkPerfCount(performanceCounter config)
		{
			this.config = config;
			pluginName = String.Format("perf_{0}", this.config.name);

			openCounters();

			NodeService.log(String.Format("{0} initialized", pluginName));
		}

		~checkPerfCount()
		{
			foreach (PerformanceCounter pc in counters)
			{
				pc.Close();
				NodeService.log(String.Format("{0}\\{1} closed", pc.CounterName, pc.InstanceName));
			}
		}

		override public Boolean isOK()
		{
			return (config != null);
		}
		
		override public String[] getConfig()
		{
			PerformanceCounterCategory category = new PerformanceCounterCategory(config.perfObject);

			List<String> ret = new List<String>();
			
			ret.Add(String.Format("graph_title {0}\n", config.graphTitle));
			ret.Add(String.Format("graph_category {0}\n", config.graphCategory));
			ret.Add(String.Format("graph_args {0}\n", config.graphArgs));
			ret.Add(String.Format("graph_info {0}\n", category.CategoryHelp));
			ret.Add(String.Format("graph_vlabel {0}\n", config.perfCounter));

			String draw = "{0}_{1}.draw {2}\n";
			String type = "{0}_{1}.type {2}\n";
			String min = config.graphType.Equals("DERIVE") ? "{0}_{1}.min 0\n" : "";
			String label = "{0}_{1}.label {2}\n";

			PerformanceCounter pc;
			for (int i = 0; i < counters.Count; i++)
			{
				pc = counters.ElementAt(i);
				ret.Add(String.Format(label, config.name, i, (pc.InstanceName.Length == 0) ? pc.CounterName : pc.InstanceName));
				if (i == 0)
				{
					if (config.graphDraw.Equals("STACK"))
					{
						ret.Add(String.Format(draw, config.name, i, "AREA"));
					}
					else
					{
						ret.Add(String.Format(draw, config.name, i, config.graphDraw));
					}
				}
				else
				{
					ret.Add(String.Format(draw, config.name, i, config.graphDraw));
				}
				ret.Add(String.Format(type, config.name, i, config.graphType));
				ret.Add(String.Format(min, config.name, i));
			}

			ret.Add(".\n");
			return ret.ToArray();
		}

		override public String[] getValues()
		{
			List<String> ret = new List<String>();
			Double value = 0;
			PerformanceCounter pc;
			try
			{
				for (int i = 0; i < counters.Count; i++)
				{
					pc = counters.ElementAt(i);
					NodeService.log(String.Format("Retrieving for {0}\\{1}", pc.InstanceName, pc.CounterName));
					value = pc.NextValue() * config.graphMultiplier;
					ret.Add(String.Format("{0}_{1}.value {2:F3}\n", config.name, i, value).Replace(',', '.'));
				}
			}
			catch (Exception ex)
			{
				NodeService.log(ex.Message);
			}
			ret.Add(".\n");
			return ret.ToArray();
		}

		private void openCounters()
		{
			try
			{
				switch (config.perfInstance)
				{
					case "":
					{
						PerformanceCounter pc = new PerformanceCounter(config.perfObject, config.perfCounter, true);
						pc.NextValue();
						counters.Add(pc);
					}
						break;
					case "*":
					{
						String[] skip = config.skipInstances.Split(new Char[] {';'});
						PerformanceCounterCategory category = new PerformanceCounterCategory(config.perfObject);
						Boolean found = false;
						foreach (String instance in category.GetInstanceNames())
						{
							found = false;
							NodeService.log(String.Format("Adding instance {0}\\{1}", category.CategoryName, instance));
							if (skip.Length > 0)
							{
								foreach (String s in skip)
								{
									if (s.Equals(instance))
									{
										NodeService.log(String.Format("Skipping instance {0}\\{1}", category.CategoryName, instance));
										found = true;
										break;
									}
								}
							}
							if (!found)
							{
								PerformanceCounter pc = new PerformanceCounter(config.perfObject, config.perfCounter, instance, true);
								pc.NextValue();
								if (counters.Count > 0)
								{
									int i = 0;
									foreach (PerformanceCounter p in counters)
									{
										if (p.InstanceName.CompareTo(pc.InstanceName) > 0)
										{
											counters.Insert(i, pc);
											i = -1;
											break;
										}
										i++;
									}
									if (i > 0)
									{
										counters.Add(pc);
									}
								}
								else
								{
									counters.Add(pc);
								}
							}
						}
					}
						break;
					default:
					{
						try
						{
							PerformanceCounter pc = new PerformanceCounter(config.perfObject, config.perfCounter, config.perfInstance, true);
							pc.NextValue();
							counters.Add(pc);
						}
						catch (Exception ex)
						{
							NodeService.log(ex.Message);
						}
					}
						break;
				}
			}
			catch (Exception ex)
			{
				NodeService.log(ex.Message);
				config = null;
			}
		}
	}
}
