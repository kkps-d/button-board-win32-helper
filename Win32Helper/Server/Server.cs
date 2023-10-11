using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;

namespace Win32Helper
{
    internal class Server
    {
        internal static async Task Start(int port, bool keepServerAliveOnNonCriticalExceptions = true)
        {
            TcpListener? server = null;

            bool shouldProgramTerminate;

            // Start the server
            try
            {
                server = new TcpListener(IPAddress.Loopback, port);
                server.Start();

                // Handle the client requests
                do
                {
                    Console.WriteLine("[Server] Listening for connections at {0}:{1}", IPAddress.Loopback, port);
                    TcpClient client = server.AcceptTcpClient();
                    Console.WriteLine("[Server] Connected!");
                    bool criticalExceptionEncountered = await HandleClient(client);
                    shouldProgramTerminate = criticalExceptionEncountered || !keepServerAliveOnNonCriticalExceptions;
                } while (!shouldProgramTerminate);
            }
            catch (SocketException ex)
            {
                Console.WriteLine("[Server] SocketException: {0}", ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("[Server] Unknown exception:");
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                Console.WriteLine("[Server] Cleaning up and shutting down");
                server?.Stop();
            }
        }

        private static async Task<bool> HandleClient(TcpClient client)
        {
            try
            {

                while (true)
                {
                    NetworkStream stream = client.GetStream();
                    byte[] buffer = new byte[4096];
                    int bytesRead = await stream.ReadAsync(buffer);

                    if (bytesRead == 0) throw new IOException("No bytes were read. Stream may be closed and client may have disconnected");

                    string receivedString = Encoding.ASCII.GetString(buffer);
                    string[] messages = receivedString.Split(";");

                    foreach (string message in messages)
                    {
                        if (message.StartsWith('\0')) continue;

                        // getAudioSessions,7,{0.0.0.00000000}.{608aeb73-45a2-4291-8e49-4af3adcd1ccc}

                        string command = "";
                        int messageNum = 0;
                        string payload = "";

                        int firstCommaIndex = message.IndexOf(",");
                        if (firstCommaIndex <= 0) continue; // Potentially corrupted message

                        command = message.Substring(0, firstCommaIndex);

                        int secondCommaIndex = message.IndexOf(",", firstCommaIndex + 1);

                        Console.WriteLine("{0}, {1}", firstCommaIndex, secondCommaIndex);

                        if (secondCommaIndex <= 0)
                        {
                            messageNum = int.Parse(message.Substring(firstCommaIndex + 1));
                        } else
                        {
                            messageNum = int.Parse(message.Substring(firstCommaIndex + 1, secondCommaIndex - firstCommaIndex - 1));
                            payload = message.Substring(secondCommaIndex + 1);
                        }

                        Console.WriteLine("[HandleClient] Command: '{0}', MsgNum: {1}, Payload: '{2}'", command, messageNum, payload);

                        // Convert the method to upper case
                        string methodName = char.ToUpper(command[0]) + command.Substring(1);

                        // Get the method and call it
                        MethodInfo? method = typeof(ServerFunctions).GetMethod(methodName);
                        if (method != null)
                        {
                            method.Invoke(null, new object[] { stream, messageNum, payload });
                        }
                        else
                        {
                            Console.WriteLine("[HandleClient] Unknown command '{0}'", methodName);
                        }
                    }


                }

                client.Close();
            }
            catch (IOException ex)
            {
                Console.WriteLine("[HandleClient] Client has disconnected");
                Console.WriteLine("[HandleClient] IOException: {0}", ex.Message);
                return false; // Not a critical exception
            }
            catch (Exception ex)
            {
                Console.WriteLine("[HandleClient] Critical exception encountered!");
                Console.WriteLine(ex.ToString());
                return true; // A critical exception
            }

            return false;
        }
    }
}
