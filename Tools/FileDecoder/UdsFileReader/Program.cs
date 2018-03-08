using System;
using System.Collections.Generic;
using System.Linq;
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

            List<string> fileList = args.ToList();

            List<string> includeFiles = new List<string>();
            if (UdsReader.GetIncludeFiles(fileList[0], includeFiles))
            {
                Console.WriteLine("Includes:");
                foreach (string includeFile in includeFiles)
                {
                    Console.WriteLine(includeFile);
                }
            }

            List<string[]> lineList = UdsReader.ExtractFileSegment(fileList, "MWB");

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
    }
}
