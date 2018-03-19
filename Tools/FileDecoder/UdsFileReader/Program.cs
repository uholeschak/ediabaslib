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
            Console.OutputEncoding = Encoding.UTF8;

            if (args.Length < 1)
            {
                Console.WriteLine("No input file specified");
                return 1;
            }

            string fileSpec = args[0];
            string dir = Path.GetDirectoryName(fileSpec);
            string searchPattern = Path.GetFileName(fileSpec);
            if (dir == null || searchPattern == null)
            {
                Console.WriteLine("Invalid file name");
                return 1;
            }

            try
            {
                UdsReader udsReader = new UdsReader();
                if (!udsReader.Init(dir))
                {
                    Console.WriteLine("Init failed");
                    return 1;
                }

                string[] files = Directory.GetFiles(dir, searchPattern, SearchOption.AllDirectories);
                foreach (string file in files)
                {
                    try
                    {
                        string fileExt = Path.GetExtension(file);
                        if (string.Compare(fileExt, UdsReader.FileExtension, StringComparison.OrdinalIgnoreCase) != 0)
                        {
                            continue;
                        }
                        Console.WriteLine("Parsing: {0}", file);
                        string outFile = Path.ChangeExtension(file, ".txt");
                        if (outFile == null)
                        {
                            Console.WriteLine("*** Invalid output file");
                        }
                        else
                        {
                            using (StreamWriter outputStream = new StreamWriter(outFile, false, new UTF8Encoding(true)))
                            {
                                if (!ParseFile(udsReader, file, outputStream))
                                {
                                    Console.WriteLine("*** Parsing failed: {0}", file);
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("*** Exception {0}", e.Message);
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

        static bool ParseFile(UdsReader udsReader, string fileName, StreamWriter outStream)
        {
            try
            {
                List<string> includeFiles = udsReader.GetFileList(fileName);
                if (includeFiles == null)
                {
                    outStream.WriteLine("Get file list failed");
                    return false;
                }

                outStream.WriteLine("Includes:");
                foreach (string includeFile in includeFiles)
                {
                    outStream.WriteLine(includeFile);
                }

                List<UdsReader.ParseInfoBase> resultList = udsReader.ExtractFileSegment(includeFiles, UdsReader.SegmentType.Mwb);
                if (resultList == null)
                {
                    outStream.WriteLine("Parsing failed");
                    return false;
                }

                outStream.WriteLine("MWB:");
                foreach (UdsReader.ParseInfoBase parseInfo in resultList)
                {
                    outStream.WriteLine("");

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
                    outStream.WriteLine(sb.ToString());

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
                        outStream.WriteLine(sb.ToString());
                        outStream.WriteLine(string.Format(CultureInfo.InvariantCulture, "Service ID: {0:X04}", parseInfoMwb.ServiceId));

                        if (!PrintDataTypeEntry(outStream, parseInfoMwb.DataTypeEntry))
                        {
                            return false;
                        }
                    }
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        static bool PrintDataTypeEntry(StreamWriter outStream, UdsReader.DataTypeEntry dataTypeEntry)
        {
            StringBuilder sb = new StringBuilder();
            if (dataTypeEntry.NameDetailArray != null)
            {
                sb.Clear();
                foreach (string entry in dataTypeEntry.NameDetailArray)
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
                outStream.WriteLine(sb.ToString());
            }

            sb.Clear();
            sb.Append(string.Format(CultureInfo.InvariantCulture, "Data type: {0}", UdsReader.DataTypeEntry.DataTypeIdToString(dataTypeEntry.DataTypeId)));

            if (dataTypeEntry.FixedEncoding != null)
            {
                sb.Append(dataTypeEntry.FixedEncoding.ConvertFunc != null ? " (Fixed, Function)" : " (Fixed)");
            }

            if (dataTypeEntry.NumberOfDigits.HasValue)
            {
                sb.Append(string.Format(CultureInfo.InvariantCulture, "; Digits: {0}", dataTypeEntry.NumberOfDigits.Value));
            }

            if (dataTypeEntry.ScaleOffset.HasValue)
            {
                sb.Append(string.Format(CultureInfo.InvariantCulture, "; Offset: {0}", dataTypeEntry.ScaleOffset.Value));
            }

            if (dataTypeEntry.ScaleMult.HasValue)
            {
                sb.Append(string.Format(CultureInfo.InvariantCulture, "; Mult: {0}", dataTypeEntry.ScaleMult.Value));
            }

            if (dataTypeEntry.ScaleDiv.HasValue)
            {
                sb.Append(string.Format(CultureInfo.InvariantCulture, "; Div: {0}", dataTypeEntry.ScaleDiv.Value));
            }

            if (dataTypeEntry.UnitText != null)
            {
                sb.Append(string.Format(CultureInfo.InvariantCulture, "; Unit: \"{0}\"", dataTypeEntry.UnitText));
            }

            if (dataTypeEntry.ByteOffset.HasValue)
            {
                sb.Append(string.Format(CultureInfo.InvariantCulture, "; Byte: {0}", dataTypeEntry.ByteOffset.Value));
            }

            if (dataTypeEntry.BitOffset.HasValue)
            {
                sb.Append(string.Format(CultureInfo.InvariantCulture, "; Bit: {0}", dataTypeEntry.BitOffset.Value));
            }

            if (dataTypeEntry.BitLength.HasValue)
            {
                sb.Append(string.Format(CultureInfo.InvariantCulture, "; Len: {0}", dataTypeEntry.BitLength.Value));
            }

            outStream.WriteLine(sb.ToString());

            if (dataTypeEntry.NameValueList != null)
            {
                foreach (UdsReader.ValueName valueName in dataTypeEntry.NameValueList)
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
                    outStream.WriteLine(sb.ToString());
                }
            }

            if (dataTypeEntry.MuxEntryList != null)
            {
                foreach (UdsReader.MuxEntry muxEntry in dataTypeEntry.MuxEntryList)
                {
                    sb.Clear();

                    foreach (string entry in muxEntry.LineArray)
                    {
                        if (sb.Length > 0)
                        {
                            sb.Append("; ");
                        }
                        sb.Append("\"");
                        sb.Append(entry);
                        sb.Append("\"");
                    }
                    sb.Insert(0, "Mux: ");
                    outStream.WriteLine(sb.ToString());

                    sb.Clear();
                    if (muxEntry.Default)
                    {
                        sb.Append(string.Format(CultureInfo.InvariantCulture, "Default"));
                    }

                    if (muxEntry.MinValue != null)
                    {
                        sb.Append(string.Format(CultureInfo.InvariantCulture, "Min: {0}", muxEntry.MinValue));
                    }

                    if (muxEntry.MaxValue != null)
                    {
                        sb.Append(string.Format(CultureInfo.InvariantCulture, " Max: {0}", muxEntry.MaxValue));
                    }

                    if (sb.Length > 0)
                    {
                        outStream.WriteLine(sb.ToString());
                    }

                    if (muxEntry.DataTypeEntry != null)
                    {
                        if (!PrintDataTypeEntry(outStream, muxEntry.DataTypeEntry))
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }
    }
}
