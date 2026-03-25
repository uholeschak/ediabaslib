using CommandLine;
using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PsdzRpcServer
{
    internal class Program
    {
        public class Options
        {
            public Options()
            {
                KeepRunning = false;
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

            [Option('r', "keeprunning", Required = false, HelpText = "Keep running on client disconnect.")]
            public bool KeepRunning { get; set; }

            [Option('v', "verbosity", Required = false, HelpText = "Option for message verbosity (Error, Warning, Info, Debug)")]
            public VerbosityOption Verbosity { get; set; }
        }

        static Options.VerbosityOption _verbosity = Options.VerbosityOption.Important;

        static async Task<int> Main(string[] args)
        {
#if NET
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
#endif
            bool keepRunning = false;
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
                    keepRunning = o.KeepRunning;
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

            using CancellationTokenSource cts = new CancellationTokenSource();
            PsdzRpcServer server = new PsdzRpcServer(Console.Out);

            try
            {
                CancellationTokenSource ctsLocal = cts;
                Console.CancelKeyPress += (_, e) =>
                {
                    e.Cancel = true;
                    ctsLocal.Cancel();
                };

                Console.WriteLine("Starting PsdzJsonRpcServer...");
                Console.WriteLine("Server will stop when the last client disconnects or ESC is pressed.");
                Task serverTask = server.StartAsync(cts.Token);
                Task keyTask = WaitForEscapeKeyAsync(cts.Token);

                // Beenden bei: ESC, Ctrl+C oder letzter Client getrennt
                if (keepRunning)
                {
                    await Task.WhenAny(serverTask, keyTask);
                }
                else
                {
                    await Task.WhenAny(serverTask, keyTask, server.AllClientsDisconnected);
                    if (server.AllClientsDisconnected.IsCompleted)
                    {
                        if (_verbosity <= Options.VerbosityOption.Important)
                        {
                            Console.WriteLine("Last client disconnected. Shutting down...");
                        }
                    }
                }

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

            if (_verbosity <= Options.VerbosityOption.Important)
            {
                Console.WriteLine("PsdzJsonRpcServer stopped.");
            }
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
                        if (_verbosity <= Options.VerbosityOption.Important)
                        {
                            Console.WriteLine("ESC pressed, stopping server...");
                        }
                        return;
                    }
                }
                await Task.Delay(100, ct);
            }
        }
    }
}
