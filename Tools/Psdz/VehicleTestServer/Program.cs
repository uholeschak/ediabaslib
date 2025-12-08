using CommandLine;
using EdiabasLib;
using System;
using System.IO;
using System.Text;
using System.Threading;

namespace VehicleTestServer
{
    class Program
    {
        public class Options
        {
            public Options()
            {
                VehicleIp = string.Empty;
            }

            [Option('v', "vehicle", Required = false, HelpText = "Vehicle IP, default is auto:all")]
            public string VehicleIp { get; set; }
        }

        static int Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            TextWriter outWriter = Console.Out;
            string vehicleIp = null;
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
                })
                .WithNotParsed(errs =>
                {
                    string errors = string.Join("\n", errs);
                    outWriter.WriteLine("Option parsing errors:\n{0}", string.Join("\n", errors));

                    hasErrors = true;
                });

            if (hasErrors)
            {
                return 1;
            }

            if (string.IsNullOrEmpty(vehicleIp))
            {
                vehicleIp = EdInterfaceEnet.AutoIp + EdInterfaceEnet.AutoIpAll;
            }
            outWriter.WriteLine("Vehicle IP: {0}", vehicleIp);

            EdiabasNet ediabas = EdiabasSetup(vehicleIp);
            EdWebServer edWebServer = new EdWebServer(ediabas, message =>
            {
                outWriter.WriteLine(message);
            });
            edWebServer.StartTcpListener("http://127.0.0.1:8080");
            outWriter.WriteLine("Press ESC to stop");
            do
            {
                while (!Console.KeyAvailable)
                {
                    Thread.Sleep(100);
                }
            }
            while (Console.ReadKey(true).Key != ConsoleKey.Escape);

            edWebServer.Dispose();
            return 0;
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
