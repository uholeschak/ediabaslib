using CommandLine;
using PsdzRpcServer.Shared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EdiabasLib;

namespace PsdzRpcClient
{
    internal class Program
    {
        public class Options
        {
            public Options()
            {
                VehicleIp = string.Empty;
                ServerExe = string.Empty;
                Verbosity = VerbosityOption.Important;
            }

            public enum VerbosityOption
            {
                None,
                Error,
                Important,
                Warning,
                Info,
                Debug
            }

            [Option('i', "vehicleip", Required = false, HelpText = "Vehicle IP address.")]
            public string VehicleIp { get; set; }

            [Option('o', "vehicleproxy", Required = false, HelpText = "Enable vehicle proxy.")]
            public bool VehicleProxy { get; set; }

            [Option('s', "serverexe", Required = false, HelpText = "Server executable path.")]
            public string ServerExe { get; set; }
            
            [Option('u', "username", Required = false, HelpText = "Username for server authentication.")]
            public string UserName { get; set; }

            [Option('p', "password", Required = false, HelpText = "Password for server authentication.")]
            public string Password { get; set; }

            [Option('v', "verbosity", Required = false, HelpText = "Option for message verbosity (Error, Warning, Info, Debug)")]
            public VerbosityOption Verbosity { get; set; }
        }

        static PsdzRpcSwiRegisterEnum selectedRegisterEnum = PsdzRpcSwiRegisterEnum.VehicleModificationCodingConversion;
        static Options.VerbosityOption _verbosity = Options.VerbosityOption.Important;
        private static EdiabasProxyClient _ediabasProxyClient;

