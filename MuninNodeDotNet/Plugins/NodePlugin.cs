using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MuninNodeDotNet.Plugins
{
	partial class NodePlugin
	{
		protected String pluginName;

		public NodePlugin()
		{
			pluginName = "NodePlugin";
		}

		public String Name
		{
			get { return pluginName; }
		}

		virtual public Boolean isOK()
		{
			return false;
		}

		virtual public String[] getConfig()
		{
			return new String[] { ".\n" };
		}

		virtual public String[] getValues()
		{
			return new String[] { ".\n" };
		}
	}
}
