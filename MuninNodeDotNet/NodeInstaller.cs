using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;
using System.Diagnostics;

namespace MuninNodeDotNet
{
	[RunInstaller(true)]
	public class NodeInstaller : Installer
	{
		private ServiceProcessInstaller processInstaller;
		private ServiceInstaller serviceInstaller;

		public NodeInstaller()
		{
			processInstaller = new ServiceProcessInstaller();
			serviceInstaller = new ServiceInstaller();

			processInstaller.Account = ServiceAccount.LocalSystem;
			serviceInstaller.StartType = ServiceStartMode.Automatic;
			serviceInstaller.ServiceName = "MuninNode";

			Installers.Add(serviceInstaller);
			Installers.Add(processInstaller);
		}

		protected override void OnAfterInstall(System.Collections.IDictionary savedState)
		{
			base.OnAfterInstall(savedState);

			Process proc = Process.Start(
				new ProcessStartInfo {
					FileName = "netsh",
					Arguments = "advfirewall firewall add rule name=\"MuninNode.Net\" dir=in action=allow protocol=tcp localport=4949",
					WindowStyle = ProcessWindowStyle.Hidden
				}
			);
			proc.WaitForExit();
		}
	}
}
