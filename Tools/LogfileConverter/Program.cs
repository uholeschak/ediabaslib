using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using NDesk.Options;

namespace LogfileConverter
{
    class Program
    {
        static bool _responseFile;
        static bool _cFormat;

        static int Main(string[] args)
        {
            bool sortFile = false;
            bool showHelp = false;
            List<string> inputFiles = new List<string>();
            string outputFile = null;

            var p = new OptionSet()
            {
                { "i|input=", "input file.",
                  v => inputFiles.Add(v) },
                { "o|output=", "output file (if omitted '.conv' is appended to input file).",
                  v => outputFile = v },
                { "c|cformat", "c format for hex values", 
                  v => _cFormat = v != null },
                { "r|response", "create reponse file", 
                  v => _responseFile = v != null },
                { "s|sort", "sort reponse file", 
                  v => sortFile = v != null },
                { "h|help",  "show this message and exit", 
                  v => showHelp = v != null },
            };

            try
            {
                p.Parse(args);
            }
            catch (OptionException e)
            {
                string thisName = Path.GetFileNameWithoutExtension(AppDomain.CurrentDomain.FriendlyName);
                Console.Write(thisName + ": ");
                Console.WriteLine(e.Message);
                Console.WriteLine("Try `" + thisName + " --help' for more information.");
                return 1;
            }

            if (showHelp)
            {
                ShowHelp(p);
                return 0;
            }

            if (inputFiles.Count < 1)
            {
                Console.WriteLine("No input files specified");
                return 1;
            }
            if (outputFile == null)
            {
                outputFile = inputFiles[0] + ".conv";
            }

            foreach (string inputFile in inputFiles)
            {
                if (!File.Exists(inputFile))
                {
                    Console.WriteLine("Input file '{0}' not found", inputFile);
                    return 1;
                }
            }

            if (!ConvertLog(inputFiles, outputFile))
            {
                Console.WriteLine("Conversion failed");
                return 1;
            }
            if (sortFile && _responseFile)
            {
                if (!SortLines(outputFile))
                {
                    Console.WriteLine("Sorting failed");
                    return 1;
                }
            }

            return 0;
        }

        static private bool ConvertLog(List<string> inputFiles, string outputFile)
        {
            try
            {
                using (StreamWriter streamWriter = new StreamWriter(outputFile))
                {
                    foreach (string inputFile in inputFiles)
                    {
                        if (string.Compare(Path.GetExtension(inputFile), ".trc", StringComparison.OrdinalIgnoreCase) == 0)
                        {   // trace file
                            ConvertTraceFile(inputFile, streamWriter);
                        }
                        else
                        {
                            ConvertPortLogFile(inputFile, streamWriter);
                        }
                    }
                }
            }
            catch
            {
                return false;
            }
            return true;
        }

