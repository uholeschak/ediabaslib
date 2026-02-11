using System;
using System.Threading;
using System.Threading.Tasks;

namespace PsdzRpcServer
{
    internal class Program
    {
        static async Task<int> Main(string[] args)
        {
            using CancellationTokenSource cts = new CancellationTokenSource();
            PsdzRpcServer server = new PsdzRpcServer();

            try
            {
                CancellationTokenSource ctsLocal = cts;
                Console.CancelKeyPress += (_, e) =>
                {
                    e.Cancel = true;
                    ctsLocal.Cancel();
                };

                Console.WriteLine("Starting PsdzJsonRpcServer...");
                Task serverTask = server.StartAsync(cts.Token);
                Task keyTask = WaitForEscapeKeyAsync(cts.Token);

                await Task.WhenAny(serverTask, keyTask);

                cts.Cancel();

                await Task.WhenAll(
                    serverTask.ContinueWith(_ => { }),
                    keyTask.ContinueWith(_ => { }));
            }
            catch (OperationCanceledException)
            {
                // Erwartet bei Abbruch
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return 1;
            }

            Console.WriteLine("PsdzJsonRpcServer stopped.");
            return 0;
        }

        private static async Task WaitForEscapeKeyAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                if (Console.KeyAvailable)
                {
                    ConsoleKeyInfo key = Console.ReadKey(intercept: true);
                    if (key.Key == ConsoleKey.Escape)
                    {
                        Console.WriteLine("ESC pressed, stopping server...");
                        return;
                    }
                }
                await Task.Delay(100, ct);
            }
        }
    }
}
