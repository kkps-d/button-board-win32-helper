using System.Net;
using System.Net.Sockets;
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

                    string message = Encoding.ASCII.GetString(buffer);

                    Console.WriteLine("[HandleClient] Received: {0}", message);
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
