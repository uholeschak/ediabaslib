using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using NDesk.Options;
using UdsFileReader;

namespace LogfileConverter
{
    class Program
    {
        private static bool _responseFile;
        private static bool _cFormat;
        private static bool _ignoreCrcErrors;
        private static bool _ds2Mode;
        private static bool _kwp2000sMode;
        private static bool _edicCanMode;
        private static bool _edicCanIsoTpMode;
        private static int _edicCanAddr;
        private static int _edicCanTesterAddr;
        private static int _edicCanEcuAddr;

        private enum SimFormat
        {
            None,
            BmwFast,
            Kwp2000s_Ds2,
            Edic,
        }

        [Flags]
        private enum EdicTypes
        {
            None = 0x00,
            Kwp1281 = 0x01,
            Kwp2000 = 0x02,
            Tp20 = 0x04,
            Uds = 0x08,
        }

        private class SimEntry(string key, string request, string response, int? ecuAddr = null, bool keyByte = false)
        {
            public string Key { get; set; } = key;
            public string Request { get; private set; } = request;
            public string Response { get; private set; } = response;
            public int? EcuAddr { get; private set; } = ecuAddr;
            public bool KeyByte { get; private set; } = keyByte;
        }

        private class SimData(string[] request, string[] response, List<SimData> addData = null, int? ecuAddr = null) : IEquatable<SimData>
        {
            public string[] Request { get; private set; } = request;
            public string[] Response { get; private set; } = response;
            public List<SimData> AddData { get; private set; } = addData;
            public int? EcuAddr { get; private set; } = ecuAddr;

            private int? hashCode;

            public override bool Equals(object obj)
            {
                SimData simData = obj as SimData;
                if ((object)simData == null)
                {
                    return false;
                }

                return Equals(simData);
            }

            public bool Equals(SimData simData)
            {
                if (Request == null || (object)simData == null || simData.Request == null)
                {
                    return false;
                }

                if (!Request.SequenceEqual(simData.Request))
                {
                    return false;
                }

                if (Response != null && simData.Response != null)
                {
                    if (!Response.SequenceEqual(simData.Response))
                    {
                        return false;
                    }
                }
                else if (Response != null || simData.Response != null)
                {
                    return false;
                }

                if (EcuAddr != simData.EcuAddr)
                {
                    return false;
                }

                return true;
            }

            public override int GetHashCode()
            {
                // ReSharper disable NonReadonlyMemberInGetHashCode
                if (!hashCode.HasValue)
                {
                    hashCode = Request.GetHashCode();
                    if (Response != null)
                    {
                        hashCode ^= Response.GetHashCode();
                    }
                    if (EcuAddr.HasValue)
                    {
                        hashCode ^= EcuAddr.Value;
                    }
                }

                return hashCode.Value;
                // ReSharper restore NonReadonlyMemberInGetHashCode
            }

            public static bool operator ==(SimData lhs, SimData rhs)
            {
                if ((object)lhs == null || (object)rhs == null)
                {
                    return Object.Equals(lhs, rhs);
                }

                return lhs.Equals(rhs);
            }

            public static bool operator !=(SimData lhs, SimData rhs)
            {
                if ((object)lhs == null || (object)rhs == null)
                {
                    return !Object.Equals(lhs, rhs);
                }

                return !(lhs == rhs);
            }
        }

