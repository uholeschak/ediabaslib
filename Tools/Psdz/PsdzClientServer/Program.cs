using System;
using System.Threading.Tasks;

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
                Task serverTask = server.StartAsync();

                while (!serverTask.IsCompleted)
                {
                    if (Console.KeyAvailable)
                    {
                        ConsoleKeyInfo key = Console.ReadKey(intercept: true);
                        if (key.Key == ConsoleKey.Escape)
                        {
                            Console.WriteLine("ESC pressed, stopping server...");
                            server.Stop();
                            break;
                        }
                    }

                    // Kurze Pause um CPU-Last zu reduzieren
                    Task.Delay(100).Wait();
                }

                serverTask.Wait();
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
