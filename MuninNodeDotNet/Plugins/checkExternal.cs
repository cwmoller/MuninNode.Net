using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;

namespace MuninNodeDotNet.Plugins
{
	class checkExternal : NodePlugin
	{
		private external command = null;

		public checkExternal(external command)
		{
			this.command = command;
			pluginName = String.Format("ex_{0}", this.command.name);

			NodeService.log(String.Format("{0} initialized", pluginName));
		}

		override public String[] getConfig()
		{
			return execute("config");
		}

		override public String[] getValues()
		{
			return execute("");
		}

		private String[] execute(String argument)
		{
			String result = null;
			ProcessStartInfo program = new ProcessStartInfo();
			program.FileName = command.cmd;
			program.UseShellExecute = false;
			program.RedirectStandardOutput = true;
			program.WindowStyle = ProcessWindowStyle.Hidden;
			program.Arguments = argument;

			using (Process process = Process.Start(program))
			{
				using (StreamReader reader = process.StandardOutput)
				{
					result = reader.ReadToEnd();
				}
			}

			return result.Split(new Char[] { '\n' });
		}

	}
}
