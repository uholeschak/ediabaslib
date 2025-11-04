using CommandLine;
using System;
using System.IO;
using System.Linq;

namespace SourceCodeSync
{
    internal class Program
    {
        public class Options
        {
            public Options()
            {
                SourceDir = string.Empty;
                DestDir = string.Empty;
            }

            [Option('s', "sourcedir", Required = true, HelpText = "Source directory.")]
            public string SourceDir { get; set; }

            [Option('d', "destdir", Required = true, HelpText = "Destination directory.")]
            public string DestDir { get; set; }
        }

        static int Main(string[] args)
        {
            try
            {
                string sourceDir = null;
                string destDir = null;
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
                        sourceDir = o.SourceDir;
                        destDir = o.DestDir;
                    })
                    .WithNotParsed(errs =>
                    {
                        string errors = string.Join("\n", errs);
                        Console.WriteLine("Option parsing errors:\n{0}", string.Join("\n", errors));

                        hasErrors = true;
                    });

                if (hasErrors)
                {
                    return 1;
                }

                if (string.IsNullOrEmpty(sourceDir) || !Directory.Exists(sourceDir))
                {
                    Console.WriteLine("Source directory not existing: {0}", sourceDir);
                    return 1;
                }

                if (string.IsNullOrEmpty(destDir) || !Directory.Exists(destDir))
                {
                    Console.WriteLine("Destination directory not existing: {0}", destDir);
                    return 1;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("*** Exception: {0}", e.Message);
                return 1;
            }

            return 0;
        }
    }
}
