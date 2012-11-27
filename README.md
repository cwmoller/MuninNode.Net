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
			<add name="checkNetwork" enabled="true" />
			<add name="checkProcess" enabled="true" />
		</modules>
		<performanceCounters>
			<add name="processor" perfObject="Processor" perfCounter="% Processor Time" perfInstance="*" skipInstances="_Total" graphMultiplier="1.000000" graphTitle="Processor Time" graphCategory="system" graphArgs="--base 1000 -l 0" graphDraw="STACK" />
			<add name="uptime" perfObject="System" perfCounter="System Up Time" graphMultiplier="1.1574074074074073e-005" graphTitle="Uptime" graphCategory="system" graphArgs="--base 1000 -l 0" graphDraw="AREA" />
			<add name="cps" perfObject="ServiceModelService 4.0.0.0" perfCounter="Calls Per Second" perfInstance="*" graphMultiplier="1.000000" graphTitle="Calls Per Second" graphCategory="wcf" graphArgs="--base 1000 -l 0" graphDraw="LINE" />
			<add name="cout" perfObject="ServiceModelService 4.0.0.0" perfCounter="Calls Outstanding" perfInstance="*" graphMultiplier="1.000000" graphTitle="Calls Outstanding" graphCategory="wcf" graphArgs="--base 1000 -l 0" graphDraw="LINE" />
		</performanceCounters>
	</munin>
</configuration>
```

Set `debug` to true to have debugging output written to the event log.

`checkDisk` can take two parameters, `warning` and `critical` which override the default values.

`checkNetwork` can take one parameter, `networkInterface` which gives the name of the interface to graph. If this parameter is not specified, traffic from all interfaces are summed.

If `checkPerfCount` is enabled, all performance counters listed in the `performanceCounters` section will be loaded and opened at startup.

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

Security
========

TLS is supported, requires a certificate with key. Assuming you've created a certificate and key with OpenSSL before, create a valid PKCS12 file with the following

```bash
$ openssl pkcs12 -export -out munin-node.pfx -in munin-node.crt -inkey munin-node.key -certfile ca.crt
```

Any valid PKCS12 certificate created with OpenSSL or makecert.exe should work. The certificate should be placed in the application folder.

License
=======

This code is distributed under the terms and conditions of the MIT license.

Change log
==========

**20121127**
- Initial release