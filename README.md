MuninNode.Net
=============

A Microsoft.Net implementation of munin-node with support for TLS and performance counters.

Configuration
=============

This example configuration enables all plugins and defines four performance counter graphs.

```xml
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
	<configSections>
		<section name="munin" type="MuninNodeDotNet.AppConfig,MuninNodeDotNet" restartOnExternalChanges="true" />
	</configSections>
	<munin debug="false" certificateFile="munin-node.pfx" certificatePassword="password">
		<modules>
			<add name="checkDisk" enabled="true" warning="80" critical="90" />
			<add name="checkMemory" enabled="true" />
			<add name="checkPerfCount" enabled="true" />
<!--
			<add name="checkNetwork" enabled="true" networkInterface="Realtek PCIe GBE Family Controller" />
-->
<!-- use 'Network Adapter' rather than 'Network Interface', get list from Powershell 'Get-NetworkAdapter' command
			<add name="checkNetwork" enabled="true" useNetworkAdapter="true" networkInterface="Local Area Connection *2" />
-->
			<add name="checkNetwork" enabled="true" />
			<add name="checkProcess" enabled="true" />
			<add name="checkExternal" enabled="true" />
		</modules>
		<performanceCounters>
			<add name="processor" perfObject="Processor" perfCounter="% Processor Time" perfInstance="*" skipInstances="_Total" graphMultiplier="1.000000" graphTitle="Processor Time" graphCategory="system" graphArgs="--base 1000 -l 0" graphDraw="STACK" />
			<add name="uptime" perfObject="System" perfCounter="System Up Time" graphMultiplier="1.1574074074074073e-005" graphTitle="Uptime" graphCategory="system" graphArgs="--base 1000 -l 0" graphDraw="AREA" />
			<add name="cps" perfObject="ServiceModelService 4.0.0.0" perfCounter="Calls Per Second" perfInstance="*" graphMultiplier="1.000000" graphTitle="Calls Per Second" graphCategory="wcf" graphArgs="--base 1000 -l 0" graphDraw="LINE" />
			<add name="cout" perfObject="ServiceModelService 4.0.0.0" perfCounter="Calls Outstanding" perfInstance="*" graphMultiplier="1.000000" graphTitle="Calls Outstanding" graphCategory="wcf" graphArgs="--base 1000 -l 0" graphDraw="LINE" />
		</performanceCounters>
		<externals>
			<add name="random" cmd="C:\Progra~2\MuninNode.Net\random.ps1" shell="C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe" shellArgs="-executionpolicy remotesigned" />
			<add name="random2" cmd="C:\Progra~2\MuninNode.Net\random.cmd" shell="C:\Windows\System32\cmd.exe" shellArgs="/Q /C" />
		</externals>
	</munin>
</configuration>
```

Set `debug` to true to have debugging output written to the event log.

`checkDisk` can take two parameters, `warning` and `critical` which override the default values.

`checkNetwork` can take one parameter, `networkInterface` which gives the name of the interface to graph. If this parameter is not specified, traffic from all interfaces are summed.

If `checkPerfCount` is enabled, all performance counters listed in the `performanceCounters` section will be loaded and opened at startup.

If `checkExternal` is enabled, all external scripts/executables listed in the `externals` section will be loaded. Please note that enabling this plugin poses a serious security risk. Scripts/Executables will be run as the service user, generally LocalSystem.

Performance Counters
--------------------

**Required parameters**

- `name`, should be unique, check will be prepended with `perf_`
- `perfObject`
- `perfCounter`
- `graphCategory`
- `graphTitle`
- `graphArgs`

**Optional parameters**

- `perfInstance`, defaults to `""` (no instances for counter), use `"*"` for all instances, `"my instance"` for specific instance
- `skipInstances`, semi-colon separated list of instances to ignore. Only makes sense if `perfInstance` is set to `"*"`
- `graphMultiplier`, defaults to 1.000000
- `graphDraw`, defaults to "LINE"
- `graphType`, defaults to "GAUGE"

External Scripts
----------------

**Required parameters**

- `name`, should be unique, check will be prepended with `ex_`
- `cmd`, full pathname of the script/executable. Use 8.3 style short paths (no spaces).

**Optional parameters**

- `shell`, shell to execute script in if it is not an executable, defaults to none. Use 8.3 style short paths (no spaces).
- `shellArgs`, arguments to the shell, defaults to none

Security
========

TLS is supported, requires a certificate with key. Assuming you've created a certificate and key with OpenSSL before, create a valid PKCS12 file with the following

```bash
$ openssl pkcs12 -export -out munin-node.pfx -in munin-node.crt -inkey munin-node.key -certfile ca.crt
```

Or see [this](http://windowsitpro.com/blog/creating-self-signed-certificates-powershell) very good post for doing it with PowerShell.

Any valid PKCS12 certificate should work. The certificate should be placed in the application folder.

License
=======

This code is distributed under the terms and conditions of the MIT license.

Change log
==========

**20121127**
- Initial release

**20121128**
- Added support for external scripts
