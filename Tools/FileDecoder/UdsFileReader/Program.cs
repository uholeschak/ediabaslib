using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace UdsFileReader
{
    static class Program
    {
        static int Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("No input file specified");
                return 1;
            }

            try
            {
                string fileName = args[0];

                UdsReader udsReader = new UdsReader();
                if (!udsReader.Init(Path.GetDirectoryName(fileName)))
                {
                    Console.WriteLine("Init failed");
                    return 1;
                }
                List<string> includeFiles = new List<string>
                {
                    fileName
                };
                if (UdsReader.GetIncludeFiles(fileName, includeFiles))
                {
                    Console.WriteLine("Includes:");
                    foreach (string includeFile in includeFiles)
                    {
                        Console.WriteLine(includeFile);
                    }
                }

                List<string[]> lineList = UdsReader.ExtractFileSegment(includeFiles, "MWB");

                Console.WriteLine("MWB:");
                foreach (string[] line in lineList)
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (string entry in line)
                    {
                        if (sb.Length > 0)
                        {
                            sb.Append("; ");
                        }
                        sb.Append("\"");
                        sb.Append(entry);
                        sb.Append("\"");
                    }
                    Console.WriteLine(sb.ToString());
                }

                return 0;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return 1;
            }
        }
    }
}
