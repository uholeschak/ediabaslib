using PsdzClient;
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
        static PsdzRpcSwiRegisterEnum selectedRegisterEnum = PsdzRpcSwiRegisterEnum.VehicleModificationCodingConversion;

        static async Task<int> Main(string[] args)
        {
#if NET
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
#endif
            ShowMessageEventArgs pendingMessage = null;
            using CancellationTokenSource cts = new CancellationTokenSource();

            try
            {
                CancellationTokenSource ctsLocal = cts;
                Console.CancelKeyPress += (_, e) =>
                {
                    e.Cancel = true;
                    ctsLocal.Cancel();
                };

                SingleThreadSynchronizationContext syncContext = new();

                await using PsdzRpcClient client = new PsdzRpcClient();
                client.CallbackHandler.ProgressChanged += (s, e) =>
                {
                    syncContext.BeginInvoke(() =>
                    {
                        Console.WriteLine($"[{e.Percent}%] {e.Message}");
                    });
                };

                client.CallbackHandler.OperationCompleted += (s, success) =>
                {
                    syncContext.BeginInvoke(() =>
                    {
                        Console.WriteLine($"Operation finished: {(success ? "Success" : "Error")}");
                    });
                };

                client.CallbackHandler.UpdateStatus += (s, e) =>
                {
                    syncContext.BeginInvoke(() =>
                    {
                        Console.WriteLine($"Status: {e}");
                    });
                };

                client.CallbackHandler.UpdateProgress += (sender, tuple) =>
                {
                    syncContext.BeginInvoke(() =>
                    {
                        Console.WriteLine($"[{tuple.percent}%] Marquee={tuple.marquee}: {tuple.message}");
                    });
                };

                client.CallbackHandler.UpdateOptions += (sender, optionArgs) =>
                {
                    syncContext.BeginInvoke(async () =>
                    {
                        Console.WriteLine("Options updated");
                        await PrintOptionTypes(client);
                    });
                };

                client.CallbackHandler.UpdateOptionSelections += (sender, swiRegisterEnum) =>
                {
                    syncContext.BeginInvoke(() =>
                    {
                        Console.WriteLine($"Option selections updated: {swiRegisterEnum}");
                    });
                };

                client.CallbackHandler.ShowMessage += (sender, msgArgs) =>
                {
                    syncContext.BeginInvoke(() =>
                    {
                        Console.WriteLine($"Message: {msgArgs.Message}");
                    });

                    msgArgs.Result = true;
                };

                client.CallbackHandler.ShowMessageWait += (sender, msgArgs) =>
                {
                    syncContext.BeginInvoke(() =>
                    {
                        Console.WriteLine($"Message: {msgArgs.Message}");
                        PrintMessageAction(msgArgs);
                        pendingMessage = msgArgs;
                    });
                };

                client.CallbackHandler.TelSendQueueSize += (sender, queueArgs) =>
                {
                    syncContext.BeginInvoke(() =>
                    {
                        queueArgs.Result = -1; // Simulate no queue
                    });
                };

                client.CallbackHandler.ServiceInitialized += (sender, hostLogDir) =>
                {
                    syncContext.BeginInvoke(async () =>
                    {
                        Console.WriteLine($"Service initialized. Host log directory: {hostLogDir}");
                        if (client.RpcService != null)
                        {
                            string logFile = Path.Combine(hostLogDir, "PsdzClient.log");
                            Console.WriteLine($"SetupLog4Net with log file: {logFile}");
                            bool result = await client.RpcService.SetupLog4Net(logFile);
                            Console.WriteLine($"SetupLog4Net result: {result}");
                        }
                    });
                };

                Console.WriteLine("Starting PsdzJsonRpcClient...");
                Task clientTask = client.ConnectAsync(null, cts.Token);
                Task keyTask = WaitForEscapeKeyAsync(cts.Token);

                await Task.WhenAny(clientTask, keyTask);

                cts.Cancel();

                await Task.WhenAll(
                    clientTask.ContinueWith(_ => { }),
                    keyTask.ContinueWith(_ => { }));

                if (client.RpcService != null)
                {
                    string istaFolder = await client.RpcService.GetIstaInstallLocation();
                    if (string.IsNullOrEmpty(istaFolder))
                    {
                        Console.WriteLine("Failed to get ISTA install location.");
                        return 1;
                    }

                    Console.WriteLine($"ISTA Install location: {istaFolder}");
                    bool licenseResult = await client.RpcService.SetLicenseValid(true);
                    if (!licenseResult)
                    {
                        Console.WriteLine("Failed to set license valid.");
                        return 1;
                    }

                    Console.WriteLine("License set to valid.");
                    string remoteHost = "127.0.0.1";
                    PrintOptions();

                    for (;;)
                    {
                        bool exitLoop = false;

                        // Ausstehende Callbacks auf dem Main-Thread verarbeiten
                        syncContext.ProcessPendingCallbacks();

                        if (Console.KeyAvailable)
                        {
                            // Zuerst prüfen ob eine Nachricht auf Eingabe wartet
                            if (pendingMessage != null)
                            {
                                bool? result = null;
                                ConsoleKeyInfo key = Console.ReadKey(intercept: true);
                                if (pendingMessage.OkBtn)
                                {
                                    switch (key.Key)
                                    {
                                        case ConsoleKey.Enter:
                                            result = true;
                                            break;
                                    }
                                }
                                else
                                {
                                    switch (key.Key)
                                    {
                                        case ConsoleKey.Y:
                                            result = true;
                                            break;
                                        case ConsoleKey.N:
                                            result = false;
                                            break;
                                    }
                                }

                                if (result != null)
                                {
                                    pendingMessage.SetResult(result.Value);
                                    pendingMessage = null;
                                    PrintOptions();
                                }
                                else
                                {
                                    PrintMessageAction(pendingMessage);
                                }

                                continue;
                            }

                            ConsoleKeyInfo cmdKey = Console.ReadKey(intercept: true);
                            bool active = await client.RpcService.OperationActive();
                            if (active)
                            {
                                Console.WriteLine("An operation is currently active. Please wait...");
                                continue;
                            }

                            switch (cmdKey.Key)
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
                                    bool result = await client.RpcService.VehicleFunctions(PsdzOperationType.CreateOptions);
                                    Console.WriteLine($"Create Options = {result}");
                                    break;
                                }

                                case ConsoleKey.Y:
                                {
                                    await PrintOptionTypes(client);
                                    Console.WriteLine("Enter option index:");
                                    string line = Console.ReadLine();
                                    if (int.TryParse(line, out int index))
                                    {
                                        bool result = await SelectOptionType(client, index);
                                        Console.WriteLine($"Select Option = {result}");
                                        if (result)
                                        {
                                            await PrintOptionTypes(client);
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine("Invalid index.");
                                    }
                                    break;
                                }

                                case ConsoleKey.P:
                                {
                                    await PrintSelectedOptions(client);
                                    break;
                                }

                                case ConsoleKey.M:
                                {
                                    await PrintSelectedOptions(client);
                                    Console.WriteLine("Enter option index:");
                                    string line = Console.ReadLine();
                                    if (int.TryParse(line, out int index))
                                    {
                                        bool result = await ModifyOption(client, index);
                                        Console.WriteLine($"Modify Option = {result}");
                                        if (result)
                                        {
                                            await PrintSelectedOptions(client);
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine("Invalid index.");
                                    }
                                    break;
                                }

                                case ConsoleKey.T:
                                {
                                    Console.WriteLine("Building TAL...");
                                    bool result = await client.RpcService.VehicleFunctions(PsdzOperationType.BuildTalModFa);
                                    Console.WriteLine($"Build TAL = {result}");
                                    break;
                                }

                                case ConsoleKey.E:
                                {
                                    Console.WriteLine("Executing TAL...");
                                    bool result = await client.RpcService.VehicleFunctions(PsdzOperationType.ExecuteTal);
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
                        else
                        {
                            // reduce CPU load when idle
                            Thread.Sleep(10);
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
            //Console.WriteLine("Press any key to exit...");
            //Console.ReadKey();
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
            Console.WriteLine("Y - Select Option Type");
            Console.WriteLine("P - Print Selected Options");
            Console.WriteLine("M - Modify Option");
            Console.WriteLine("T - Build TAL");
            Console.WriteLine("E - Execute TAL");
            Console.WriteLine("A - Abort Operation");
            Console.WriteLine("ESC - Exit Client");
        }

        private static void PrintMessageAction(ShowMessageEventArgs pendingMessage)
        {
            if (pendingMessage == null)
            {
                return;
            }

            if (pendingMessage.OkBtn)
            {
                Console.WriteLine("Press Enter to continue...");
            }
            else
            {
                Console.WriteLine("Press Y (Yes) or N (No)...");
            }

        }

        private static async Task PrintOptionTypes(PsdzRpcClient client)
        {
            List<PsdzRpcOptionType> optionTypes = await client.RpcService.GetOptionTypes();
            if (optionTypes != null)
            {
                Console.WriteLine("Available option types:");
                int index = 0;
                foreach (PsdzRpcOptionType option in optionTypes)
                {
                    bool selected = option.SwiRegisterEnum == selectedRegisterEnum;
                    string selectedMarker = selected ? "* " : " ";
                    Console.WriteLine($"- {selectedMarker}{index}: {option.Caption} ({option.SwiRegisterEnum.ToString()})");
                    index++;
                }
            }
        }

        private static async Task<bool> SelectOptionType(PsdzRpcClient client, int index)
        {
            List<PsdzRpcOptionType> optionTypes = await client.RpcService.GetOptionTypes();
            if (optionTypes == null)
            {
                Console.WriteLine("No option types available.");
                return false;
            }

            if (index < 0 || index >= optionTypes.Count)
            {
                Console.WriteLine($"Invalid option type index: {index}");
                return false;
            }

            PsdzRpcOptionType selectedOption = optionTypes[index];
            selectedRegisterEnum = selectedOption.SwiRegisterEnum;
            Console.WriteLine($"Selected option type: {selectedOption.Caption} ({selectedRegisterEnum})");
            return true;
        }

        private static async Task PrintSelectedOptions(PsdzRpcClient client)
        {
            List<PsdzRpcOptionItem> selectedOptions = await client.RpcService.GetSelectedOptions(selectedRegisterEnum);
            if (selectedOptions != null)
            {
                Console.WriteLine("Selected options for CodingConversion:");
                int index = 0;
                foreach (PsdzRpcOptionItem item in selectedOptions)
                {
                    Console.WriteLine($"- {index}: {item.Caption} ({item.SwiRegisterEnum.ToString()}) - Enabled: {item.Enabled}, Selected: {item.Selected}");
                    index++;
                }
            }
        }

        private static async Task<bool> ModifyOption(PsdzRpcClient client, int index)
        {
            List<PsdzRpcOptionItem> selectedOptions = await client.RpcService.GetSelectedOptions(selectedRegisterEnum);
            if (selectedOptions == null)
            {
                Console.WriteLine("No options available.");
                return false;
            }

            if (index < 0 || index >= selectedOptions.Count)
            {
                Console.WriteLine($"Invalid option index: {index}");
                return false;
            }

            PsdzRpcOptionItem option = selectedOptions[index];
            if (!option.Enabled)
            {
                Console.WriteLine($"Option '{option.Caption}' is not enabled and cannot be selected.");
                return false;
            }

            bool select = !option.Selected;
            Console.WriteLine($"{(select ? "Selecting" : "Deselecting")} option: {option.Caption}...");
            bool result = await client.RpcService.SelectOption(option, select);
            if (!result)
            {
                Console.WriteLine($"Failed to configure option: {option.Caption}");
                return false;
            }

            Console.WriteLine($"Option successfully configured: '{option.Caption}'.");
            return true;
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
