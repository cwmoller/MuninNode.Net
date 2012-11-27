using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Reflection;
using System.Configuration;

namespace MuninNodeDotNet
{
	public partial class NodeService : ServiceBase
	{
		private static Boolean debugging = false;
		internal static EmbeddedServer server = null;
		internal static AppConfig config = null;

		public NodeService()
		{
			InitializeComponent();
			this.ServiceName = "MuninNode.Net";
			this.CanStop = true;
			this.CanPauseAndContinue = false;
			this.AutoLog = true;
		}

		protected override void OnStart(string[] args)
		{
			config = new AppConfig();

			debugging = ((AppConfig)ConfigurationManager.GetSection("munin")).debug;

			config.init();

			server = new EmbeddedServer(((AppConfig)ConfigurationManager.GetSection("munin")).certificateFile, ((AppConfig)ConfigurationManager.GetSection("munin")).certificatePassword);
			server.processDelegate = ProcessRequest;
			server.Start();
		}

		protected override void OnStop()
		{
			if (server != null)
			{
				server.Stop();
			}
		}

		public static void log(string evt)
		{
			if (debugging)
			{
				const String eSource = "MuninNode.Net";
				const String eLog = "Application";

				if (!EventLog.SourceExists(eSource))
				{
					EventLog.CreateEventSource(eSource, eLog);
				}
				EventLog.WriteEntry(eSource, evt);
			}
		}

		public static String[] ProcessRequest(String command)
		{
			List<String> rtn = new List<String>();
			String cmd = command.ToLower().Trim();
			String param = "";
			if (cmd.IndexOf(" ") > 0)
			{
				cmd = command.ToLower().Substring(0, command.IndexOf(" ")).Trim();
				param = command.Substring(command.IndexOf(" ") + 1).Trim();
			}
			switch (cmd)
			{
				case "starttls":
					rtn.Add((server.serverCertificate != null) ? "TLS OK\n" : "TLS MAYBE\n");
					break;
				case "cap":
					rtn.Add(".\n");
					break;
				case "version":
					rtn.Add(String.Format("munin node on {0} version: {1}\n", System.Environment.MachineName, Assembly.GetExecutingAssembly().GetName().Version));
					break;
				case "nodes":
					rtn.Add(String.Format("{0}\n.\n", System.Environment.MachineName));
					break;
				case "list":
					{
						String tmp = "";
						foreach (Plugins.NodePlugin p in config.activePlugins)
						{
							tmp += String.Format("{0} ", p.Name);
						}
						tmp += "\n";
						rtn.Add(tmp);
					}
					break;
				case "config":
					{
						foreach (Plugins.NodePlugin p in config.activePlugins)
						{
							if (param.Equals(p.Name))
							{
								rtn.AddRange(p.getConfig());
								break;
							}
						}
						if (rtn.Count == 0)
						{
							rtn.Add(String.Format("# Unknown service {0}\n.\n", param));
						}
					}
					break;
				case "fetch":
					{
						foreach (Plugins.NodePlugin p in config.activePlugins)
						{
							if (param.Equals(p.Name))
							{
								rtn.AddRange(p.getValues());
								break;
							}
						}
						if (rtn.Count == 0)
						{
							rtn.Add(String.Format("# Unknown service {0}\n.\n", param));
						}
					}
					break;
				case "quit":
					rtn.Add("");
					break;
				default:
					rtn.Add("# Unknown command. Try list, nodes, config, fetch, version or quit\n");
					break;
			}
			return rtn.ToArray();
		}

	}
}