        static int Main(string[] args)
        {
            bool sortFile = false;
            string simFile = null;
            string sFormat = null;
            bool showHelp = false;
            List<string> inputFiles = new List<string>();
            List<string> mergeFiles = new List<string>();
            string outputFile = null;

            var p = new OptionSet()
            {
                { "i|input=", "input file.",
                  v => inputFiles.Add(v) },
                { "m|merge=", "response merge file.",
                    v => mergeFiles.Add(v) },
                { "o|output=", "output file (if omitted '.conv' is appended to input file).",
                  v => outputFile = v },
                { "c|cformat", "c++ style format for hex values", 
                  v => _cFormat = v != null },
                { "r|response", "create response file", 
                  v => _responseFile = v != null },
                { "sim=", "EDIABAS simulation file",
                    v => simFile = v },
                { "sformat=", "simulation format (bmw_fast, ds2, edic)",
                    v => sFormat = v },
                { "s|sort", "sort response file", 
                  v => sortFile = v != null },
                { "e|errors", "ignore CRC errors",
                  v => _ignoreCrcErrors = v != null },
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

            bool tempOutput = false;
            try
            {
                tempOutput = outputFile == null && inputFiles.Count == 0 && mergeFiles.Count > 0;
                if (tempOutput)
                {
                    string firstMergeFile = mergeFiles[0];
                    if (!File.Exists(firstMergeFile))
                    {
                        Console.WriteLine("Merge file '{0}' not found", firstMergeFile);
                        return 1;
                    }

                    string tempFile = Path.GetTempFileName();
                    File.Copy(firstMergeFile, tempFile, true);
                    outputFile = tempFile;
                    mergeFiles.RemoveAt(0);
                }
                else
                {
                    if (outputFile == null)
                    {
                        if (inputFiles.Count < 1)
                        {
                            Console.WriteLine("No input or output file specified");
                            return 1;
                        }

                        outputFile = inputFiles[0] + ".conv";
                    }
                }

                if (!string.IsNullOrEmpty(simFile))
                {   // force response file format
                    _responseFile = true;
                    _cFormat = false;
                }

                SimFormat simFormat = SimFormat.None;
                if (!string.IsNullOrEmpty(sFormat))
                {
                    switch (sFormat.Trim().ToLowerInvariant())
                    {
                        case "bmw_fast":
                            simFormat = SimFormat.BmwFast;
                            break;

                        case "ds2":
                            simFormat = SimFormat.Kwp2000s_Ds2;
                            break;

                        case "edic":
                            simFormat = SimFormat.Edic;
                            break;
                    }
                }

                foreach (string inputFile in inputFiles)
                {
                    if (!File.Exists(inputFile))
                    {
                        Console.WriteLine("Input file '{0}' not found", inputFile);
                        return 1;
                    }
                }

                foreach (string mergeFile in mergeFiles)
                {
                    if (!File.Exists(mergeFile))
                    {
                        Console.WriteLine("Merge file '{0}' not found", mergeFile);
                        return 1;
                    }
                }

                if (inputFiles.Count > 0)
                {
                    if (!ConvertLog(inputFiles, outputFile))
                    {
                        Console.WriteLine("Conversion failed");
                        return 1;
                    }
                }

                if (mergeFiles.Count > 0)
                {
                    if (!AddMergeFiles(mergeFiles, outputFile))
                    {
                        Console.WriteLine("Adding merge files failed");
                        return 1;
                    }
                }

                if (_responseFile && !_cFormat)
                {
                    if (sortFile && string.IsNullOrEmpty(simFile))
                    {
                        if (!SortLines(outputFile))
                        {
                            Console.WriteLine("Sorting failed");
                            return 1;
                        }
                    }

                    if (!string.IsNullOrEmpty(simFile))
                    {
                        if (simFormat == SimFormat.None)
                        {
                            if (_ds2Mode)
                            {
                                simFormat = SimFormat.Kwp2000s_Ds2;
                            }
                        }

                        if (!CreateSimFile(outputFile, simFile, simFormat))
                        {
                            Console.WriteLine("Create sim file failed");
                            return 1;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: {0}", e.Message);
            }
            finally
            {
                if (tempOutput && string.IsNullOrEmpty(outputFile))
                {
                    if (File.Exists(outputFile))
                    {
                        File.Delete(outputFile);
                    }
                }
            }

            return 0;
        }

        private static bool AddMergeFiles(List<string> mergeFiles, string outputFile)
        {
            if (mergeFiles.Count == 0)
            {
                return true;
            }

            try
            {
                using (Stream outputStream = new FileStream(outputFile, FileMode.Append, FileAccess.Write, FileShare.None))
                {
                    foreach (string mergeFile in mergeFiles)
                    {
                        if (string.IsNullOrEmpty(mergeFile))
                        {
                            continue;
                        }

                        using (Stream inputStream = File.OpenRead(mergeFile))
                        {
                            inputStream.CopyTo(outputStream);
                        }
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        private static bool ConvertLog(List<string> inputFiles, string outputFile)
        {
            try
            {
                using (StreamWriter streamWriter = new StreamWriter(outputFile))
                {
                    foreach (string inputFile in inputFiles)
                    {
                        if (string.IsNullOrEmpty(inputFile))
                        {
                            continue;
                        }

                        string fileExt = Path.GetExtension(inputFile);
                        if ((string.Compare(fileExt, ".trc", StringComparison.OrdinalIgnoreCase) == 0) ||
                            (string.Compare(fileExt, ".log", StringComparison.OrdinalIgnoreCase) == 0))
                        {   // trace file
                            ConvertTraceFile(inputFile, streamWriter);
                        }
                        else
                        {
                            bool ifhLog = false;
                            bool wireShark = false;
                            using (StreamReader streamReader = new StreamReader(inputFile))
                            {
                                string line = streamReader.ReadLine();
                                if (line != null)
                                {
                                    if (Regex.IsMatch(line, @"^dllStartupIFH"))
                                    {
                                        ifhLog = true;
                                    }
                                    else if (Regex.IsMatch(line, @"^([0-9a-f]{2}){10,}$", RegexOptions.IgnoreCase))
                                    {
                                        wireShark = true;
                                    }
                                }
                            }
                            if (ifhLog)
                            {
                                ConvertIfhlogFile(inputFile, streamWriter);
                            }
                            else if (wireShark)
                            {
                                ConvertWireSharkFile(inputFile, streamWriter);
                            }
                            else
                            {
                                ConvertPortLogFile(inputFile, streamWriter);
                            }
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
            _ds2Mode = false;
            _kwp2000sMode = false;
            _edicCanMode = false;
            _edicCanIsoTpMode = false;
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
            _ds2Mode = false;
            _kwp2000sMode = false;
            _edicCanMode = false;
            _edicCanIsoTpMode = false;
            using (StreamReader streamReader = new StreamReader(inputFile))
            {
                string line;
                string readString = string.Empty;
                string writeString = string.Empty;
                string lastCfgLine = string.Empty;
                while ((line = streamReader.ReadLine()) != null)
                {
                    if (line.Length > 0)
                    {
                        if (Regex.IsMatch(line, @"^\s\(EDIC CommParameter", RegexOptions.IgnoreCase))
                        {
                            _edicCanMode = false;
                            _edicCanIsoTpMode = false;
                        }

                        MatchCollection canEdicMatches = Regex.Matches(line, @"^EDIC CAN: (..), Tester: (..), Ecu: (..)");
                        if (canEdicMatches.Count == 1)
                        {
                            if (canEdicMatches[0].Groups.Count == 4)
                            {
                                try
                                {
                                    _edicCanAddr = Convert.ToInt32(canEdicMatches[0].Groups[1].Value, 16);
                                    _edicCanTesterAddr = Convert.ToInt32(canEdicMatches[0].Groups[2].Value, 16);
                                    _edicCanEcuAddr = Convert.ToInt32(canEdicMatches[0].Groups[3].Value, 16);
                                    _edicCanMode = true;
                                    _edicCanIsoTpMode = false;
                                }
                                catch (Exception)
                                {
                                    // ignored
                                }
                            }
                        }
                        MatchCollection canEdicIsoTpMatches = Regex.Matches(line, @"^EDIC ISO-TP: Tester: (...), Ecu: (...)");
                        if (canEdicIsoTpMatches.Count == 1)
                        {
                            if (canEdicIsoTpMatches[0].Groups.Count == 3)
                            {
                                try
                                {
                                    _edicCanAddr = 0;
                                    _edicCanTesterAddr = Convert.ToInt32(canEdicIsoTpMatches[0].Groups[1].Value, 16);
                                    _edicCanEcuAddr = Convert.ToInt32(canEdicIsoTpMatches[0].Groups[2].Value, 16);
                                    _edicCanMode = false;
                                    _edicCanIsoTpMode = true;
                                }
                                catch (Exception)
                                {
                                    // ignored
                                }
                            }
                        }

                        MatchCollection canEdicIsoTpEcuOverrideMatches = Regex.Matches(line, @"^Overriding UDS ECU CAN ID with (....)");
                        if (canEdicIsoTpEcuOverrideMatches.Count == 1)
                        {
                            if (canEdicIsoTpEcuOverrideMatches[0].Groups.Count == 2)
                            {
                                try
                                {
                                    _edicCanEcuAddr = Convert.ToInt32(canEdicIsoTpEcuOverrideMatches[0].Groups[1].Value, 16);
                                    _edicCanMode = false;
                                    _edicCanIsoTpMode = true;
                                }
                                catch (Exception)
                                {
                                    // ignored
                                }
                            }
                        }

                        MatchCollection canEdicIsoTpTesterOverrideMatches = Regex.Matches(line, @"^Overriding UDS tester CAN ID with (....)");
                        if (canEdicIsoTpTesterOverrideMatches.Count == 1)
                        {
                            if (canEdicIsoTpTesterOverrideMatches[0].Groups.Count == 2)
                            {
                                try
                                {
                                    _edicCanTesterAddr = Convert.ToInt32(canEdicIsoTpTesterOverrideMatches[0].Groups[1].Value, 16);
                                    _edicCanMode = false;
                                    _edicCanIsoTpMode = true;
                                }
                                catch (Exception)
                                {
                                    // ignored
                                }
                            }
                        }

                        if (Regex.IsMatch(line, @"^ \((Send|Resp)\):"))
                        {
                            bool send = Regex.IsMatch(line, @"^ \(Send\):");
                            line = Regex.Replace(line, @"^.*\:[\s]*", String.Empty);

                            List<byte> lineValues = NumberString2List(line);
                            if (line.Length > 0)
                            {
                                if (send)
                                {
                                    string cfgLineWrite = null;
                                    if (_edicCanIsoTpMode)
                                    {
                                        if (_responseFile)
                                        {
                                            int deviceAddress = -1;
                                            foreach (VehicleInfoVag.EcuAddressEntry ecuAddressEntry in VehicleInfoVag.EcuAddressArray)
                                            {
                                                if (ecuAddressEntry.IsoTpEcuCanId == _edicCanEcuAddr && ecuAddressEntry.IsoTpTesterCanId == _edicCanTesterAddr)
                                                {
                                                    deviceAddress = (int)ecuAddressEntry.Address;
                                                    break;
                                                }
                                            }

                                            if (deviceAddress >= 0)
                                            {
                                                string cfgLine = $"CFG: {deviceAddress:X02} {(_edicCanEcuAddr >> 8):X02} {(_edicCanEcuAddr & 0xFF):X02} {(_edicCanTesterAddr >> 8):X02} {(_edicCanTesterAddr & 0xFF):X02}";
                                                if (string.Compare(lastCfgLine, cfgLine, StringComparison.Ordinal) != 0)
                                                {
                                                    lastCfgLine = cfgLine;
                                                    cfgLineWrite = cfgLine;
                                                }
                                            }
                                        }

                                        // convert to KWP2000 format
                                        int dataLength = lineValues.Count;
                                        if (dataLength < 0x3F)
                                        {
                                            lineValues.Insert(0, (byte) (0x80 + dataLength));
                                            lineValues.Insert(1, (byte)(_edicCanEcuAddr >> 8));
                                            lineValues.Insert(2, (byte)(_edicCanEcuAddr & 0xFF));
                                        }
                                        else
                                        {
                                            lineValues.Insert(0, 0x80);
                                            lineValues.Insert(1, (byte)(_edicCanEcuAddr >> 8));
                                            lineValues.Insert(2, (byte)(_edicCanEcuAddr & 0xFF));
                                            lineValues.Insert(3, (byte) dataLength);
                                        }
                                        byte checksum = CalcChecksumBmwFast(lineValues, 0, lineValues.Count);
                                        lineValues.Add(checksum);
                                        line = List2NumberString(lineValues);
                                    }
                                    else
                                    {
                                        if (_edicCanMode)
                                        {
                                            string cfgLine = $"CFG: {_edicCanAddr:X02} {_edicCanEcuAddr:X02}";
                                            if (string.Compare(lastCfgLine, cfgLine, StringComparison.Ordinal) != 0)
                                            {
                                                lastCfgLine = cfgLine;
                                                cfgLineWrite = cfgLine;
                                            }
                                        }

                                        List<byte> lineConv = ConvertBmwTelegram(lineValues);
                                        if (lineConv != null)
                                        {
                                            lineValues = lineConv;
                                        }
                                    }

                                    int sendLength = TelLengthBmwFast(lineValues, 0);
                                    if (sendLength > 0 && sendLength == lineValues.Count)
                                    {
                                        // checksum missing
                                        byte checksum = CalcChecksumBmwFast(lineValues, 0, lineValues.Count);
                                        lineValues.Add(checksum);
                                        line += $" {checksum:X02}";
                                    }
                                    bool validWrite = ChecksumValid(lineValues);
                                    if (_responseFile)
                                    {
                                        if (validWrite)
                                        {
                                            if (writeString.Length > 0 && readString.Length > 0)
                                            {
                                                List<byte> writeValues = NumberString2List(writeString);
                                                List<byte> writeConv = ConvertBmwTelegram(writeValues);
                                                if (writeConv != null)
                                                {
                                                    writeValues = writeConv;
                                                }

                                                List<byte> readValues = NumberString2List(readString);
                                                List<byte> readConv = ConvertBmwTelegram(readValues);
                                                if (readConv != null)
                                                {
                                                    readValues = readConv;
                                                }

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

                                        if (!string.IsNullOrEmpty(cfgLineWrite))
                                        {
                                            streamWriter.WriteLine(cfgLineWrite);
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
                                    bool addResponse = true;
                                    if (_edicCanMode)
                                    {
                                        if (lineValues.Count == 6 && lineValues[1] == 0xF1 && lineValues[2] == 0xF1)
                                        {   // filter adapter responses
                                            addResponse = false;
                                        }
                                    }
                                    if (_edicCanIsoTpMode)
                                    {
                                        addResponse = false;
                                        if (lineValues.Count >= 4 && lineValues[0] == 0x01)
                                        {   // standard response
                                            int dataLength = (lineValues[1] << 8) + lineValues[2];
                                            if (dataLength + 4 == lineValues.Count)
                                            {
                                                addResponse = true;
                                                // convert to KWP2000 format
                                                lineValues.RemoveAt(lineValues.Count - 1);
                                                lineValues.RemoveAt(0);
                                                lineValues.RemoveAt(0);
                                                lineValues.RemoveAt(0);

                                                if (dataLength < 0x3F)
                                                {
                                                    lineValues.Insert(0, (byte)(0x80 + dataLength));
                                                    lineValues.Insert(1, (byte)(_edicCanEcuAddr >> 8));
                                                    lineValues.Insert(2, (byte)(_edicCanEcuAddr & 0xFF));
                                                }
                                                else
                                                {
                                                    lineValues.Insert(0, 0x80);
                                                    lineValues.Insert(1, (byte)(_edicCanEcuAddr >> 8));
                                                    lineValues.Insert(2, (byte)(_edicCanEcuAddr & 0xFF));
                                                    lineValues.Insert(3, (byte)dataLength);
                                                }
                                                byte checksum = CalcChecksumBmwFast(lineValues, 0, lineValues.Count);
                                                lineValues.Add(checksum);
                                                line = List2NumberString(lineValues);
                                            }
                                        }
                                    }
                                    if (addResponse)
                                    {
                                        readString += line;
                                    }
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
                        List<byte> writeConv = ConvertBmwTelegram(writeValues);
                        if (writeConv != null)
                        {
                            writeValues = writeConv;
                        }

                        List<byte> readValues = NumberString2List(readString);
                        List<byte> readConv = ConvertBmwTelegram(readValues);
                        if (readConv != null)
                        {
                            readValues = readConv;
                        }

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

        private static void ConvertIfhlogFile(string inputFile, StreamWriter streamWriter)
        {
            _ds2Mode = false;
            _kwp2000sMode = false;
            _edicCanMode = false;
            _edicCanIsoTpMode = false;
            using (StreamReader streamReader = new StreamReader(inputFile))
            {
                Regex regexCleanLine = new Regex(@"^.*\:[\s]*");
                string line;
                string writeString = string.Empty;
                bool ignoreResponse = false;
                bool keyBytes = false;
                bool kwp1281 = false;
                int remoteAddr = -1;
                int wakeAddrPar = -1;
                string lastCfgLine = string.Empty;
                List<byte> lineValuesPar = null;
                List<byte> lineValuesPreface = null;
                while ((line = streamReader.ReadLine()) != null)
                {
                    if (line.Length > 0)
                    {
                        if (Regex.IsMatch(line, @"^msgIn:"))
                        {
                            if (Regex.IsMatch(line, @"^.*'ifhRequestKeyBytes"))
                            {
                                keyBytes = true;
                            }
                            if (Regex.IsMatch(line, @"^.*'ifhSetParameter"))
                            {
                                //kwp1281 = false;
                                remoteAddr = -1;
                            }
                            if (!Regex.IsMatch(line, @"^.*('ifhSendTelegram'|'ifhGetResult')"))
                            {
                                ignoreResponse = true;
                                writeString = string.Empty;
                            }
                        }
                        if (Regex.IsMatch(line, @"^\((ifhSetParameter|ifhSetTelPreface)\): "))
                        {
                            bool par = Regex.IsMatch(line, @"^\(ifhSetParameter\): ");
                            line = regexCleanLine.Replace(line, String.Empty);
                            List<byte> lineValues = NumberString2List(line);
                            if (par)
                            {
                                lineValuesPar = lineValues;
                            }
                            else
                            {
                                lineValuesPreface = lineValues;
                            }
                            continue;
                        }
                        if (Regex.IsMatch(line, @"^\((ifhSendTelegram|ifhGetResult)\): "))
                        {
                            bool send = Regex.IsMatch(line, @"^\(ifhSendTelegram\): ");
                            line = regexCleanLine.Replace(line, String.Empty);

                            List<byte> lineValues = NumberString2List(line);
                            if (send && lineValues.Count == 0)
                            {
                                if (lineValuesPar?.Count >= 6 && lineValuesPreface?.Count >= 4 &&
                                    lineValuesPar[4] == 0x81 && lineValuesPreface[2] == 0x02 && lineValuesPreface[3] == 0x00)
                                {
                                    byte wakeAddress = (byte)(lineValuesPar[5] & 0x7F);
                                    bool oddParity = true;
                                    for (int i = 0; i < 7; i++)
                                    {
                                        oddParity ^= (wakeAddress & (1 << i)) != 0;
                                    }
                                    if (oddParity)
                                    {
                                        wakeAddress |= 0x80;
                                    }
                                    wakeAddrPar = wakeAddress;
                                }
                            }
                            if (line.Length > 0)
                            {
                                if (send)
                                {
                                    if (lineValues.Count > 0)
                                    {
                                        if (!kwp1281)
                                        {
                                            lineValues = CreateBmwFastTel(lineValues, 0x00, 0xF1);
                                        }
                                        line = List2NumberString(lineValues);
                                    }
                                    bool validWrite;
                                    // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                                    if (kwp1281)
                                    {
                                        validWrite = CheckKwp1281Tel(lineValues);
                                    }
                                    else
                                    {
                                        validWrite = ChecksumValid(lineValues);
                                    }
                                    if (_responseFile)
                                    {
                                        // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                                        if (validWrite)
                                        {
                                            writeString = NumberString2String(line, _responseFile || !_cFormat);
                                        }
                                        else
                                        {
                                            writeString = string.Empty;
                                        }
                                    }
                                    else
                                    {
                                        if (validWrite)
                                        {
                                            line = "w: " + NumberString2String(line, _responseFile || !_cFormat);
                                        }
                                        else
                                        {
                                            line = "w (Invalid): " + NumberString2String(line, _responseFile || !_cFormat);
                                        }
                                    }
                                }
                                else
                                {   // receive
                                    if (keyBytes)
                                    {
                                        string readString = line;
                                        List<byte> readValues = NumberString2List(readString);
                                        if (readValues.Count >= 5)
                                        {
                                            bool kpw1281Found = readValues[1] != 0x8F;
                                            if (kpw1281Found)
                                            {
                                                kwp1281 = true;
                                            }
                                            if (_responseFile)
                                            {
                                                if (!kpw1281Found)
                                                {
                                                    if (readValues[4] > 0x40)
                                                    {
                                                        // TP20
                                                        if (remoteAddr >= 0)
                                                        {
                                                            string cfgLine = $"CFG: {readValues[2]:X02} {remoteAddr:X02}";
                                                            if (string.Compare(lastCfgLine, cfgLine, StringComparison.Ordinal) != 0)
                                                            {
                                                                streamWriter.WriteLine(cfgLine);
                                                                lastCfgLine = cfgLine;
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        // KWP2000
                                                        string cfgLine = $"CFG: {readValues[2] ^ 0xFF:X02} {readValues[0]:X02} {readValues[1]:X02}";
                                                        if (string.Compare(lastCfgLine, cfgLine, StringComparison.Ordinal) != 0)
                                                        {
                                                            streamWriter.WriteLine(cfgLine);
                                                            lastCfgLine = cfgLine;
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    // KWP1281
                                                    readValues = CleanKwp1281Tel(readValues, true);
                                                    if (wakeAddrPar >= 0 && readValues.Count > 5)
                                                    {
                                                        string cfgLine = $"CFG: {wakeAddrPar:X02} {readValues[0]:X02} {readValues[1]:X02}" +
                                                            "\r\n: " + List2NumberString(readValues.GetRange(5, readValues.Count - 5));

                                                        if (string.Compare(lastCfgLine, cfgLine, StringComparison.Ordinal) != 0)
                                                        {
                                                            streamWriter.WriteLine(cfgLine);
                                                            lastCfgLine = cfgLine;
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                streamWriter.WriteLine("KEY: " + NumberString2String(readString, _responseFile || !_cFormat));
                                            }
                                        }
                                    }
                                    if (!ignoreResponse)
                                    {
                                        string readString = line;
                                        if (_responseFile)
                                        {
                                            if (writeString.Length > 0 && readString.Length > 0)
                                            {
                                                List<byte> writeValues = NumberString2List(writeString);
                                                List<byte> readValues = NumberString2List(readString);
                                                if (kwp1281)
                                                {
                                                    readValues = CleanKwp1281Tel(readValues);
                                                    if (readValues.Count > 0)
                                                    {
                                                        readString = List2NumberString(readValues);
                                                        streamWriter.Write(NumberString2String(writeString, _responseFile || !_cFormat));
                                                        streamWriter.WriteLine(" : " + NumberString2String(readString, _responseFile || !_cFormat));
                                                    }
                                                }
                                                else
                                                {
                                                    if (UpdateRequestAddr(writeValues, readValues))
                                                    {
                                                        remoteAddr = writeValues[1];
                                                        writeString = List2NumberString(writeValues);
                                                        if (ValidResponse(writeValues, readValues))
                                                        {
                                                            streamWriter.Write(NumberString2String(writeString, _responseFile || !_cFormat));
                                                            StoreReadString(streamWriter, readString);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (kwp1281)
                                            {
                                                streamWriter.WriteLine("r: " + NumberString2String(readString, _responseFile || !_cFormat));
                                            }
                                            else
                                            {
                                                StoreReadString(streamWriter, readString);
                                            }
                                        }
                                    }
                                    writeString = string.Empty;
                                    line = string.Empty;
                                }
                            }
                            else
                            {
                                writeString = string.Empty;
                            }
                            if (!_responseFile && line.Length > 0)
                            {
                                streamWriter.WriteLine(line);
                            }
                            ignoreResponse = false;
                            keyBytes = false;
                        }
                    }
                }
            }
        }

        private static void ConvertWireSharkFile(string inputFile, StreamWriter streamWriter)
        {
            List<byte> lastTel = null;
            List<byte> reqTel = null;
            List<List<byte>> respTels = new List<List<byte>>();

            void WriteOutput()
            {
                if (_responseFile)
                {
                    if (respTels.Count > 0)
                    {
                        List<byte> bmwTelReq = CreateEnetBmwFastTel(reqTel);
                        if (bmwTelReq != null)
                        {
                            string line = List2NumberString(bmwTelReq);
                            line += ": ";
                            foreach (List<byte> respTel in respTels)
                            {
                                List<byte> bmwTelResp = CreateEnetBmwFastTel(respTel);
                                if (bmwTelResp != null)
                                {
                                    line += List2NumberString(bmwTelResp);
                                }
                            }

                            streamWriter.WriteLine(line);
                        }
                    }
                }
                else
                {
                    List<byte> bmwTelReq = CreateEnetBmwFastTel(reqTel);
                    if (bmwTelReq != null)
                    {
                        streamWriter.WriteLine("w: " + List2NumberString(bmwTelReq));
                    }
                    else
                    {
                        streamWriter.WriteLine("w (Invalid): " + List2NumberString(reqTel));
                    }

                    foreach (List<byte> respTel in respTels)
                    {
                        List<byte> bmwTelResp = CreateEnetBmwFastTel(respTel);
                        if (bmwTelResp != null)
                        {
                            streamWriter.WriteLine("r: " + List2NumberString(bmwTelResp));
                        }
                        else
                        {
                            streamWriter.WriteLine("r (Invalid): " + List2NumberString(respTel));
                        }
                    }
                }
            }


            using (StreamReader streamReader = new StreamReader(inputFile))
            {
                for (;;)
                {
                    List<byte> telegram = ReadHexStreamTel(streamReader, streamWriter);
                    if (telegram == null)
                    {
                        return;
                    }

                    if (telegram.Count == 0)
                    {
                        break;
                    }

                    if (telegram.Count < 8)
                    {
                        return;
                    }

                    if (telegram[5] == 0xFF)
                    {   //nack
                        continue;
                    }

                    if (telegram[5] == 0x12)
                    {   // alive
                        continue;
                    }

                    if (telegram[5] == 0x02)
                    {   // ack
                        if (reqTel != null)
                        {
                            WriteOutput();

                            reqTel = null;
                            respTels.Clear();
                        }

                        if (lastTel != null)
                        {
                            reqTel = lastTel;
                            respTels.Clear();
                        }

                        lastTel = null;
                        continue;
                    }

                    if (telegram[5] != 0x01)
                    {   // no data
                        if (!_responseFile)
                        {
                            streamWriter.WriteLine("Invalid tel type: {0:X02}", telegram[5]);
                        }
                        return;
                    }

                    if (reqTel != null && lastTel != null)
                    {
                        respTels.Add(lastTel);
                    }
                    lastTel = telegram;
                }
            }

            if (reqTel != null)
            {
                if (lastTel != null)
                {
                    respTels.Add(lastTel);
                }

                WriteOutput();
            }
        }

        private static int LineComparer(string x, string y)
        {
            if (x.Length < 3 || y.Length < 3)
            {
                return 0;
            }

            string lineX = x.Substring(3);
            string lineY = y.Substring(3);

            return string.Compare(lineX, lineY, StringComparison.Ordinal);
        }

        private static bool SortLines(string fileName)
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

        private static bool CreateSimFile(string outputFile, string simFile, SimFormat simFormat)
        {
            try
            {
                if (!File.Exists(outputFile))
                {
                    return false;
                }

                string outFilePath = Path.GetDirectoryName(outputFile);
                if (string.IsNullOrEmpty(outFilePath))
                {
                    return false;
                }

                List<SimData> simErrorAddBmwFast = new List<SimData>();
                simErrorAddBmwFast.Add(new SimData(new string[] { "83", "XX", "F1", "14", "XX", "XX" },
                    new string[] { "83", "F1", "00|[01]", "54", "FF", "FF", "00^$00" }));    // clear DTC
                simErrorAddBmwFast.Add(new SimData(new string[] { "84", "XX", "F1", "14", "XX", "XX", "XX" },
                    new string[] { "83", "F1", "00|[01]", "54", "FF", "FF", "00^$00" }));    // clear DTC
                simErrorAddBmwFast.Add(new SimData(new string[] { "C3", "XX", "F1", "14", "XX", "XX" },
                    new string[] { "83", "F1", "12", "54", "FF", "FF", "00^$00" }));    // global clear DTC
                simErrorAddBmwFast.Add(new SimData(new string[] { "84", "XX", "F1", "14", "XX", "XX", "XX" },
                    new string[] { "83", "F1", "00|[01]", "54", "FF", "FF", "00^$00" }));    // clear DTC
                simErrorAddBmwFast.Add(new SimData(new string[] { "82", "XX", "F1", "11", "XX" },
                    new string[] { "82", "F1", "00|[01]", "51", "00|[04]", "00^$00" }));    // STEUERGERAETE_RESET
                simErrorAddBmwFast.Add(new SimData(new string[] { "83", "XX", "F1", "17", "XX", "XX" },
                    new string[] { "83", "F1", "00|[01]", "7F", "17", "12", "00^$00" }, simErrorAddBmwFast));    // FS_LESEN_DETAIL

                List<SimData> simCandidatesBmwFast = new List<SimData>();
                simCandidatesBmwFast.Add(new SimData(new string[] { "83", "XX", "F1", "19", "02", "XX" },
                    new string[] { "83", "F1", "00|[01]", "59", "02", "FF", "00^$00" }, simErrorAddBmwFast));  // FS_LESEN
                simCandidatesBmwFast.Add(new SimData(new string[] { "86", "XX", "F1", "19", "06", "XX", "XX", "XX", "XX" },
                    new string[] { "83", "F1", "00|[01]", "59", "06", "FF", "00^$00" }, simErrorAddBmwFast));  // Service 19 06
                simCandidatesBmwFast.Add(new SimData(new string[] { "84", "XX", "F1", "18", "02", "FF", "FF" },
                    new string[] { "82", "F1", "00|[01]", "58", "00", "00^$00" }, simErrorAddBmwFast));  // FS_LESEN
                simCandidatesBmwFast.Add(new SimData(new string[] { "83", "XX", "F1", "22", "XX", "XX" },
                    new string[] { "83", "F1", "00|[01]", "7F", "22", "31", "00^$00" }));     // Service 22
                simCandidatesBmwFast.Add(new SimData(new string[] { "85", "XX", "F1", "31", "01", "XX", "XX", "XX" },
                    null)); // Routine control

                List<SimData> simAddDataBmwFast = new List<SimData>();
                simAddDataBmwFast.Add(new SimData(new string[] { "80&C0", "XX", "F1", "23", "XX", "XX" },
                    new string[] { "83", "F1", "00|[01]", "7F", "23", "31", "00^$00" }));     // Service 23
                simAddDataBmwFast.Add(new SimData(new string[] { "80&C0", "XX", "F1", "30", "XX", "XX", ".." },
                    new string[] { "83", "F1", "00|[01]", "70", "00|[04]", "00|[05]", "00^$00" }));     // Service 30

                List<SimData> simAddDataEdicUds = new List<SimData>();
                simAddDataEdicUds.Add(new SimData(new string[] { "22", "06", "XX" },
                    new string[] { "7F", "22", "31" }));     // Service 22 error response
                simAddDataEdicUds.Add(new SimData(new string[] { "22", "60", "XX" },
                    new string[] { "7F", "22", "31" }));     // Service 22 error response
                simAddDataEdicUds.Add(new SimData(new string[] { "22", "F1", "XX" },
                    new string[] { "7F", "22", "31" }));     // Service 22 error response
                simAddDataEdicUds.Add(new SimData(new string[] { "22", "XX", "XX" },
                    new string[] { "62", "00|[01]", "00|[02]", "30", "31", "32", "33", "34", "35", "36", "37", "38", "39", "41", "42", "43", "44", "45", "46", "47", "48", "49", "4A", "4B", "4C", "4D", "4E", "4F", "50", "51", "52", "53", "54", "56", "57", "58", "59", "5A" }));    // Service 22 simulate string response
                simAddDataEdicUds.Add(new SimData(new string[] { "31", "XX", ".." },
                    new string[] { "7F", "31", "11" }));     // Service 31 error response, not supported
                simAddDataEdicUds.Add(new SimData(new string[] { "2E", "XX", "XX", ".." },
                    new string[] { "6E", "00|[01]", "00|[02]" }));          // Service 2E pos ACK
                simAddDataEdicUds.Add(new SimData(new string[] { "3E" },
                    new string[] { "7E" }));                                // Service 3E tester present short
                simAddDataEdicUds.Add(new SimData(new string[] { "3E", "80" },
                    new string[] { "7E", "F1" }));                          // Service 3E tester present long

                List<SimData> simAddDataEdicTp20 = new List<SimData>();
                simAddDataEdicTp20.Add(new SimData(new string[] { "3E" },
                    new string[] { "81", "F1", "00|#00", "7E", "00" }));            // Service 3E tester present short

                List<SimData> simAddDataEdicKwp2000 = new List<SimData>();
                simAddDataEdicKwp2000.Add(new SimData(new string[] { "21", "XX" },
                    new string[] { "9A", "F1", "00|#00", "61", "00|[01]", "25", "00", "00", "25", "00", "00", "25", "00", "00", "25", "00", "00", "25", "00", "00", "25", "00", "00", "25", "00", "00", "25", "00", "00", "00^$00" }));     // read wmblock
                simAddDataEdicKwp2000.Add(new SimData(new string[] { "22", "XX", "XX" },
                    new string[] { "83", "F1", "00|#00", "7F", "22", "31", "00^$00" }));              // Service 22 error response

                List<SimData> simAddDataEdicKwp1281 = new List<SimData>();
                simAddDataEdicKwp1281.Add(new SimData(new string[] { "03", "XX", "09" },
                    new string[] { "03", "01+[01]", "09", "03" }));     // ACK
                simAddDataEdicKwp1281.Add(new SimData(new string[] { "03", "XX", "0A" },
                    new string[] { "03", "01+[01]", "09", "03" }));     // NACK

                List<SimData> simAddData = new List<SimData>();
                SimFormat simFormatUse = simFormat;
                bool bmwFastFormat = true;
                bool kwp2000_Ds2Format = true;
                EdicTypes edicTypes = EdicTypes.None;
                string[] lines = File.ReadAllLines(outputFile);
                List<SimEntry> simLines = new List<SimEntry>();
                for (int iteration = 0; iteration < 2; iteration++)
                {
                    EdicTypes edicType = EdicTypes.None;
                    int? ecuAddr = null;
                    List<byte> keyBytesPrefix = null;
                    List<byte> keyBytesFinal = null;

                    foreach (string line in lines)
                    {
                        string lineTrim = line.Trim();
                        int commentIndex = lineTrim.IndexOf(';');
                        if (commentIndex >= 0)
                        {
                            lineTrim = lineTrim.Substring(0, commentIndex);
                        }

                        if (string.IsNullOrEmpty(lineTrim))
                        {
                            continue;
                        }

                        List<byte> cfgBytes = null;
                        if (lineTrim.StartsWith("CFG:"))
                        {
                            string cfgLine = lineTrim.Substring(4);
                            cfgBytes = NumberString2List(cfgLine);
                            lineTrim = string.Empty;
                        }

                        if (cfgBytes != null)
                        {
                            bmwFastFormat = false;
                            kwp2000_Ds2Format = false;
                            switch (cfgBytes.Count)
                            {
                                case 2:
                                    edicType = EdicTypes.Tp20;
                                    ecuAddr = cfgBytes[1];
#if false
                                    keyBytesFinal = new List<byte>();
                                    keyBytesFinal.Add(0xDA);
                                    keyBytesFinal.Add(0x8F);
                                    keyBytesFinal.Add(cfgBytes[0]);
                                    keyBytesFinal.Add(0x54);
                                    keyBytesFinal.Add(0x50);
#endif
                                    break;

                                case 3:
                                {
                                    if (cfgBytes[2] == 0x8F)
                                    {
                                        edicType = EdicTypes.Kwp2000;
                                    }
                                    else
                                    {
                                        edicType = EdicTypes.Kwp1281;
                                    }

                                    ecuAddr = cfgBytes[0];
                                    if ((edicType & EdicTypes.Kwp1281) != EdicTypes.None)
                                    {
                                        const int baudRate = 10400;
                                        keyBytesPrefix = new List<byte>();
                                        keyBytesPrefix.Add(cfgBytes[1]);
                                        keyBytesPrefix.Add(cfgBytes[2]);
                                        keyBytesPrefix.Add(0x00);
                                        keyBytesPrefix.Add((baudRate & 0xFF));
                                        keyBytesPrefix.Add(((baudRate >> 8) & 0xFF));
                                    }
                                    else
                                    {
                                        const int baudRate = 10400;
                                        keyBytesFinal = new List<byte>();
                                        keyBytesFinal.Add(cfgBytes[1]);
                                        keyBytesFinal.Add(cfgBytes[2]);
                                        keyBytesFinal.Add((byte)~ecuAddr);
                                        keyBytesFinal.Add((baudRate & 0xFF));
                                        keyBytesFinal.Add(((baudRate >> 8) & 0xFF));
                                    }

                                    break;
                                }

                                case 5:
                                    edicType = EdicTypes.Uds;
                                    ecuAddr = (cfgBytes[1] << 8) | cfgBytes[2];
                                    break;
                            }

                            edicTypes |= edicType;
                        }

                        string[] lineParts = lineTrim.Split(':', StringSplitOptions.TrimEntries);
                        List<byte> requestBytes = new List<byte>();
                        List<byte> responseBytes = new List<byte>();

                        if (lineParts.Length == 2)
                        {
                            requestBytes = NumberString2List(lineParts[0]);
                            responseBytes = NumberString2List(lineParts[1]);

                            if (requestBytes.Count < 1)
                            {
                                if (responseBytes.Count < 1)
                                {
                                    continue;
                                }

                                if ((edicTypes & EdicTypes.Kwp1281) != EdicTypes.None)
                                {
                                    if (keyBytesPrefix != null)
                                    {
                                        keyBytesFinal = new List<byte>();
                                        keyBytesFinal.AddRange(keyBytesPrefix);

                                        int blockEnd = -1;
                                        int blockCount = 0;
                                        int index = 0;
                                        foreach (byte keyByte in responseBytes)
                                        {
                                            if (index == blockEnd)
                                            {
                                                keyBytesFinal.Add(0x03);
                                                blockEnd = -1;
                                            }

                                            if (blockEnd < 0)
                                            {
                                                blockEnd = index + keyByte;
                                                blockCount++;
                                            }

                                            keyBytesFinal.Add(keyByte);
                                            index++;
                                        }

                                        keyBytesFinal.Add(0x03);
                                        keyBytesFinal.Add(0x03);
                                        keyBytesFinal.Add((byte) ((blockCount * 2) + 1));
                                        keyBytesFinal.Add(0x09);
                                        keyBytesFinal.Add(0x03);
                                    }
                                }
                                responseBytes = new List<byte>();
                            }
                        }

                        int dataLengthReq = TelLengthBmwFast(requestBytes, 0);
                        int dataLengthResp = TelLengthBmwFast(responseBytes, 0);

                        if (iteration == 0)
                        {
                            if (dataLengthReq == 0 || dataLengthResp == 0 || requestBytes.Count != dataLengthReq + 1)
                            {
                                bmwFastFormat = false;
                                kwp2000_Ds2Format = false;
                                break;
                            }
                        }

                        if (kwp2000_Ds2Format)
                        {
                            if (IsDs2BmwFastEncoded(requestBytes, responseBytes))
                            {
                                if (ConvertToDs2Telegram(requestBytes) == null)
                                {
                                    kwp2000_Ds2Format = false;
                                }

                                if (ConvertToDs2Telegram(responseBytes) == null)
                                {
                                    kwp2000_Ds2Format = false;
                                }
                            }
                            else if (IsKwp2000BmwFastEncoded(requestBytes, responseBytes))
                            {
                                if (ConvertToKwp2000Telegram(requestBytes) == null)
                                {
                                    kwp2000_Ds2Format = false;
                                }

                                if (ConvertToKwp2000Telegram(responseBytes) == null)
                                {
                                    kwp2000_Ds2Format = false;
                                }
                            }
                            else
                            {
                                kwp2000_Ds2Format = false;
                            }
                        }

                        if (iteration == 0)
                        {
                            continue;
                        }

                        if (!bmwFastFormat && simFormatUse == SimFormat.BmwFast)
                        {
                            return false;
                        }

                        if (!kwp2000_Ds2Format && simFormatUse == SimFormat.Kwp2000s_Ds2)
                        {
                            return false;
                        }

                        if (simFormatUse == SimFormat.None)
                        {
                            if (kwp2000_Ds2Format)
                            {
                                simFormatUse = SimFormat.Kwp2000s_Ds2;
                            }
                            else if (bmwFastFormat)
                            {
                                simFormatUse = SimFormat.BmwFast;
                            }
                            else
                            {
                                if (edicTypes != EdicTypes.None)
                                {
                                    simFormatUse = SimFormat.Edic;
                                }
                                else
                                {
                                    return false;
                                }
                            }
                        }

                        List<byte> requestUse = requestBytes;
                        if (simFormatUse == SimFormat.BmwFast)
                        {
                            simAddData.AddRange(simAddDataBmwFast);

                            requestUse = requestBytes.GetRange(0, dataLengthReq);
                            foreach (SimData simData in simCandidatesBmwFast)
                            {
                                if (simAddData.Contains(simData))
                                {
                                    continue;
                                }

                                bool lineMatched = true;
                                string[] addRequest = simData.Request;
                                if (requestUse.Count == addRequest.Length)
                                {
                                    for (int index = 0; index < requestUse.Count; index++)
                                    {
                                        if (!byte.TryParse(addRequest[index], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out byte requestValue))
                                        {
                                            continue;
                                        }

                                        if (requestUse[index] != requestValue)
                                        {
                                            lineMatched = false;
                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    lineMatched = false;
                                }

                                if (lineMatched)
                                {
                                    if (simData.Response != null)
                                    {
                                        simAddData.Add(simData);
                                    }
                                    else
                                    {
                                        if (responseBytes.Count > 3 && responseBytes[0] == 0x83 && responseBytes[3] == 0x7F)
                                        {   // error response
                                            continue;
                                        }

                                        List<string> requestList = new List<string>(simData.Request);
                                        List<string> responseList = new List<string>();
                                        foreach (byte responseByte in responseBytes)
                                        {
                                            responseList.Add(string.Format(CultureInfo.InvariantCulture, "{0:X02}", responseByte));
                                        }

                                        if (responseList.Count > 2)
                                        {
                                            if (string.Compare(requestList[1], "XX", StringComparison.OrdinalIgnoreCase) == 0)
                                            {
                                                string addressString = string.Format(CultureInfo.InvariantCulture, "{0:X02}", requestUse[1]);
                                                requestList[1] = addressString;
                                                responseList[2] = addressString;
                                            }

                                        }

                                        simAddData.Add(new SimData(requestList.ToArray(), responseList.ToArray()));
                                    }
                                }
                            }

                            List<List<byte>> responseContentList = ExtractBmwFastContentList(responseBytes, true);
                            List<byte> responseUse = new List<byte>();

                            if (responseContentList.Count > 0)
                            {
                                foreach (List<byte> responseContent in responseContentList)
                                {
                                    responseUse.AddRange(responseContent);
                                }
                            }

                            responseBytes = responseUse;
                        }
                        else if (simFormatUse == SimFormat.Edic)
                        {
                            if ((edicType & EdicTypes.Uds) != EdicTypes.None)
                            {
                                AddSimDataEntries(ref simAddData, simAddDataEdicUds, ecuAddr);
                            }
                            if ((edicType & EdicTypes.Tp20) != EdicTypes.None)
                            {
                                AddSimDataEntries(ref simAddData, simAddDataEdicTp20, ecuAddr);
                            }
                            if ((edicType & EdicTypes.Kwp2000) != EdicTypes.None)
                            {
                                if (ecuAddr != null)
                                {
                                    AddSimDataEntries(ref simAddData, simAddDataEdicKwp2000, ecuAddr);
                                }
                            }
                            if ((edicType & EdicTypes.Kwp1281) != EdicTypes.None)
                            {
                                AddSimDataEntries(ref simAddData, simAddDataEdicKwp1281, ecuAddr);
                            }
                        }

                        if (dataLengthReq > 0 && dataLengthResp > 0)
                        {
                            switch (simFormatUse)
                            {
                                case SimFormat.Kwp2000s_Ds2:
                                    if (IsDs2BmwFastEncoded(requestUse, responseBytes))
                                    {
                                        requestUse = ConvertToDs2Telegram(requestUse);
                                        if (requestUse == null)
                                        {
                                            continue;
                                        }

                                        responseBytes = ConvertToDs2Telegram(responseBytes);
                                        if (responseBytes == null)
                                        {
                                            continue;
                                        }
                                    }
                                    else if (IsKwp2000BmwFastEncoded(requestUse, responseBytes))
                                    {
                                        requestUse = ConvertToKwp2000Telegram(requestUse);
                                        if (requestUse == null)
                                        {
                                            continue;
                                        }

                                        responseBytes = ConvertToKwp2000Telegram(responseBytes);
                                        if (responseBytes == null)
                                        {
                                            continue;
                                        }
                                    }
                                    else
                                    {
                                        continue;
                                    }

                                    responseBytes.Add(CalcChecksumXor(responseBytes, 0, responseBytes.Count));
                                    break;

                                case SimFormat.Edic:
                                {
                                    if ((edicType & EdicTypes.Kwp1281) != EdicTypes.None)
                                    {
                                        break;
                                    }

                                    List<List<byte>> requestContentList = ExtractBmwFastContentList(requestUse);
                                    if (requestContentList == null || requestContentList.Count < 1)
                                    {
                                        continue;
                                    }
                                    requestUse = requestContentList[0];

                                    bool fullFrame = (edicType & (EdicTypes.Tp20 | EdicTypes.Kwp2000)) != EdicTypes.None;
                                    List<List<byte>> responseContentList = ExtractBmwFastContentList(responseBytes, fullFrame);
                                    if (responseContentList == null || responseContentList.Count < 1)
                                    {
                                        continue;
                                    }

                                    if (!fullFrame)
                                    {
                                        responseBytes = responseContentList[0];
                                        if (responseContentList.Count > 1)
                                        {
                                            Console.WriteLine("Multiple responses for ECU {0:X02}: {1}", ecuAddr, BitConverter.ToString(requestUse.ToArray()).Replace("-", ","));
                                            foreach (List<byte> singleResponse in responseContentList)
                                            {
                                                Console.WriteLine("Response: {0}", BitConverter.ToString(singleResponse.ToArray()).Replace("-", ","));
                                            }
                                        }
                                        break;
                                    }

                                    responseBytes = new List<byte>();
                                    foreach (List<byte> responseContent in responseContentList)
                                    {
                                        responseBytes.AddRange(responseContent);
                                    }
                                    break;
                                }
                            }
                        }

                        if (keyBytesFinal != null)
                        {
                            string key = GenerateKey(BitConverter.ToString(keyBytesFinal.ToArray()));
                            string keyBytesEntry = BitConverter.ToString(keyBytesFinal.ToArray()).Replace("-", ",");
                            keyBytesFinal = null;
                            AddSimLine(ref simLines, new SimEntry(key, keyBytesEntry, string.Empty, ecuAddr, true));
                        }

                        if (responseBytes.Count > 0)
                        {
                            string key = GenerateKey(BitConverter.ToString(requestUse.ToArray()));
                            if (string.IsNullOrWhiteSpace(key))
                            {
                                key = "_";
                            }

                            string request = BitConverter.ToString(requestUse.ToArray()).Replace("-", ",");
                            if (string.IsNullOrWhiteSpace(request))
                            {
                                request = "_";
                            }

                            string response = BitConverter.ToString(responseBytes.ToArray()).Replace("-", ",");
                            if (string.IsNullOrWhiteSpace(response))
                            {
                                response = string.Empty;
                            }

                            if (edicType == EdicTypes.Kwp1281)
                            {
                                if (request.Length >= 5)
                                {
                                    StringBuilder sbRequest = new StringBuilder(request);
                                    sbRequest[3] = 'X';
                                    sbRequest[4] = 'X';
                                    request = sbRequest.ToString();
                                }

                                if (response.Length >= 5)
                                {
                                    StringBuilder sbResponse = new StringBuilder(response);
                                    sbResponse[3] = '0';
                                    sbResponse[4] = '1';
                                    sbResponse.Insert(5, "+[01]");
                                    sbResponse.Append(",03");
                                    response = sbResponse.ToString();
                                }
                            }
                            AddSimLine(ref simLines, new SimEntry(key, request, response, ecuAddr));
                        }
                    }
                }

                List<SimData> simAddAll = new List<SimData>();
                foreach (SimData simData in simAddData)
                {
                    if (simAddAll.Contains(simData))
                    {
                        continue;
                    }

                    simAddAll.Add(simData);
                    if (simData.AddData != null)
                    {
                        foreach (SimData simDataAdd in simData.AddData)
                        {
                            if (simAddAll.Contains(simDataAdd))
                            {
                                continue;
                            }

                            simAddAll.Add(simDataAdd);
                        }
                    }
                }

                foreach (SimData simData in simAddAll)
                {
                    string genericErrorRequest = List2SimEntry(simData.Request.ToList());
                    string genericErrorResponse = List2SimEntry(simData.Response.ToList());
                    string genericErrorKey = GenerateKey(genericErrorRequest);
                    AddSimLine(ref simLines, new SimEntry(genericErrorKey, genericErrorRequest, genericErrorResponse, simData.EcuAddr));
                }

                string simFileName = simFile;
                string simFilePath = Path.GetDirectoryName(simFile);
                if (string.IsNullOrEmpty(simFilePath))
                {
                    simFileName = Path.Combine(outFilePath, simFile);
                }

                simLines = simLines.OrderBy(x => x.EcuAddr ?? -1).ToList();

                string protocolName = "OBD";
                switch (simFormatUse)
                {
                    case SimFormat.BmwFast:
                        protocolName = "BMW FAST";
                        break;

                    case SimFormat.Kwp2000s_Ds2:
                        protocolName = "KWP2000 / DS2";
                        break;

                    case SimFormat.Edic:
                        protocolName = "EDIC";
                        break;
                }

                using (StreamWriter streamWriter = new StreamWriter(simFileName))
                {
                    streamWriter.WriteLine(";*************************************************");
                    streamWriter.WriteLine($"; {protocolName} simulation file");
                    streamWriter.WriteLine(";*************************************************");

                    streamWriter.WriteLine();
                    streamWriter.WriteLine("[POWERSUPPLY]");
                    streamWriter.WriteLine("UBatt = 12500");

                    streamWriter.WriteLine();
                    streamWriter.WriteLine("[IGNITION]");
                    streamWriter.WriteLine("Ignition = 12500");

                    string lastSection = string.Empty;
                    foreach (SimEntry simEntry in simLines)
                    {
                        if (!simEntry.KeyByte)
                        {
                            continue;
                        }

                        string section = GetSectionName("KEYBYTES", simEntry.EcuAddr);

                        if (lastSection != section)
                        {
                            streamWriter.WriteLine();
                            streamWriter.WriteLine(section);
                            lastSection = section;
                        }

                        streamWriter.WriteLine(simEntry.Key + "=" + simEntry.Request);
                    }

                    lastSection = string.Empty;
                    foreach (SimEntry simEntry in simLines)
                    {
                        if (simEntry.KeyByte)
                        {
                            continue;
                        }

                        string section = GetSectionName("REQUEST", simEntry.EcuAddr);

                        if (lastSection != section)
                        {
                            streamWriter.WriteLine();
                            streamWriter.WriteLine(section);
                            lastSection = section;
                        }

                        streamWriter.WriteLine(simEntry.Key + "=" + simEntry.Request);
                    }

                    lastSection = string.Empty;
                    foreach (SimEntry simEntry in simLines)
                    {
                        if (simEntry.KeyByte)
                        {
                            continue;
                        }

                        string section = GetSectionName("RESPONSE", simEntry.EcuAddr);

                        if (lastSection != section)
                        {
                            streamWriter.WriteLine();
                            streamWriter.WriteLine(section);
                            lastSection = section;
                        }

                        streamWriter.WriteLine(simEntry.Key + "=" + simEntry.Response);
                    }
                }
            }
            catch
            {
                return false;
            }
            return true;
        }

        private static string GenerateKey(string line)
        {
            line = line.Replace(".", "_");
            return Regex.Replace(line, "[^A-Za-z0-9_]", string.Empty);
        }

        private static string GetSectionName(string section, int? ecuAddr)
        {
            string result = section;
            if (ecuAddr != null)
            {
                if (ecuAddr > 0xFF)
                {
                    result = string.Format(CultureInfo.InvariantCulture, "{0:X04}.", ecuAddr) + result;
                }
                else
                {
                    result = string.Format(CultureInfo.InvariantCulture, "{0:X02}.", ecuAddr) + result;
                }
            }

            return "[" + result + "]";
        }

        private static bool AddSimLine(ref List<SimEntry> simLines, SimEntry simEntry)
        {
            string key = simEntry.Key;
            int ecuAddr = simEntry.EcuAddr ?? -1;
            for (int keyIndex = 0; keyIndex < int.MaxValue; keyIndex++)
            {
                string subKey = key + "_" + keyIndex.ToString(CultureInfo.InvariantCulture);
                if (simLines.All(x => string.Compare(x.Key, subKey, StringComparison.OrdinalIgnoreCase) != 0 ||
                                      ecuAddr != (x.EcuAddr ?? -1)))
                {
                    simEntry.Key = subKey;
                    simLines.Add(simEntry);
                    return true;
                }
            }

            return false;
        }

        private static void AddSimDataEntries(ref List<SimData> simDataList, List<SimData> simDataAddList, int? ecuAddr)
        {
            foreach (SimData simData in simDataAddList)
            {
                SimData simDataAdd = new SimData(simData.Request, simData.Response, simData.AddData, ecuAddr);
                if (!simDataList.Contains(simDataAdd))
                {
                    simDataList.Add(simDataAdd);
                }
            }
        }

        private static void StoreReadString(StreamWriter streamWriter, string readString)
        {
            try
            {
                if (readString.Length > 0)
                {
                    List<byte> lineValues = NumberString2List(readString);
                    List<byte> lineConv = ConvertBmwTelegram(lineValues);
                    if (lineConv != null)
                    {
                        lineValues = lineConv;
                    }

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

        private static List<byte> NumberString2List(string numberString)
        {
            List<byte> values = new List<byte>();
            string[] numberArray = numberString.Split(' ', StringSplitOptions.RemoveEmptyEntries);
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

        private static string List2NumberString(List<byte> dataList)
        {
            StringBuilder sr = new StringBuilder();
            foreach (byte data in dataList)
            {
                sr.Append($"{data:X02} ");
            }
            return sr.ToString();
        }

        private static List<byte> HexString2List(string hexString)
        {
            string trimmed = hexString.Trim();
            if (trimmed.Length % 2 != 0)
            {
                return null;
            }

            List<byte> values = new List<byte>();
            for (int i = 0; i < trimmed.Length / 2; i++)
            {
                string number = trimmed.Substring(i * 2, 2);
                if (number.Length > 0)
                {
                    try
                    {
                        int value = Convert.ToInt32(number, 16);
                        values.Add((byte)value);
                    }
                    catch
                    {
                        // ignored
                    }
                }
            }
            return values;
        }

        private static string List2HexString(List<byte> dataList)
        {
            return BitConverter.ToString(dataList.ToArray()).Replace("-", string.Empty);
        }

        private static string List2SimEntry(List<string> dataList)
        {
            StringBuilder sb = new StringBuilder();
            foreach (string data in dataList)
            {
                if (sb.Length > 0)
                {
                    sb.Append(",");
                }

                sb.Append(data);
            }

            return sb.ToString();
        }

        private static List<byte> ReadHexStreamTel(StreamReader streamReader, StreamWriter streamWriter)
        {
            List<byte> telegram = new List<byte>(); 
            string line = string.Empty;
            int dataByte;
            while ((dataByte = streamReader.Read()) >= 0)
            {
                switch (dataByte)
                {
                    case '\r':
                    case '\n':
                    case '\t':
                    case ' ':
                        if (line.Length != 0)
                        {
                            if (!_responseFile)
                            {
                                streamWriter.WriteLine("Line extra char invalid: {0}", line);
                            }
                            return null;
                        }
                        continue;

                    default:
                        line += (char) dataByte;
                        break;
                }

                if (line.Length >= 2)
                {
                    List<byte> lineList = HexString2List(line);
                    if (lineList == null || lineList.Count == 0)
                    {
                        if (!_responseFile)
                        {
                            streamWriter.WriteLine("Line invalid: {0}", line);
                        }
                        return null;
                    }

                    telegram.AddRange(lineList);

                    if (telegram.Count >= 4)
                    {
                        int payloadLength = (((int)telegram[0] << 24) | ((int)telegram[1] << 16) | ((int)telegram[2] << 8) | telegram[3]);
                        int telLength = payloadLength + 6;

                        if (telegram.Count == telLength)
                        {
                            return telegram;
                        }

                        if (telegram.Count > telLength)
                        {
                            if (!_responseFile)
                            {
                                streamWriter.WriteLine("Telegram length overflow: {0}", List2NumberString(telegram));
                            }
                            return null;
                        }
                    }

                    line = string.Empty;
                }
            }

            if (telegram.Count == 0)
            {
                return telegram;
            }

            if (!_responseFile)
            {
                streamWriter.WriteLine("EOF reached");
            }
            return null;
        }

        private static byte CalcChecksumBmwFast(List<byte> data, int offset, int length)
        {
            byte sum = 0;
            for (int i = 0; i < length; i++)
            {
                sum += data[i + offset];
            }
            return sum;
        }

        private static byte CalcChecksumXor(List<byte> data, int offset, int length)
        {
            byte sum = 0;
            for (int i = 0; i < length; i++)
            {
                sum ^= data[i + offset];
            }
            return sum;
        }

        private static bool IsKwp2000sTelegram(List<byte> telegram)
        {
            if (telegram.Count < 5)
            {
                return false;
            }

            if (telegram[0] != 0xB8)
            {
                return false;
            }

            if (telegram[3] + 5 != telegram.Count)
            {
                return false;
            }

            if (CalcChecksumXor(telegram, 0, telegram.Count) != 0x00)
            {
                return false;
            }

            if (IsBmwFastTelegram(telegram))
            {
                return false;
            }

            return true;
        }

        private static List<byte> ConvertBmwTelegram(List<byte> telegram)
        {
            List<byte> resultDs2 = ConvertFromDs2Telegram(telegram);
            if (resultDs2 != null)
            {
                _ds2Mode = true;
                return resultDs2;
            }

            List<byte> resultKwp2000s = ConvertKwp2000sTelegram(telegram);
            if (resultKwp2000s != null)
            {
                _kwp2000sMode = true;
                return resultKwp2000s;
            }

            return null;
        }

        private static List<byte> ConvertKwp2000sTelegram(List<byte> telegram)
        {
            if (!IsKwp2000sTelegram(telegram))
            {
                return null;
            }

            int dataLength = telegram[3];
            List<byte> result = new List<byte>();
            if (dataLength > 0x3F)
            {
                result.Add(0x80);
                result.Add(telegram[1]);
                result.Add(telegram[2]);
                result.Add((byte)dataLength);
            }
            else
            {
                result.Add((byte)(dataLength | 0x80));    // header
                result.Add(telegram[1]);
                result.Add(telegram[2]);
            }

            result.AddRange(telegram.GetRange(4, dataLength));
            byte checkSum = CalcChecksumBmwFast(result, 0, result.Count);
            result.Add(checkSum);

            return result;
        }

        private static bool IsBmwFastTelegram(List<byte> telegram)
        {
            if (telegram.Count < 5)
            {
                return false;
            }

            switch (telegram[0] & 0xC0)
            {
                case 0x80:
                case 0xC0:
                    break;

                default:
                    return false;
            }

            if (!ChecksumValid(telegram))
            {
                return false;
            }

            return true;
        }

        private static bool IsDs2Telegram(List<byte> telegram)
        {
            if (telegram.Count < 3)
            {
                return false;
            }

            if (telegram[1] != telegram.Count)
            {
                return false;
            }

            if (CalcChecksumXor(telegram, 0, telegram.Count) != 0x00)
            {
                return false;
            }

            if (IsBmwFastTelegram(telegram))
            {
                return false;
            }

            return true;
        }

        private static bool IsDs2BmwFastEncoded(List<byte> request, List<byte> response)
        {
            if (TelLengthBmwFast(request, 0) == 0)
            {
                return false;
            }

            if (TelLengthBmwFast(response, 0) == 0)
            {
                return false;
            }

            if (request.Count < 3 || response.Count < 3)
            {
                return false;
            }

            if (request[2] != 0xF1)
            {
                return false;
            }

            if (response[2] != 0xF1)
            {
                return false;
            }

            return true;
        }

        private static bool IsKwp2000BmwFastEncoded(List<byte> request, List<byte> response)
        {
            if (TelLengthBmwFast(request, 0) == 0)
            {
                return false;
            }

            if (TelLengthBmwFast(response, 0) == 0)
            {
                return false;
            }

            if (request.Count < 3 || response.Count < 3)
            {
                return false;
            }

            if (request[1] != response[2])
            {
                return false;
            }

            if (request[2] != response[1])
            {
                return false;
            }

            return true;
        }

        private static List<byte> ConvertFromDs2Telegram(List<byte> telegram)
        {
            if (telegram == null)
            {
                return null;
            }

            if (!IsDs2Telegram(telegram))
            {
                return null;
            }

            int dataLength = telegram.Count - 3;
            List<byte> result = new List<byte>();
            if (dataLength > 0x3F)
            {
                result.Add(0x80);
                result.Add(telegram[0]);    // address
                result.Add(0xF1);
                result.Add((byte)dataLength);
            }
            else
            {
                result.Add((byte)(dataLength | 0x80));    // header
                result.Add(telegram[0]);     // address
                result.Add(0xF1);
            }

            result.AddRange(telegram.GetRange(2, dataLength));
            byte checkSum = CalcChecksumBmwFast(result, 0, result.Count);
            result.Add(checkSum);

            return result;
        }

        private static List<byte> ConvertToDs2Telegram(List<byte> telegram)
        {
            if (telegram == null)
            {
                return null;
            }

            int telLength = TelLengthBmwFast(telegram, 0);
            if (telLength == 0)
            {
                return null;
            }

            if (telLength + 1 != telegram.Count)
            {
                return null;
            }

            if ((telegram[0] & 0xC0) != 0x80)
            {
                return null;
            }

            int dataLength = telegram[0] & 0x3F;
            byte ecuAddr = telegram[1];
            List<byte> result = new List<byte>();
            if (dataLength == 0)
            {   // with length byte
                dataLength = telegram[3];
                if (dataLength == 0)
                {
                    return null;
                }

                result.Add(ecuAddr);
                result.Add((byte)(dataLength + 3));
                result.AddRange(telegram.GetRange(4, dataLength));
            }
            else
            {   // without length byte
                result.Add(ecuAddr);
                result.Add((byte)(dataLength + 3));
                result.AddRange(telegram.GetRange(3, dataLength));
            }

            return result;
        }

        private static List<byte> ConvertToKwp2000Telegram(List<byte> telegram)
        {
            if (telegram == null)
            {
                return null;
            }

            int telLength = TelLengthBmwFast(telegram, 0);
            if (telLength == 0)
            {
                return null;
            }

            if (telLength + 1 != telegram.Count)
            {
                return null;
            }

            if ((telegram[0] & 0xC0) != 0x80)
            {
                return null;
            }

            byte dataLength = (byte) (telegram[0] & 0x3F);
            List<byte> result = new List<byte>();
            result.Add(0xB8);
            result.Add(telegram[1]);
            result.Add(telegram[2]);
            if (dataLength == 0)
            {   // with length byte
                dataLength = telegram[3];
                if (dataLength == 0)
                {
                    return null;
                }

                result.Add((byte)(dataLength));
                result.AddRange(telegram.GetRange(4, dataLength));
            }
            else
            {   // without length byte
                result.Add(dataLength);
                result.AddRange(telegram.GetRange(3, dataLength));
            }

            return result;
        }

        private static List<List<byte>> ExtractBmwFastContentList(List<byte> telegram, bool completeFrame = false)
        {
            if (telegram == null)
            {
                return null;
            }

            List<List<byte>> resultList = new List<List<byte>>();
            int offset = 0;
            for (;;)
            {
                int telLength = TelLengthBmwFast(telegram, offset);
                if (telLength == 0)
                {
                    return null;
                }

                if (offset + telLength + 1 > telegram.Count)
                {
                    return null;
                }

                int dataLength = telegram[offset] & 0x3F;
                int dataOffset;
                List<byte> result = new List<byte>();
                if (dataLength == 0)
                {
                    // with length byte
                    dataLength = telegram[3 + offset];
                    if (dataLength == 0)
                    {
                        dataLength = (telegram[4 + offset] << 8) | telegram[5 + offset];
                        dataOffset = 6;
                    }
                    else
                    {
                        dataOffset = 4;
                    }
                }
                else
                {
                    // without length byte
                    dataOffset = 3;
                }

                result.AddRange(telegram.GetRange(dataOffset + offset, dataLength));

                bool filterResponse = false;
                if (result.Count == 3)
                {
                    if (result[0] == 0x7F)
                    {
                        switch (result[2])
                        {
                            case 0x22:
                            case 0x23:
                            case 0x78:
                                filterResponse = true;
                                break;
                        }
                    }
                }

                if (!filterResponse)
                {
                    if (completeFrame)
                    {
                        result = telegram.GetRange(offset, telLength);
                        result.Add(CalcChecksumBmwFast(result, 0, result.Count));
                    }

                    resultList.Add(result);
                }

                offset += telLength + 1;    // checksum
                if (offset == telegram.Count)
                {
                    break;
                }
            }

            return resultList;
        }

        // telegram length without checksum
        private static int TelLengthBmwFast(List<byte> telegram, int offset)
        {
            if (telegram.Count - offset < 4)
            {
                return 0;
            }
            int telLength = telegram[0 + offset] & 0x3F;
            if (telLength == 0)
            {   // with length byte
                if (telegram[3 + offset] == 0)
                {
                    if (telegram.Count < 6)
                    {
                        return 0;
                    }
                    telLength = ((telegram[4 + offset] << 8) | telegram[5 + offset]) + 6;
                }
                else
                {
                    telLength = telegram[3 + offset] + 4;
                }
            }
            else
            {
                telLength += 3;
            }
            return telLength;
        }

        private static List<byte> CreateBmwFastTel(List<byte> data, byte dest, byte source)
        {
            List<byte> result = new List<byte>();
            if (data.Count > 0x3F)
            {
                result.Add(0x80);
                result.Add(dest);
                result.Add(source);
                result.Add((byte)data.Count);
            }
            else
            {
                result.Add((byte) (0x80 | data.Count));
                result.Add(dest);
                result.Add(source);
            }

            if (IsFunctionalAddress(dest))
            {
                result[0] |= 0x40;
            }

            result.AddRange(data);
            result.Add(CalcChecksumBmwFast(result, 0, result.Count));
            return result;
        }

        private static List<byte> CreateEnetBmwFastTel(List<byte> data)
        {
            if (data.Count < 8)
            {
                return null;
            }

            byte dest = data[7];
            byte source = data[6];
            if (dest == 0xF4)
            {
                dest = 0xF1;
            }

            if (source == 0xF4)
            {
                source = 0xF1;
            }

            return CreateBmwFastTel(data.GetRange(8, data.Count - 8), dest, source);
        }

        public static bool IsFunctionalAddress(byte address)
        {
            if (address == 0xDF)
            {
                return true;
            }

            if (address >= 0xE6 && address <= 0xEF)
            {
                return true;
            }

            return false;
        }

        private static bool ChecksumValid(List<byte> telegram)
        {
            if (_ignoreCrcErrors)
            {
                return true;
            }

            int offset = 0;
            for (; ; )
            {
                int dataLength = TelLengthBmwFast(telegram, offset);
                if (dataLength == 0) return false;
                if (telegram.Count - offset < dataLength + 1)
                {
                    return false;
                }

                byte sum = CalcChecksumBmwFast(telegram, offset, dataLength);
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

        private static bool ValidResponse(List<byte> request, List<byte> response)
        {
            bool broadcast = (request[0] & 0xC0) != 0x80;
            if (!ChecksumValid(request) || !ChecksumValid(response))
            {
                return false;
            }
            if (!broadcast && !_ds2Mode && !_edicCanMode && !_edicCanIsoTpMode)
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

        private static bool UpdateRequestAddr(List<byte> request, List<byte> response)
        {
            if (!ChecksumValid(request) || !ChecksumValid(response))
            {
                return false;
            }
            if (request.Count < 4)
            {
                return false;
            }
            request[1] = response[2];
            request[2] = response[1];
            request[request.Count - 1] = CalcChecksumBmwFast(request, 0, request.Count - 1);
            return true;
        }

        private static List<byte> CleanKwp1281Tel(List<byte> tel, bool keyBytes = false)
        {
            List<byte> result = new List<byte>();
            int offset = 0;
            if (keyBytes)
            {
                offset = 5;
                if (tel.Count < offset)
                {
                    return new List<byte>();
                }
                result.AddRange(tel.GetRange(0, offset));
            }
            for (;;)
            {
                if (offset >= tel.Count)
                {
                    break;
                }
                byte len = tel[offset];
                if (tel.Count < offset + len + 1)
                {
                    return new List<byte>();
                }
                if (tel[offset + len] != 0x03)
                {
                    return new List<byte>();
                }
                if (len != 3 || tel[offset + 2] != 0x09)
                {   // ack
                    result.AddRange(tel.GetRange(offset, len));
                }
                offset += len + 1;
            }
            return result;
        }

        private static bool CheckKwp1281Tel(List<byte> tel)
        {
            if (tel.Count == 0)
            {
                return false;
            }
            int offset = 0;
            for (;;)
            {
                if (offset >= tel.Count)
                {
                    break;
                }
                byte len = tel[offset];
                if (len == 0)
                {
                    return false;
                }
                if (tel.Count < offset + len)
                {
                    return false;
                }
                if (len == 3 && tel[offset + 2] == 0x09)
                {   // ack
                    return false;
                }
                offset += len;
            }
            return true;
        }

        private static string NumberString2String(string numberString, bool simpleFormat)
        {
            string result = string.Empty;

            List<byte> values = NumberString2List(numberString);

            if (_ds2Mode || _kwp2000sMode)
            {
                List<byte> valuesConv = ConvertBmwTelegram(values);
                if (valuesConv != null)
                {
                    values = valuesConv;
                }
            }

            if (_edicCanMode && values.Count > 0)
            {

                int offset = 0;
                for (;;)
                {
                    int dataLength = TelLengthBmwFast(values, offset);
                    if (dataLength == 0) return string.Empty;
                    if (values.Count - offset < dataLength + 1)
                    {   // error
                        break;
                    }

                    bool updateChecksum = false;
                    if (values[1 + offset] == _edicCanAddr && values[2 + offset] == _edicCanTesterAddr)
                    {
                        values[1 + offset] = (byte)_edicCanEcuAddr;
                        updateChecksum = true;
                    }
                    else if (values[1 + offset] == 0x00 && values[2 + offset] == _edicCanAddr)
                    {
                        values[1 + offset] = (byte)_edicCanTesterAddr;
                        values[2 + offset] = (byte)_edicCanEcuAddr;
                        updateChecksum = true;
                    }
                    if (updateChecksum)
                    {
                        byte sum = CalcChecksumBmwFast(values, offset, dataLength);
                        values[dataLength + offset] = sum;
                    }

                    offset += dataLength + 1;    // checksum
                    if (offset > values.Count)
                    {   // error
                        break;
                    }
                    if (offset == values.Count)
                    {
                        break;
                    }
                }
            }

            foreach (byte value in values)
            {
                if (simpleFormat)
                {
                    if (result.Length > 0)
                    {
                        result += " ";
                    }
                    result += $"{value:X02}";
                }
                else
                {
                    if (result.Length > 0)
                    {
                        result += ", ";
                    }
                    result += $"0x{value:X02}";
                }
            }

            return result;
        }

        static void ShowHelp(OptionSet p)
        {
            Console.WriteLine("Usage: " + Path.GetFileNameWithoutExtension(AppDomain.CurrentDomain.FriendlyName) + " [OPTIONS]");
            Console.WriteLine("Convert OBD log files");
            Console.WriteLine();
            Console.WriteLine("Options:");
            p.WriteOptionDescriptions(Console.Out);
        }
    }
}
