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

                List<UdsReader.ParseInfoBase> resultList = udsReader.ExtractFileSegment(includeFiles, UdsReader.SegmentType.Mwb);
                if (resultList == null)
                {
                    Console.WriteLine("Parsing failed");
                    return 1;
                }

                Console.WriteLine("MWB:");
                foreach (UdsReader.ParseInfoBase parseInfo in resultList)
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (string entry in parseInfo.LineArray)
                    {
                        if (sb.Length > 0)
                        {
                            sb.Append("; ");
                        }
                        sb.Append("\"");
                        sb.Append(entry);
                        sb.Append("\"");
                    }
                    sb.Insert(0, "Raw: ");
                    Console.WriteLine(sb.ToString());

                    if (parseInfo is UdsReader.ParseInfoMwb parseInfoMwb)
                    {
                        Console.WriteLine("Service ID: {0:X04}", parseInfoMwb.ServiceId);

                        sb.Clear();
                        foreach (string entry in parseInfoMwb.NameArray)
                        {
                            if (sb.Length > 0)
                            {
                                sb.Append("; ");
                            }
                            sb.Append("\"");
                            sb.Append(entry);
                            sb.Append("\"");
                        }
                        sb.Insert(0, "Name: ");
                        Console.WriteLine(sb.ToString());

                        if (parseInfoMwb.NameDetailArray != null)
                        {
                            sb.Clear();
                            foreach (string entry in parseInfoMwb.NameDetailArray)
                            {
                                if (sb.Length > 0)
                                {
                                    sb.Append("; ");
                                }
                                sb.Append("\"");
                                sb.Append(entry);
                                sb.Append("\"");
                            }

                            sb.Insert(0, "Name Detail: ");
                            Console.WriteLine(sb.ToString());
                        }
                    }
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