        private static void ConvertPortLogFile(string inputFile, StreamWriter streamWriter)
        {
            using (StreamReader streamReader = new StreamReader(inputFile))
            {
                string line;
                string readString = string.Empty;
                string writeString = string.Empty;
                while ((line = streamReader.ReadLine()) != null)
                {
                    if (line.Length > 0)
                    {
                        if (!Regex.IsMatch(line, @"^\[\\\\"))
                        {
                            line = Regex.Replace(line, @"^[\d]+[\s]+[\d\.]+[\s]+[\w\.]+[\s]*", String.Empty);
                            if (Regex.IsMatch(line, @"IRP_MJ_WRITE"))
                            {
                                line = Regex.Replace(line, @"^IRP_MJ_WRITE.*\:[\s]*", String.Empty);
                                List<byte> lineValues = NumberString2List(line);
#if false
                                if ((lineValues.Count > 1) && (lineValues[1] == 0x56))
                                {
                                    line = string.Empty;
                                }
#endif
                                if (line.Length > 0)
                                {
                                    bool validWrite = ChecksumValid(lineValues);
                                    if (_responseFile)
                                    {
                                        if (validWrite)
                                        {
                                            if (writeString.Length > 0 && readString.Length > 0)
                                            {
                                                List<byte> writeValues = NumberString2List(writeString);
                                                List<byte> readValues = NumberString2List(readString);
                                                if (ValidResponse(writeValues, readValues))
                                                {
                                                    streamWriter.Write(NumberString2String(writeString, _responseFile || !_cFormat));
                                                    StoreReadString(streamWriter, readString);
                                                }
                                            }
                                            writeString = NumberString2String(line, _responseFile || !_cFormat);
                                        }
                                        else
                                        {
                                            writeString = string.Empty;
                                        }
                                    }
                                    else
                                    {
                                        StoreReadString(streamWriter, readString);
                                        if (validWrite)
                                        {
                                            line = "w: " + NumberString2String(line, _responseFile || !_cFormat);
                                        }
                                        else
                                        {
                                            line = "w (Invalid): " + NumberString2String(line, _responseFile || !_cFormat);
                                        }
                                    }
                                    readString = string.Empty;
                                }
                            }
                            else if (Regex.IsMatch(line, @"^Length 1:"))
                            {
                                line = Regex.Replace(line, @"^Length 1:[\s]*", String.Empty);
                                readString += line;
                                line = string.Empty;
                            }
                            else
                            {
                                line = string.Empty;
                            }
                            if (!_responseFile && line.Length > 0)
                            {
                                streamWriter.WriteLine(line);
                            }
                        }
                    }
                }
                if (_responseFile)
                {
                    if (writeString.Length > 0 && readString.Length > 0)
                    {
                        List<byte> writeValues = NumberString2List(writeString);
                        List<byte> readValues = NumberString2List(readString);
                        if (ValidResponse(writeValues, readValues))
                        {
                            streamWriter.Write(NumberString2String(writeString, _responseFile || !_cFormat));
                            StoreReadString(streamWriter, readString);
                        }
                    }
                }
                else
                {
                    StoreReadString(streamWriter, readString);
                }
            }
        }

        private static void ConvertTraceFile(string inputFile, StreamWriter streamWriter)
        {
            using (StreamReader streamReader = new StreamReader(inputFile))
            {
                string line;
                string readString = string.Empty;
                string writeString = string.Empty;
                while ((line = streamReader.ReadLine()) != null)
                {
                    if (line.Length > 0)
                    {
                        if (Regex.IsMatch(line, @"^ \((Send|Resp)\):"))
                        {
                            bool send = Regex.IsMatch(line, @"^ \(Send\):");
                            line = Regex.Replace(line, @"^.*\:[\s]*", String.Empty);

                            List<byte> lineValues = NumberString2List(line);
                            if (line.Length > 0)
                            {
                                if (send)
                                {
                                    bool validWrite = ChecksumValid(lineValues);
                                    if (_responseFile)
                                    {
                                        if (validWrite)
                                        {
                                            if (writeString.Length > 0 && readString.Length > 0)
                                            {
                                                List<byte> writeValues = NumberString2List(writeString);
                                                List<byte> readValues = NumberString2List(readString);
                                                if (ValidResponse(writeValues, readValues))
                                                {
                                                    streamWriter.Write(NumberString2String(writeString,
                                                        _responseFile || !_cFormat));
                                                    StoreReadString(streamWriter, readString);
                                                }
                                            }
                                            writeString = NumberString2String(line, _responseFile || !_cFormat);
                                        }
                                        else
                                        {
                                            writeString = string.Empty;
                                        }
                                    }
                                    else
                                    {
                                        StoreReadString(streamWriter, readString);
                                        if (validWrite)
                                        {
                                            line = "w: " + NumberString2String(line, _responseFile || !_cFormat);
                                        }
                                        else
                                        {
                                            line = "w (Invalid): " +
                                                   NumberString2String(line, _responseFile || !_cFormat);
                                        }
                                    }
                                    readString = string.Empty;
                                }
                                else
                                {   // receive
                                    readString += line;
                                    line = string.Empty;
                                }
                            }
                            else
                            {
                                readString = string.Empty;
                            }
                            if (!_responseFile && line.Length > 0)
                            {
                                streamWriter.WriteLine(line);
                            }
                        }
                    }
                }
                if (_responseFile)
                {
                    if (writeString.Length > 0 && readString.Length > 0)
                    {
                        List<byte> writeValues = NumberString2List(writeString);
                        List<byte> readValues = NumberString2List(readString);
                        if (ValidResponse(writeValues, readValues))
                        {
                            streamWriter.Write(NumberString2String(writeString, _responseFile || !_cFormat));
                            StoreReadString(streamWriter, readString);
                        }
                    }
                }
                else
                {
                    StoreReadString(streamWriter, readString);
                }
            }
        }

