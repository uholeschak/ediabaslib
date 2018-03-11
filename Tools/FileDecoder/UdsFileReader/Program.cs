using System;
using System.Collections.Generic;
using System.Globalization;
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
                List<string> includeFiles = udsReader.GetFileList(fileName);
                if (includeFiles == null)
                {
                    Console.WriteLine("Get file list failed");
                    return 1;
                }

                Console.WriteLine("Includes:");
                foreach (string includeFile in includeFiles)
                {
                    Console.WriteLine(includeFile);
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
                    sb.Insert(0, "*** ");
                    Console.WriteLine(sb.ToString());

                    if (parseInfo is UdsReader.ParseInfoMwb parseInfoMwb)
                    {
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

                        sb.Clear();
                        sb.Append(string.Format(CultureInfo.InvariantCulture, "Service ID: {0:X04}", parseInfoMwb.ServiceId));
                        sb.Append(string.Format(CultureInfo.InvariantCulture, "; Data type: {0}", parseInfoMwb.DataTypeId));

                        if (parseInfoMwb.ScaleOffset.HasValue)
                        {
                            sb.Append(string.Format(CultureInfo.InvariantCulture, "; Offset: {0}", parseInfoMwb.ScaleOffset.Value));
                        }

                        if (parseInfoMwb.ScaleMult.HasValue)
                        {
                            sb.Append(string.Format(CultureInfo.InvariantCulture, "; Mult: {0}", parseInfoMwb.ScaleMult.Value));
                        }

                        if (parseInfoMwb.ScaleDiv.HasValue)
                        {
                            sb.Append(string.Format(CultureInfo.InvariantCulture, "; Div: {0}", parseInfoMwb.ScaleDiv.Value));
                        }

                        if (parseInfoMwb.ByteOffset.HasValue)
                        {
                            sb.Append(string.Format(CultureInfo.InvariantCulture, "; Byte: {0}", parseInfoMwb.ByteOffset.Value));
                        }

                        if (parseInfoMwb.BitOffset.HasValue)
                        {
                            sb.Append(string.Format(CultureInfo.InvariantCulture, "; Bit: {0}", parseInfoMwb.BitOffset.Value));
                        }

                        if (parseInfoMwb.BitLength.HasValue)
                        {
                            sb.Append(string.Format(CultureInfo.InvariantCulture, "; Len: {0}", parseInfoMwb.BitLength.Value));
                        }

                        Console.WriteLine(sb.ToString());

                        if (parseInfoMwb.NameValueList != null)
                        {
                            foreach (UdsReader.ValueName valueName in parseInfoMwb.NameValueList)
                            {
                                sb.Clear();

                                foreach (string entry in valueName.LineArray)
                                {
                                    if (sb.Length > 0)
                                    {
                                        sb.Append("; ");
                                    }
                                    sb.Append("\"");
                                    sb.Append(entry);
                                    sb.Append("\"");
                                }

                                if (valueName.NameArray != null)
                                {
                                    sb.Append(": ");
                                    foreach (string nameEntry in valueName.NameArray)
                                    {
                                        sb.Append("\"");
                                        sb.Append(nameEntry);
                                        sb.Append("\" ");
                                    }
                                }

                                sb.Insert(0, "Value Name: ");
                                Console.WriteLine(sb.ToString());
                            }
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
