using PsdzClient.Programming;
using PsdzRpcServer.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PsdzRpcClient
{
    internal class Program
    {
        static async Task<int> Main(string[] args)
        {
#if NET
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
#endif
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

                client.CallbackHandler.UpdateStatus += (s, e) =>
                {
                    Console.WriteLine($"Status: {e}");
                };

                client.CallbackHandler.UpdateProgress += (sender, tuple) =>
                {
                    Console.WriteLine($"[{tuple.percent}%] Marquee={tuple.marquee}: {tuple.message}");
                };

                client.CallbackHandler.UpdateOptions += async (sender, optionsDict) =>
                {
                    Console.WriteLine("Options updated");
                    if (client.RpcService != null)
                    {
                        List<PsdzRpcOptionType> optionTypes = await client.RpcService.GetOptionTypes();
                        if (optionTypes != null)
                        {
                            Console.WriteLine("Available option types:");
                            foreach (PsdzRpcOptionType option in optionTypes)
                            {
                                Console.WriteLine($"- {option.Caption} ({option.SwiRegisterEnum.ToString()})");
                            }
                        }
                    }
                };

                client.CallbackHandler.UpdateOptionSelections += (sender, swiRegisterEnum) =>
                {
                    Console.WriteLine($"Option selections updated: {swiRegisterEnum}");
                };

                client.CallbackHandler.ShowMessage += (sender, args) =>
                {
                    Console.WriteLine($"Message from server: {args.Message} (OK Button: {args.OkBtn}, Wait: {args.Wait}) -> OK");
                    args.Result = true; // Simulate user clicking OK
                };

                client.CallbackHandler.TelSendQueueSize += (sender, args) =>
                {
                    args.Result = -1; // Simulate no queue
                };

                client.CallbackHandler.ServiceInitialized += async (sender, hostLogDir) =>
                {
                    Console.WriteLine($"Service initialized. Host log directory: {hostLogDir}");
                    if (client.RpcService != null)
                    {
                        string logFile = Path.Combine(hostLogDir, "PsdzClient.log");
                        Console.WriteLine($"SetupLog4Net with log file: {logFile}");
                        bool result = await client.RpcService.SetupLog4Net(logFile);
                        Console.WriteLine($"SetupLog4Net result: {result}");
                    }
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
                    string istaFolder = await client.RpcService.GetIstaInstallLocation();
                    Console.WriteLine($"ISTA Install location: {istaFolder}");
                    string remoteHost = "127.0.0.1";
                    PrintOptions();

                    for (;;)
                    {
                        bool exitLoop = false;
                        if (Console.KeyAvailable)
                        {
                            bool active = await client.RpcService.OperationActive();
                            if (active)
                            {
                                Console.WriteLine("An operation is currently active. Please wait...");
                                continue;
                            }

                            ConsoleKeyInfo key = Console.ReadKey(intercept: true);
                            switch (key.Key)
                            {
                                case ConsoleKey.C:
                                {
                                    Console.WriteLine($"Connecting vehicle: {remoteHost}...");
                                    bool result = await client.RpcService.ConnectVehicle(istaFolder, remoteHost, false);
                                    Console.WriteLine($"Connect = {result}");
                                    break;
                                }

                                case ConsoleKey.D:
                                {
                                    Console.WriteLine("Disconnecting vehicle...");
                                    bool result = await client.RpcService.DisconnectVehicle();
                                    Console.WriteLine($"Disconnect = {result}");
                                    break;
                                }

                                case ConsoleKey.S:
                                {
                                    Console.WriteLine("Stopping programming service...");
                                    bool result = await client.RpcService.StopProgrammingService(istaFolder);
                                    Console.WriteLine($"Stop Programming Service = {result}");
                                    break;
                                }

                                case ConsoleKey.O:
                                {
                                    Console.WriteLine("Creating options...");
                                    bool result = await client.RpcService.VehicleFunctions(ProgrammingJobs.OperationType.CreateOptions);
                                    Console.WriteLine($"Create Options = {result}");
                                    break;
                                }

                                case ConsoleKey.T:
                                {
                                    Console.WriteLine("Building TAL...");
                                    bool result = await client.RpcService.VehicleFunctions(ProgrammingJobs.OperationType.BuildTalModFa);
                                    Console.WriteLine($"Build TAL = {result}");
                                    break;
                                }

                                case ConsoleKey.E:
                                {
                                    Console.WriteLine("Executing TAL...");
                                    bool result = await client.RpcService.VehicleFunctions(ProgrammingJobs.OperationType.ExecuteTal);
                                    Console.WriteLine($"Execute TAL = {result}");
                                    break;
                                }

                                case ConsoleKey.A:
                                {
                                    Console.WriteLine("Aborting operation...");
                                    await client.RpcService.CancelOperation();
                                    break;
                                }

                                case ConsoleKey.Escape:
                                    Console.WriteLine("ESC pressed, stopping client...");
                                    exitLoop = true;
                                    break;
                            }

                            if (!exitLoop)
                            {
                                PrintOptions();
                            }
                        }

                        if (exitLoop)
                        {
                            break;
                        }
                    }
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

        private static void PrintOptions()
        {
            Console.WriteLine();
            Console.WriteLine("Available commands:");
            Console.WriteLine("C - Connect Vehicle");
            Console.WriteLine("D - Disconnect Vehicle");
            Console.WriteLine("S - Stop Programming Service");
            Console.WriteLine("O - Create Options");
            Console.WriteLine("T - Build TAL");
            Console.WriteLine("E - Execute TAL");
            Console.WriteLine("A - Abort Operation");
            Console.WriteLine("ESC - Exit Client");
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
