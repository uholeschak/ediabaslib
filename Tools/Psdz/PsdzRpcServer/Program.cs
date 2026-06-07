using CommandLine;
using PsdzRpcServer.Shared;
using System;
using System.Configuration;
using System.IO;
using System.Linq;
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

            [Option('p', "tcpport", Required = false, HelpText = "Port for TCP server.")]
            public int? TcpPort { get; set; }

            [Option('s', "ssl", Required = false, HelpText = "Enable SSL for TCP server.")]
            public bool Ssl { get; set; }

            [Option('v', "verbosity", Required = false,
                HelpText = "Option for message verbosity (Error, Warning, Info, Debug)")]
            public VerbosityOption Verbosity { get; set; }

            [Option('l', "logfile", Required = false, HelpText = "Logfile for additional console output logging.")]
            public string LogFile { get; set; }
        }

        static Options.VerbosityOption _verbosity = Options.VerbosityOption.Important;
        static bool consoleAvailable = true;

        static async Task<int> Main(string[] args)
        {
#if NET
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
#endif
            PsdzRpcTools.DisableQuickEdit();

            bool keepRunning = false;
            int? tcpPort = null;
            bool ssl = false;
            bool hasErrors = false;
            string logFile = null;

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
                    tcpPort = o.TcpPort;
                    ssl = o.Ssl;
                    _verbosity = o.Verbosity;
                    logFile = o.LogFile;
                })
                .WithNotParsed(errs =>
                {
                    string errors = string.Join("\n", errs);
                    Console.WriteLine("Option parsing errors:\n{0}", string.Join("\n", errors));
                    if (errors.IndexOf("BadFormatConversion", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        Console.WriteLine("Valid verbosity options are: {0}",
                            string.Join(", ", Enum.GetNames(typeof(Options.VerbosityOption)).ToList()));
                    }

                    hasErrors = true;
                });

            if (hasErrors)
            {
                return 1;
            }

            if (!string.IsNullOrEmpty(logFile))
            {
                try
                {
                    StreamWriter fileWriter = new StreamWriter(logFile, append: true, encoding: System.Text.Encoding.UTF8)
                    {
                        AutoFlush = true
                    };
                    Console.SetOut(new TeeTextWriter(Console.Out, fileWriter));
                    Console.SetError(new TeeTextWriter(Console.Error, fileWriter));
                    Console.WriteLine($"Logging to: {logFile}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to open log file '{logFile}': {ex.Message}");
                }
            }

            string sqlServer = ConfigurationManager.AppSettings["SqlServer"];
            if (string.IsNullOrEmpty(sqlServer))
            {
                Console.WriteLine("SQL Server configuration is missing.");
                return 1;
            }

            sqlServer = sqlServer.Trim();

            string testLicensesString = ConfigurationManager.AppSettings["TestLicenses"];
            if (string.IsNullOrEmpty(testLicensesString))
            {
                Console.WriteLine("Test licenses configuration is missing.");
                return 1;
            }

            testLicensesString = testLicensesString.Trim();

            bool testLicenses = Int32.TryParse(testLicensesString, out Int32 testLicensesValueResult) &&
                                testLicensesValueResult != 0;

            string displayOptions = ConfigurationManager.AppSettings["DisplayOptions"];
            if (string.IsNullOrEmpty(displayOptions))
            {
                Console.WriteLine("Display options configuration is missing.");
                return 1;
            }

            displayOptions = displayOptions.Trim();

            consoleAvailable = IsConsoleAvailable();
            Console.WriteLine($"Console available: {consoleAvailable}");
            if (tcpPort.HasValue)
            {
                keepRunning = true;
                if (tcpPort.Value == 0)
                {
                    tcpPort = PsdzRpcServiceConstants.DefaultTcpPort;
                }

                Console.WriteLine($"TCP port: {tcpPort.Value}");
            }

            string appDir = AppDomain.CurrentDomain.BaseDirectory;
            string caCertPath = null;
            string serverPfxPath = null;
            if (ssl)
            {
                Console.WriteLine("SSL enabled");

                string certsPath = Path.Combine(appDir, PsdzRpcServiceConstants.CertDir);
                caCertPath = Path.Combine(certsPath, PsdzRpcServiceConstants.CaCertFile);
                serverPfxPath = Path.Combine(certsPath, PsdzRpcServiceConstants.ServerPfxFile);

                if (File.Exists(caCertPath))
                {
                    Console.WriteLine($"CA certificate found at {caCertPath}");
                }

                if (File.Exists(serverPfxPath))
                {
                    Console.WriteLine($"Server certificate found at {serverPfxPath}");
                }
            }

            PsdzLicenseCheck licenseCheck = null;
            if (string.Compare(sqlServer, "-", StringComparison.OrdinalIgnoreCase) != 0)
            {
                licenseCheck = new PsdzLicenseCheck(sqlServer, testLicenses, displayOptions);
            }

            using CancellationTokenSource cts = new CancellationTokenSource();
            using PsdzRpcServer server = new PsdzRpcServer(PsdzRpcServiceConstants.DealerId, Console.Out, licenseCheck,
                tcpPort, caCertPath, serverPfxPath);
            try
            {
                CancellationTokenSource ctsLocal = cts;
                Console.CancelKeyPress += (_, e) =>
                {
                    e.Cancel = true;
                    ctsLocal.Cancel();
                };

                Console.WriteLine("Starting PsdzJsonRpcServer...");
                if (keepRunning)
                {
                    Console.WriteLine("Server will keep running until ESC is pressed.");
                }
                else
                {
                    Console.WriteLine("Server will stop when the last client disconnects or ESC is pressed.");
                }

                Task serverTask = server.StartAsync(cts.Token);
                _ = serverTask.ContinueWith(t =>
                {
                    if (t.IsFaulted)
                    {
                        Console.WriteLine($"Server error: {t.Exception?.GetBaseException().Message}");
                        cts.Cancel();
                    }
                });

                Task keyTask = WaitForEscapeKeyAsync(cts.Token, server);

                // Beenden bei: ESC, Ctrl+C oder letzter Client getrennt
                if (keepRunning)
                {
                    await Task.WhenAny(serverTask, keyTask).ConfigureAwait(false);
                }
                else
                {
                    await Task.WhenAny(serverTask, keyTask, server.AllClientsDisconnected).ConfigureAwait(false);
                    if (server.AllClientsDisconnected.IsCompleted)
                    {
                        if (_verbosity <= Options.VerbosityOption.Important)
                        {
                            Console.WriteLine("Last client disconnected. Shutting down...");
                        }
                    }
                }

                cts.Cancel();

                await Task.WhenAll(serverTask.ContinueWith(_ => { }), keyTask.ContinueWith(_ => { }))
                    .ConfigureAwait(false);
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

        private static async Task WaitForEscapeKeyAsync(CancellationToken ct, PsdzRpcServer server)
        {
            if (!IsConsoleAvailable())
            {
                await Task.Delay(Timeout.Infinite, ct).ConfigureAwait(false);
                return;
            }

            while (!ct.IsCancellationRequested)
            {
                try
                {
                    if (Console.KeyAvailable)
                    {
                        ConsoleKeyInfo key = Console.ReadKey(intercept: true);
                        if (key.Key == ConsoleKey.Escape)
                        {
                            if (server.ClientCount > 0)
                            {
                                if (_verbosity >= Options.VerbosityOption.Error)
                                {
                                    Console.WriteLine($"ESC ignored: {server.ClientCount} client(s) still connected.");
                                }
                            }
                            else
                            {
                                if (_verbosity >= Options.VerbosityOption.Important)
                                {
                                    Console.WriteLine("ESC pressed, stopping server...");
                                }

                                return;
                            }
                        }
                    }
                }
                catch (InvalidOperationException)
                {
                    await Task.Delay(Timeout.Infinite, ct).ConfigureAwait(false);
                    return;
                }

                await Task.Delay(100, ct).ConfigureAwait(false);
            }
        }

        private static bool IsConsoleAvailable()
        {
            try
            {
                _ = Console.KeyAvailable;
                return true;
            }
            catch (InvalidOperationException)
            {
                return false;
            }
        }

        /// <summary>
        /// Schreibt gleichzeitig in zwei TextWriter (z.B. Konsole + Datei).
        /// </summary>
        private class TeeTextWriter : TextWriter
        {
            private readonly TextWriter _first;
            private readonly TextWriter _second;

            public TeeTextWriter(TextWriter first, TextWriter second)
            {
                _first = first;
                _second = second;
            }

            public override System.Text.Encoding Encoding => _first.Encoding;

            public override void Write(char value)
            {
                _first.Write(value);
                _second.Write(value);
            }

            public override void Write(string value)
            {
                _first.Write(value);
                _second.Write(value);
            }

            public override void WriteLine(string value)
            {
                _first.WriteLine(value);
                _second.WriteLine(value);
            }

            public override void Flush()
            {
                _first.Flush();
                _second.Flush();
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    _second.Dispose(); // nur Datei-Writer schließen, Console nicht
                }
                base.Dispose(disposing);
            }
        }
    }
}