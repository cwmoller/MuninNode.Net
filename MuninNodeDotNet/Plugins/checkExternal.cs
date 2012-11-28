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
			List<String> result = new List<String>();
			ProcessStartInfo program = new ProcessStartInfo();
			if (command.shell == null)
			{
				program.FileName = command.cmd;
				program.Arguments = argument;
			}
			else
			{
				program.FileName = command.shell;
				program.Arguments = String.Format("{0} {1}", command.cmd, argument);
				if (command.shellArgs != null)
				{
					program.Arguments = String.Format("{0} {1}", command.shellArgs, program.Arguments);
				}
			}
			program.UseShellExecute = false;
			program.RedirectStandardOutput = true;
			program.WindowStyle = ProcessWindowStyle.Hidden;

			NodeService.log(String.Format("Executing {0} {1}", program.FileName, program.Arguments));
			try
			{
				using (Process process = Process.Start(program))
				{
					using (StreamReader reader = process.StandardOutput)
					{
						while (!reader.EndOfStream)
						{
							result.Add(reader.ReadLine() + "\n");
						}
					}
				}
			}
			catch (Exception ex)
			{
				NodeService.log(ex.Message);
			}

			return result.ToArray();
		}

	}
}