        static async Task<int> Main(string[] args)
        {
#if NET
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
#endif
            string vehicleIp = string.Empty;
            bool vehicleProxy = false;
            string serverExe = string.Empty;
            string userName = string.Empty;
            string password = string.Empty;
            bool hasErrors = false;

            Parser parser = new Parser(with =>
            {
                //ignore case for enum values
                with.CaseInsensitiveEnumValues = true;
                with.EnableDashDash = true;
                with.HelpWriter = Console.Out;
            });

            parser.ParseArguments<Options>(args)
                .WithParsed<Options>(o =>
                {
                    vehicleIp = o.VehicleIp;
                    vehicleProxy = o.VehicleProxy;
                    serverExe = o.ServerExe;
                    userName = o.UserName;
                    password = o.Password;
                    _verbosity = o.Verbosity;
                })
                .WithNotParsed(errs =>
                {
                    string errors = string.Join("\n", errs);
                    Console.WriteLine("Option parsing errors:\n{0}", string.Join("\n", errors));
                    if (errors.IndexOf("BadFormatConversion", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        Console.WriteLine("Valid verbosity options are: {0}", string.Join(", ", Enum.GetNames(typeof(Options.VerbosityOption)).ToList()));
                    }

                    hasErrors = true;
                });

            if (hasErrors)
            {
                return 1;
            }

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
                using PsdzRpcClient client = new PsdzRpcClient(Console.Out);

                client.CallbackHandler.StartProgrammingCompleted += (s, success) =>
                {
                    syncContext.BeginInvoke(() =>
                    {
                        if (success)
                        {
                            if (_verbosity >= Options.VerbosityOption.Important)
                            {
                                Console.WriteLine("Start programming operation completed successfully.");
                            }
                        }
                        else
                        {
                            if (_verbosity >= Options.VerbosityOption.Error)
                            {
                                Console.WriteLine("Start programming operation failed.");
                            }
                        }
                    });
                };

                client.CallbackHandler.StopProgrammingCompleted += (s, success) =>
                {
                    syncContext.BeginInvoke(() =>
                    {
                        if (success)
                        {
                            if (_verbosity >= Options.VerbosityOption.Important)
                            {
                                Console.WriteLine("Stop programming operation completed successfully.");
                            }
                        }
                        else
                        {
                            if (_verbosity >= Options.VerbosityOption.Error)
                            {
                                Console.WriteLine("Stop programming operation failed.");
                            }
                        }
                    });
                };

                client.CallbackHandler.ConnectVehicleCompleted += (o, connectArgs) =>
                {
                    syncContext.BeginInvoke(() =>
                    {
                        if (connectArgs.Success)
                        {
                            if (_verbosity >= Options.VerbosityOption.Important)
                            {
                                Console.WriteLine($"Vehicle connected, Vin: {connectArgs.Vin}");
                            }
                        }
                        else
                        {
                            if (_verbosity >= Options.VerbosityOption.Error)
                            {
                                Console.WriteLine("Failed to connect vehicle.");
                            }
                        }
                    });
                };

                client.CallbackHandler.DisconnectVehicleCompleted += (sender, success) =>
                {
                    syncContext.BeginInvoke(() =>
                    {
                        if (success)
                        {
                            if (_verbosity >= Options.VerbosityOption.Important)
                            {
                                Console.WriteLine("Disconnect vehicle completed successfully.");
                            }
                        }
                        else
                        {
                            if (_verbosity >= Options.VerbosityOption.Error)
                            {
                                Console.WriteLine("Disconnect vehicle operation failed.");
                            }
                        }
                    });
                };

                client.CallbackHandler.VehicleFunctionsCompleted += (s, vehicleArgs) =>
                {
                    syncContext.BeginInvoke(() =>
                    {
                        if (vehicleArgs.Success)
                        {
                            if (_verbosity >= Options.VerbosityOption.Important)
                            {
                                Console.WriteLine($"Vehicle function {vehicleArgs.OperationType} completed successfully.");
                            }
                        }
                        else
                        {
                            if (_verbosity >= Options.VerbosityOption.Error)
                            {
                                Console.WriteLine($"Vehicle function {vehicleArgs.OperationType} failed.");
                            }
                        }
                    });
                };


                client.CallbackHandler.UpdateStatus += (s, e) =>
                {
                    syncContext.BeginInvoke(() =>
                    {
                        if (_verbosity >= Options.VerbosityOption.Important)
                        {
                            Console.WriteLine($"Status: {e}");
                        }
                    });
                };

                client.CallbackHandler.UpdateProgress += (sender, progressArgs) =>
                {
                    syncContext.BeginInvoke(() =>
                    {
                        if (progressArgs.Marquee)
                        {
                            if (_verbosity >= Options.VerbosityOption.Important)
                            {
                                if (string.IsNullOrEmpty(progressArgs.Message))
                                {
                                    Console.WriteLine("Processing ...");
                                }
                                else
                                {
                                    Console.WriteLine($"Progress: {progressArgs.Message}");
                                }
                            }
                        }
                        else
                        {
                            if (_verbosity >= Options.VerbosityOption.Important)
                            {
                                Console.WriteLine($"[{progressArgs.Percent}%]: {progressArgs.Message}");
                            }
                        }
                    });
                };

                client.CallbackHandler.UpdateOptions += (sender, optionArgs) =>
                {
                    syncContext.BeginInvoke(async () =>
                    {
                        Console.WriteLine("Options updated");
                        await PrintOptionTypes(client).ConfigureAwait(false);
                    });
                };

                client.CallbackHandler.UpdateOptionSelections += (sender, swiRegisterEnum) =>
                {
                    syncContext.BeginInvoke(() =>
                    {
                        if (_verbosity >= Options.VerbosityOption.Important)
                        {
                            Console.WriteLine($"Option selections updated: {swiRegisterEnum}");
                        }
                    });
                };

                client.CallbackHandler.ShowMessage += (sender, msgArgs) =>
                {
                    syncContext.BeginInvoke(() =>
                    {
                        if (_verbosity >= Options.VerbosityOption.Important)
                        {
                            Console.WriteLine($"Message: {msgArgs.Message}");
                        }
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
                    queueArgs.Result = -1; // Simulate no queue
                };

                client.CallbackHandler.ServiceInitialized += (sender, serviceArgs) =>
                {
                    syncContext.BeginInvoke(async () =>
                    {
                        if (_verbosity >= Options.VerbosityOption.Important)
                        {
                            Console.WriteLine($"Service initialized. Host log directory: {serviceArgs.HostLogDir}, Logging initialized: {serviceArgs.LoggingInitialized}");
                        }
                        if (client.RpcService != null && !serviceArgs.LoggingInitialized)
                        {
                            string logFile = Path.Combine(serviceArgs.HostLogDir, "PsdzClient.log");
                            if (_verbosity >= Options.VerbosityOption.Important)
                            {
                                Console.WriteLine($"SetupLog4Net with log file: {logFile}");
                            }

                            bool result = await client.RpcService.SetupLog4Net(logFile).ConfigureAwait(false);
                            if (_verbosity >= Options.VerbosityOption.Important)
                            {
                                Console.WriteLine($"SetupLog4Net result: {result}");
                            }

                            bool resetResult = await client.RpcService.ResetStarterGuard().ConfigureAwait(false);
                            if (_verbosity >= Options.VerbosityOption.Important)
                            {
                                Console.WriteLine($"ResetStarterGuard result: {resetResult}");
                            }
                        }
                    });
                };

                client.CallbackHandler.VehicleConnect += (sender, id) =>
                {
                    if (_ediabasProxyClient == null)
                    {
                        syncContext.BeginInvoke(() =>
                        {
                            if (_verbosity >= Options.VerbosityOption.Error)
                            {
                                Console.WriteLine("Ediabas proxy client is not initialized.");
                            }
                        });
                        return;
                    }

                    _ediabasProxyClient.VehicleConnect(id);
                };

                client.CallbackHandler.VehicleDisconnect += (sender, id) =>
                {
                    if (_ediabasProxyClient == null)
                    {
                        syncContext.BeginInvoke(() =>
                        {
                            if (_verbosity >= Options.VerbosityOption.Error)
                            {
                                Console.WriteLine("Ediabas proxy client is not initialized.");
                            }
                        });
                        return;
                    }

                    _ediabasProxyClient?.VehicleDisconnect(id);
                };

                client.CallbackHandler.VehicleSend += (sender, sendArgs) =>
                {
                    if (_ediabasProxyClient == null)
                    {
                        syncContext.BeginInvoke(() =>
                        {
                            if (_verbosity >= Options.VerbosityOption.Error)
                            {
                                Console.WriteLine("Ediabas proxy client is not initialized.");
                            }
                        });
                        return;
                    }

                    _ediabasProxyClient?.VehicleSend(sendArgs.Id, sendArgs.Data);
                };

                if (string.IsNullOrEmpty(serverExe) || !File.Exists(serverExe))
                {
                    serverExe = PsdzRpcServerStarter.DetectServerLocation();
                }

                if (string.IsNullOrEmpty(serverExe))
                {
                    if (_verbosity >= Options.VerbosityOption.Error)
                    {
                        Console.WriteLine("Server executable not found.");
                    }

                    return 1;
                }

                if (_verbosity >= Options.VerbosityOption.Important)
                {
                    Console.WriteLine($"Using server executable: {serverExe}");
                }

                PsdzRpcServerStarter serverStarter = new(Console.Out);
                bool connected = await serverStarter.ConnectClient(serverExe, null, ProcessWindowStyle.Minimized, client, cts, userName, password).ConfigureAwait(false);
                if (!connected)
                {
                    if (_verbosity >= Options.VerbosityOption.Error)
                    {
                        Console.WriteLine("Failed to connect to RPC server.");
                    }
                    return 1;
                }

                if (client.RpcService != null)
                {
                    string istaFolder = await client.RpcService.GetIstaInstallLocation().ConfigureAwait(false);
                    if (string.IsNullOrEmpty(istaFolder))
                    {
                        if (_verbosity >= Options.VerbosityOption.Error)
                        {
                            Console.WriteLine("Failed to get ISTA install location.");
                        }
                        return 1;
                    }

                    if (_verbosity >= Options.VerbosityOption.Important)
                    {
                        Console.WriteLine($"ISTA Install location: {istaFolder}");
                    }

                    bool licenseResult = await client.RpcService.SetLicenseValid(true).ConfigureAwait(false);
                    if (!licenseResult)
                    {
                        if (_verbosity >= Options.VerbosityOption.Error)
                        {
                            Console.WriteLine("Failed to set license valid.");
                        }
                        return 1;
                    }

                    Console.WriteLine("License set to valid.");

                    string remoteHost = vehicleIp;
                    if (string.IsNullOrEmpty(vehicleIp))
                    {
                        remoteHost = vehicleProxy ? EdInterfaceEnet.AutoIp + EdInterfaceEnet.AutoIpAll : "127.0.0.1";
                    }

                    if (_verbosity >= Options.VerbosityOption.Important)
                    {
                        Console.WriteLine($"Using vehicle IP: {remoteHost}");
                    }

                    if (vehicleProxy)
                    {
                        EdiabasNet ediabasNet = EdiabasSetup(remoteHost);
                        _ediabasProxyClient = new EdiabasProxyClient(ediabasNet);
                        _ediabasProxyClient.VehicleResponseEvent += (vehicleResponse) =>
                        {
                            return Task.Run(() => client.RpcService.SetVehicleResponse(vehicleResponse)).GetAwaiter().GetResult();
                        };

                        _ediabasProxyClient.ErrorMessageEvent += (message) =>
                        {
                            syncContext.BeginInvoke(() =>
                            {
                                if (_verbosity >= Options.VerbosityOption.Error)
                                {
                                    Console.WriteLine($"Proxy error: {message}");
                                }
                            });
                        };

                        _ediabasProxyClient.InfoMessageEvent += (message) =>
                        {
                            syncContext.BeginInvoke(() =>
                            {
                                if (_verbosity >= Options.VerbosityOption.Info)
                                {
                                    Console.WriteLine($"Proxy info: {message}");
                                }
                            });
                        };

                        _ediabasProxyClient.StartEdiabasThread();

                        bool proxyResult = await client.RpcService.EnableVehicleProxy().ConfigureAwait(false);
                        if (!proxyResult)
                        {
                            if (_verbosity >= Options.VerbosityOption.Error)
                            {
                                Console.WriteLine("Failed to enable vehicle proxy.");
                            }
                            return 1;
                        }
                    }

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
                            bool active = await client.RpcService.OperationActive().ConfigureAwait(false);
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
                                    bool result = await client.RpcService.ConnectVehicle(istaFolder, remoteHost, false).ConfigureAwait(false);
                                    Console.WriteLine($"Connect = {result}");
                                    break;
                                }

                                case ConsoleKey.D:
                                {
                                    Console.WriteLine("Disconnecting vehicle...");
                                    bool result = await client.RpcService.DisconnectVehicle().ConfigureAwait(false);
                                    Console.WriteLine($"Disconnect = {result}");
                                    break;
                                }

                                case ConsoleKey.S:
                                {
                                    Console.WriteLine("Stopping programming service...");
                                    bool result = await client.RpcService.StopProgrammingService(istaFolder).ConfigureAwait(false);
                                    Console.WriteLine($"Stop Programming Service = {result}");
                                    break;
                                }

                                case ConsoleKey.O:
                                {
                                    Console.WriteLine("Creating options...");
                                    bool result = await client.RpcService.VehicleFunctions(PsdzOperationType.CreateOptions).ConfigureAwait(false);
                                    Console.WriteLine($"Create Options = {result}");
                                    break;
                                }

                                case ConsoleKey.Y:
                                {
                                    await PrintOptionTypes(client).ConfigureAwait(false);
                                    Console.WriteLine("Enter option index:");
                                    string line = Console.ReadLine();
                                    if (int.TryParse(line, out int index))
                                    {
                                        bool result = await SelectOptionType(client, index).ConfigureAwait(false);
                                        Console.WriteLine($"Select Option = {result}");
                                        if (result)
                                        {
                                            await PrintOptionTypes(client).ConfigureAwait(false);
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
                                    await PrintSelectedOptions(client).ConfigureAwait(false);
                                    break;
                                }

                                case ConsoleKey.M:
                                {
                                    await PrintSelectedOptions(client).ConfigureAwait(false);
                                    Console.WriteLine("Enter option index:");
                                    string line = Console.ReadLine();
                                    if (int.TryParse(line, out int index))
                                    {
                                        bool result = await ModifyOption(client, index).ConfigureAwait(false);
                                        Console.WriteLine($"Modify Option = {result}");
                                        if (result)
                                        {
                                            await PrintSelectedOptions(client).ConfigureAwait(false);
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
                                    bool result = await client.RpcService.VehicleFunctions(PsdzOperationType.BuildTalModFa).ConfigureAwait(false);
                                    Console.WriteLine($"Build TAL = {result}");
                                    break;
                                }

                                case ConsoleKey.E:
                                {
                                    Console.WriteLine("Executing TAL...");
                                    bool result = await client.RpcService.VehicleFunctions(PsdzOperationType.ExecuteTal).ConfigureAwait(false);
                                    Console.WriteLine($"Execute TAL = {result}");
                                    break;
                                }

                                case ConsoleKey.A:
                                {
                                    Console.WriteLine("Aborting operation...");
                                    await client.RpcService.CancelOperation().ConfigureAwait(false);
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

            if (_ediabasProxyClient != null)
            {
                _ediabasProxyClient.StopEdiabasThread();
                _ediabasProxyClient.Dispose();
                _ediabasProxyClient = null;
            }

            Console.WriteLine("Client stopped.");
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
            List<PsdzRpcOptionType> optionTypes = await client.RpcService.GetOptionTypes().ConfigureAwait(false);
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
            List<PsdzRpcOptionType> optionTypes = await client.RpcService.GetOptionTypes().ConfigureAwait(false);
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
            List<PsdzRpcOptionItem> selectedOptions = await client.RpcService.GetSelectedOptions(selectedRegisterEnum).ConfigureAwait(false);
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
            List<PsdzRpcOptionItem> selectedOptions = await client.RpcService.GetSelectedOptions(selectedRegisterEnum).ConfigureAwait(false);
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
                await Task.Delay(100, ct).ConfigureAwait(false);
            }
        }

        private static EdiabasNet EdiabasSetup(string vehicleIp)
        {
            EdInterfaceEnet edInterfaceEnet = new EdInterfaceEnet(false);
            EdiabasNet ediabas = new EdiabasNet
            {
                EdInterfaceClass = edInterfaceEnet,
            };
            edInterfaceEnet.RemoteHost = vehicleIp;
            edInterfaceEnet.VehicleProtocol = EdInterfaceEnet.ProtocolHsfz;
            edInterfaceEnet.IcomAllocate = false;

            return ediabas;
        }
    }
}
