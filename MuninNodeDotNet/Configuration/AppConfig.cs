using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Configuration;

namespace MuninNodeDotNet
{
	class AppConfig : ConfigurationSection
	{
		public List<Plugins.NodePlugin> activePlugins = new List<Plugins.NodePlugin>();
		public List<module> moduleConfigs = new List<module>();

		[ConfigurationProperty("performanceCounters")]
		public performanceCounters perfCounters
		{
			get { return (performanceCounters)this["performanceCounters"]; }
		}

		[ConfigurationProperty("modules")]
		public modules modules
		{
			get { return (modules)this["modules"]; }
		}

		[ConfigurationProperty("debug", DefaultValue=false, IsRequired=false)]
		public Boolean debug
		{
			get { return (Boolean)this["debug"]; }
		}

		[ConfigurationProperty("certificateFile", DefaultValue = "", IsRequired = false)]
		public String certificateFile
		{
			get { return (String)this["certificateFile"]; }
		}

		[ConfigurationProperty("certificatePassword", DefaultValue = "", IsRequired = false)]
		public String certificatePassword
		{
			get { return (String)this["certificatePassword"]; }
		}

		public void init()
		{
			try
			{
				modules modules = ((AppConfig)ConfigurationManager.GetSection("munin")).modules;
				foreach (module m in modules)
				{
					if (m.enabled)
					{
						if (m.name.Equals("checkPerfCount"))
						{
							try
							{
								performanceCounters perfCounters = ((AppConfig)ConfigurationManager.GetSection("munin")).perfCounters;
								foreach (performanceCounter perfCounter in perfCounters)
								{
									Plugins.checkPerfCount check = new Plugins.checkPerfCount(perfCounter);
									activePlugins.Add(check);
								}
							}
							catch (System.Configuration.ConfigurationErrorsException ex)
							{
								NodeService.log(ex.Message);
							}
						}
						else
						{
							moduleConfigs.Add(m);
							activePlugins.Add((Plugins.NodePlugin)Activator.CreateInstance(null, String.Format("MuninNodeDotNet.Plugins.{0}", m.name)).Unwrap());
						}
					}
				}
			}
			catch (System.Configuration.ConfigurationErrorsException ex)
			{
				NodeService.log(ex.Message);
			}
		}

	}

	public class performanceCounter : ConfigurationElement
	{
		[ConfigurationProperty("name", IsRequired = true, IsKey = true)]
		public String name
		{
			get
			{
				return (String)this["name"];
			}
		}

		[ConfigurationProperty("perfObject", IsRequired = true)]
		public String perfObject
		{
			get
			{
				return (String)this["perfObject"];
			}
		}

		[ConfigurationProperty("perfCounter", IsRequired = true)]
		public String perfCounter
		{
			get
			{
				return (String)this["perfCounter"];
			}
		}

		[ConfigurationProperty("perfInstance", DefaultValue = "", IsRequired = false)]
		public String perfInstance
		{
			get
			{
				return (String)this["perfInstance"];
			}
		}

		[ConfigurationProperty("skipInstances", DefaultValue = "", IsRequired = false)]
		public String skipInstances
		{
			get
			{
				return (String)this["skipInstances"];
			}
		}

		[ConfigurationProperty("graphCategory", IsRequired = true)]
		public String graphCategory
		{
			get
			{
				return (String)this["graphCategory"];
			}
		}

		[ConfigurationProperty("graphMultiplier", DefaultValue = 1.000000, IsRequired = false)]
		public Double graphMultiplier
		{
			get
			{
				return (Double)this["graphMultiplier"];
			}
		}

		[ConfigurationProperty("graphTitle", IsRequired = true)]
		public String graphTitle
		{
			get
			{
				return (String)this["graphTitle"];
			}
		}

		[ConfigurationProperty("graphArgs", IsRequired = true)]
		public String graphArgs
		{
			get
			{
				return (String)this["graphArgs"];
			}
		}

		[ConfigurationProperty("graphDraw", DefaultValue="LINE", IsRequired = false)]
		public String graphDraw
		{
			get
			{
				return (String)this["graphDraw"];
			}
		}

		[ConfigurationProperty("graphType", DefaultValue = "GAUGE", IsRequired = false)]
		public String graphType
		{
			get
			{
				return (String)this["graphType"];
			}
		}
	}

	public class performanceCounters : ConfigurationElementCollection
	{
		public performanceCounter this[int index]
		{
			get
			{
				return base.BaseGet(index) as performanceCounter;
			}
		}

		public new performanceCounter this[String key]
		{
			get
			{
				return base.BaseGet(key) as performanceCounter;
			}
		}

		protected override object GetElementKey(ConfigurationElement element)
		{
			return ((performanceCounter)element).name;
		}

		protected override ConfigurationElement CreateNewElement()
		{
			return new performanceCounter();
		}
	}

	public class module : ConfigurationElement
	{
		[ConfigurationProperty("name", IsRequired = true, IsKey = true)]
		public String name
		{
			get
			{
				return (String)this["name"];
			}
		}

		[ConfigurationProperty("enabled", DefaultValue = false, IsRequired = true)]
		public Boolean enabled
		{
			get
			{
				return (Boolean)this["enabled"];
			}
		}

		[ConfigurationProperty("warning", DefaultValue = Int32.MinValue, IsRequired = false)]
		public Int32 warning
		{
			get
			{
				return (Int32)this["warning"];
			}
		}

		[ConfigurationProperty("critical", DefaultValue = Int32.MinValue, IsRequired = false)]
		public Int32 critical
		{
			get
			{
				return (Int32)this["critical"];
			}
		}

		[ConfigurationProperty("networkInterface", DefaultValue = "*", IsRequired = false)]
		public String networkInterface
		{
			get
			{
				return (String)this["networkInterface"];
			}
		}

	}

	public class modules : ConfigurationElementCollection
	{
		public module this[int index]
		{
			get
			{
				return base.BaseGet(index) as module;
			}
		}

		public new module this[String key]
		{
			get
			{
				return base.BaseGet(key) as module;
			}
		}

		protected override object GetElementKey(ConfigurationElement element)
		{
			return ((module)element).name;
		}

		protected override ConfigurationElement CreateNewElement()
		{
			return new module();
		}
	}
}
