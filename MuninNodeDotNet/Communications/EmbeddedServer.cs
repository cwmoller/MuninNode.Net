using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace MuninNodeDotNet
{
	public class EmbeddedServer
	{
		private TcpListener listener = null;
		private Thread loop = null;
		private Char[] lineBreaks = { '\n' };
		public X509Certificate serverCertificate = null;

		public delegate String[] ProcessRequestDelegate(String command);
		public ProcessRequestDelegate processDelegate = null;

		public EmbeddedServer(String certificate, String password)
		{
			try
			{
				//openssl pkcs12 -export -out monster.chilli.local.pfx -in monster.chilli.local.crt -inkey monster.chilli.local.key -certfile ca.crt
				serverCertificate = new X509Certificate2(System.IO.File.ReadAllBytes(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), certificate)), password ?? "", X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable);

				NodeService.log(String.Format("Certificate issuer {0}", serverCertificate.Issuer));
			}
			catch (Exception ex)
			{
				NodeService.log(ex.Message);
			}
		}

		public void Start()
		{
			loop = new Thread(new ThreadStart(Loop));
			loop.Start();
		}

		public void Loop()
		{
			try
			{
				listener = new TcpListener(IPAddress.Any, 4949);
				listener.Start();

				while (true)
				{
					NodeService.log("Waiting for a client to connect...");
					TcpClient client = listener.AcceptTcpClient();
					NodeService.log(String.Format("Connection from {0}", client.Client.RemoteEndPoint));
					ThreadPool.QueueUserWorkItem(new WaitCallback(ServiceClient), client);
				}

			}
			catch (Exception)
			{
			}
			finally
			{
				NodeService.log("Listener stopped");
			}

		}

		public void ServiceClient(object remote)
		{
			TcpClient client = (TcpClient)remote;
			try
			{
				String input = "";
				Byte[] data = new Byte[8192];
				Byte[] send = null;
				String[] returnValue = null;
				Int32 bytesRec = 0;
				Int32 marker = -1;
				Boolean escape = false;
				Boolean tls = false;

				send = Encoding.UTF8.GetBytes(String.Format("# munin node at {0}\n", System.Environment.MachineName));
				NetworkStream stream = client.GetStream();
				stream.Write(send, 0, send.Length);

				while (!escape && !tls && ((bytesRec = stream.Read(data, 0, 8192)) > 0))
				{
					input += Encoding.UTF8.GetString(data, 0, bytesRec);
					NodeService.log("Input: " + input);
					while (!escape && ((marker = input.IndexOfAny(lineBreaks)) > -1))
					{
						String command = input.Substring(0, marker);
						input = input.Substring(marker + 1);
						NodeService.log("Received: " + command);

						if (processDelegate != null)
						{
							returnValue = processDelegate(command);
							if (returnValue != null)
							{
								foreach (String s in returnValue)
								{
									NodeService.log(String.Format("Sending ({0}): {1}", s.Length, s));
									stream.Write(Encoding.UTF8.GetBytes(s), 0, s.Length);
									tls = s.Equals("TLS OK\n");
								}
							}
							else
							{
								escape = true;
							}
						}
					}
				}
				if (tls)
				{
					SslStream sslStream = new SslStream(stream, false);
					NodeService.log("SSL stream opened");
					try
					{
						try
						{
							sslStream.AuthenticateAsServer(serverCertificate, false, SslProtocols.Tls, false);
						}
						catch (Exception ex)
						{
							NodeService.log(ex.Message);
						}
						while (!escape && ((bytesRec = sslStream.Read(data, 0, 8192)) > 0))
						{
							input += Encoding.UTF8.GetString(data, 0, bytesRec);
							NodeService.log("TLS Input: " + input);
							while (!escape && ((marker = input.IndexOfAny(lineBreaks)) > -1))
							{
								string command = input.Substring(0, marker);
								input = input.Substring(marker + 1);
								NodeService.log("TLS Received: " + command);

								if (processDelegate != null)
								{
									returnValue = processDelegate(command);
									if (returnValue != null)
									{
										foreach (String s in returnValue)
										{
											NodeService.log(String.Format("TLS Sending ({0}): {1}", s.Length, s));
											sslStream.Write(Encoding.UTF8.GetBytes(s));
										}
									}
									else
									{
										escape = true;
									}
								}
							}
						}
					}
					catch (AuthenticationException e)
					{
						NodeService.log(String.Format("Exception: {0}", e.Message));
						if (e.InnerException != null)
						{
							NodeService.log(String.Format("Inner exception: {0}", e.InnerException.Message));
						}
					}
					finally
					{
						// The client stream will be closed with the sslStream
						// because we specified this behavior when creating
						// the sslStream.
						sslStream.Close();
					}
				}

			}
			catch (Exception)
			{
				NodeService.log("Connection Error");
			}
			finally
			{
				try
				{
					client.Close();
				}
				catch (Exception) { }
				NodeService.log("Connection Closed");
			}
		}

		public void Stop()
		{
			if (listener != null)
			{
				listener.Stop();
			}
		}

	}
}
