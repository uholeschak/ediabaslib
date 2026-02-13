using System;
using System.Threading;
using System.Threading.Tasks;

namespace PsdzRpcClient
{
    internal class Program
    {
        static async Task<int> Main(string[] args)
        {
            using CancellationTokenSource cts = new CancellationTokenSource();

            try
            {
                CancellationTokenSource ctsLocal = cts;
                Console.CancelKeyPress += (_, e) =>
                {
                    e.Cancel = true;
                    ctsLocal.Cancel();
                };

                await using var client = new PsdzRpcClient();
                client.CallbackHandler.ProgressChanged += (s, e) =>
                {
                    Console.WriteLine($"[{e.Percent}%] {e.Message}");
                };

                client.CallbackHandler.OperationCompleted += (s, success) =>
                {
                    Console.WriteLine($"Operation finished: {(success ? "Success" : "Error")}");
                };

                client.CallbackHandler.UpdatedStatus += (s, e) =>
                {
                    Console.WriteLine($"Status: {e}");
                };

                client.CallbackHandler.UpdateProgress += (sender, tuple) =>
                {
                    Console.WriteLine($"[{tuple.percent}%] Marquee={tuple.marquee}: {tuple.message}");
                };

                client.CallbackHandler.UpdateOptions += (sender, optionsDict) =>
                {
                    Console.WriteLine("Options updated:");
                    foreach (var kvp in optionsDict)
                    {
                        Console.WriteLine($"Key: {kvp.Key}, Options Count: {kvp.Value.Count}");
                    }
                };

                client.CallbackHandler.UpdateOptionSelections += (sender, swiRegisterEnum) =>
                {
                    Console.WriteLine($"Option selections updated: {swiRegisterEnum}");
                };
                
                client.CallbackHandler.ShowMessage += (sender, tuple) =>
                {
                    Console.WriteLine($"Message from server: {tuple.message} (OK Button: {tuple.okBtn}, Wait: {tuple.wait})");
                };

                Console.WriteLine("Starting PsdzJsonRpcClient...");
                Task clientTask = client.ConnectAsync(cts.Token);
                Task keyTask = WaitForEscapeKeyAsync(cts.Token);

                await Task.WhenAny(clientTask, keyTask);

                cts.Cancel();

                await Task.WhenAll(
                    clientTask.ContinueWith(_ => { }),
                    keyTask.ContinueWith(_ => { }));

                if (client.RpcService != null)
                {
                    bool resultConnect = await client.RpcService.Connect("Connect");
                    Console.WriteLine($"Connect = {resultConnect}");

                    bool resultDisconnect = await client.RpcService.Disconnect("Disconnect");
                    Console.WriteLine($"Disconnect = {resultDisconnect}");
                }
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

            Console.WriteLine("Client stopped.");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
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
                        Console.WriteLine("ESC pressed, stopping client...");
                        return;
                    }
                }
                await Task.Delay(100, ct);
            }
        }
    }
}
