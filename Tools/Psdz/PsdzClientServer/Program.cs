using System;

namespace PsdzClientServer
{
    internal class Program
    {
        static int Main(string[] args)
        {
            PsdzJsonRpcServer server = new PsdzJsonRpcServer();

            try
            {
                Console.CancelKeyPress += (_, e) =>
                {
                    e.Cancel = true;
                    server.Stop();
                };


                Console.WriteLine("Starting PsdzJsonRpcServer...");
                server.StartAsync().Wait();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error starting PsdzJsonRpcServer: {ex.Message}");
            }
            finally
            {
                server.Stop();
            }

            Console.WriteLine("PsdzJsonRpcServer stopped.");
            return 0;
        }
    }
}