        private static int LineComparer(string x, string y)
        {
            string lineX = x.Substring(3);
            string lineY = y.Substring(3);

            return String.Compare(lineX, lineY, StringComparison.Ordinal);
        }

        static private bool SortLines(string fileName)
        {
            try
            {
                string[] lines = File.ReadAllLines(fileName);
                Array.Sort(lines, LineComparer);
                using (StreamWriter streamWriter = new StreamWriter(fileName))
                {
                    string lastLine = string.Empty;
                    foreach (string line in lines)
                    {
                        if (line != lastLine)
                        {
                            streamWriter.WriteLine(line);
                        }
                        lastLine = line;
                    }
                }
            }
            catch
            {
                return false;
            }
            return true;
        }

        static private void StoreReadString(StreamWriter streamWriter, string readString)
        {
            try
            {
                if (readString.Length > 0)
                {
                    List<byte> lineValues = NumberString2List(readString);
                    bool valid = ChecksumValid(lineValues);
                    if (_responseFile)
                    {
                        if (valid)
                        {
                            streamWriter.WriteLine(" : " + NumberString2String(readString, _responseFile || !_cFormat));
                        }
                        else
                        {
                            streamWriter.WriteLine();
                        }
                    }
                    else
                    {
                        if (valid)
                        {
                            streamWriter.WriteLine("r: " + NumberString2String(readString, _responseFile || !_cFormat));
                        }
                        else
                        {
                            streamWriter.WriteLine("r (Invalid): " + NumberString2String(readString, _responseFile || !_cFormat));
                        }
                    }
                }
            }
            catch
            {
                // ignored
            }
        }

        static private List<byte> NumberString2List(string numberString)
        {
            List<byte> values = new List<byte>();
            string[] numberArray = numberString.Split(' ');
            foreach (string number in numberArray)
            {
                if (number.Length > 0)
                {
                    try
                    {
                        int value = Convert.ToInt32(number, 16);
                        values.Add((byte) value);
                    }
                    catch
                    {
                        // ignored
                    }
                }
            }
            return values;
        }

        static private bool ChecksumValid(List<byte> telegram)
        {
            int offset = 0;
            for (; ; )
            {
                if (telegram.Count - offset < 4) return false;

                int dataLength = telegram[0 + offset] & 0x3F;
                if (dataLength == 0)
                {   // with length byte
                    dataLength = telegram[3 + offset] + 4;
                }
                else
                {
                    dataLength += 3;
                }
                if (telegram.Count - offset < dataLength + 1)
                {
                    return false;
                }

                byte sum = 0;
                for (int i = 0; i < dataLength; i++)
                {
                    sum += telegram[i + offset];
                }
                if (sum != telegram[dataLength + offset])
                {
                    return false;
                }

                offset += dataLength + 1;    // checksum
                if (offset > telegram.Count)
                {
                    return false;
                }
                if (offset == telegram.Count)
                {
                    break;
                }
            }
            return true;
        }

        static private bool ValidResponse(List<byte> request, List<byte> response)
        {
            bool broadcast = (request[0] & 0xC0) != 0x80;
            if (!ChecksumValid(request) || !ChecksumValid(response))
            {
                return false;
            }
            if (!broadcast)
            {
                if (request[1] != response[2])
                {
                    return false;
                }
                if (request[2] != response[1])
                {
                    return false;
                }
            }
            return true;
        }

        static private string NumberString2String(string numberString, bool simpleFormat)
        {
            string result = string.Empty;

            List<byte> values = NumberString2List(numberString);

            foreach (byte value in values)
            {
                if (simpleFormat)
                {
                    if (result.Length > 0)
                    {
                        result += " ";
                    }
                    result += string.Format("{0:X02}", value);
                }
                else
                {
                    if (result.Length > 0)
                    {
                        result += ", ";
                    }
                    result += string.Format("0x{0:X02}", value);
                }
            }

            return result;
        }

        static void ShowHelp(OptionSet p)
        {
            Console.WriteLine("Usage: " + Path.GetFileNameWithoutExtension(AppDomain.CurrentDomain.FriendlyName) + " [OPTIONS]");
            Console.WriteLine("Convert BMW ODB log files");
            Console.WriteLine();
            Console.WriteLine("Options:");
            p.WriteOptionDescriptions(Console.Out);
        }
    }
}
