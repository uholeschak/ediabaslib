using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace UdsFileReader
{
    static class Program
    {
        static readonly Regex InvalidFileRegex = new Regex("^(R.|ReDir|TTDOP|MUX|TTText.*|Unit.*)$", RegexOptions.IgnoreCase);
        private static Dictionary<UInt32, UInt32> _unknownIdDict;

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
            DirectoryInfo dirInfoParent = Directory.GetParent(dir);
            if (dirInfoParent == null)
            {
                Console.WriteLine("Invalid directory");
                return 1;
            }
            string rootDir = dirInfoParent.FullName;
            string searchPattern = Path.GetFileName(fileSpec);
            if (dir == null || searchPattern == null)
            {
                Console.WriteLine("Invalid file name");
                return 1;
            }

            try
            {
                UdsReader udsReader = new UdsReader();
                if (!udsReader.Init(rootDir
                    //, new HashSet<UdsReader.SegmentType>{ UdsReader.SegmentType.Mwb, UdsReader.SegmentType.Dtc }
                    ))
                {
                    Console.WriteLine("Init failed");
                    return 1;
                }

                //Console.WriteLine(udsReader.TestFixedTypes());
                //return 0;
#if false
                StringBuilder sbVin = new StringBuilder();;
                sbVin.Append(@"WVWFA71F77V");
                for (char code = '0'; code <= 'Z'; code++)
                {
                    sbVin[9] = code;
                    int modelYear = DataReader.GetModelYear(sbVin.ToString());
                    Console.WriteLine("'{0}': {1}", code, modelYear);
                }
                return 0;
#endif
#if false
                PrintSaeErrorCode(udsReader, 0x161D, 0x71);
                PrintSaeErrorCode(udsReader, 0x161D, 0x03);
                PrintSaeErrorCode(udsReader, 0x161D, 0xF5);
                PrintSaeErrorCode(udsReader, 0x2634, 0xF5);
                PrintSaeErrorCode(udsReader, 0x4003, 0x96);
                PrintSaeErrorCode(udsReader, 0x900E, 0x96);
                PrintSaeErrorCode(udsReader, 0xD156, 0x75);
                PrintSaeErrorCode(udsReader, 0xD156, 0xF5);
                PrintSaeErrorCode(udsReader, 0xD156, 0x25);
                PrintSaeErrorCode(udsReader, 0x514E, 0xF0);
                PrintKwpErrorCode(udsReader, 0x036C, 0x6B);
                PrintKwpErrorCode(udsReader, 0x036D, 0x6B);
                PrintKwpErrorCode(udsReader, 0x05DF, 0x29);
                PrintKwpErrorCode(udsReader, 0x03A0, 0x28);
                PrintKwpErrorCode(udsReader, 0x045D, 0x68);
                PrintKwpErrorCode(udsReader, 0x038B, 0x60);
                PrintKwpErrorCode(udsReader, 0x0466, 0x28);
                PrintKwpErrorCode(udsReader, 0x4123, 0x28);
                PrintKwpErrorCode(udsReader, 0x4123, 0xA8);
                PrintKwpErrorCode(udsReader, 0x4523, 0x29);
                PrintKwpErrorCode(udsReader, 0x4923, 0x2A);
                PrintKwpErrorCode(udsReader, 0x4D23, 0x2B);
                PrintKwpErrorCode(udsReader, 0x7123, 0x2C);
                PrintKwpErrorCode(udsReader, 0x7523, 0x2D);
                PrintKwpErrorCode(udsReader, 0x6523, 0x2E);

                PrintIsoErrorCode(udsReader, 0x455B, 0x23);
                PrintIsoErrorCode(udsReader, 0x4474, 0xA3);
                PrintIsoErrorCode(udsReader, 0x0074, 0xA3);
                PrintSaeErrorDetail(udsReader, new byte[] { 0x6C, 0x01, 0x71, 0x11, 0x12, 0x13, 0x02, 0xFB, 0xC4, 0x00, 0x00, 0x49, 0x04, 0x51, 0x03 });
                PrintSaeErrorDetail(udsReader, new byte[] { 0x6C, 0x01, 0x71, 0x11, 0x12, 0x13, 0x02, 0xFB, 0xC4, 0x01, 0x00, 0x00, 0x00, 0x01, 0x02 });
                PrintUdsErrorDetail(udsReader, new byte[] { 0x59, 0x06, 0x00, 0x40, 0x11, 0x08, 0x01, 0x02, 0x01, 0x02, 0x1F, 0x00, 0x93, 0x13, 0x00,
                    0x00, 0x49, 0x56, 0xB0, 0xA3, 0x71, 0x00, 0x00, 0x80, 0x18, 0x12, 0x00, 0x00, 0x00, 0xCA, 0x84, 0x00, 0x00, 0xFF, 0x00, 0x00,
                    0x61, 0x02, 0x40, 0x00, 0x00, 0x00, 0x00, 0x10 });
                PrintUdsErrorDetail(udsReader, new byte[] { 0x59, 0x06, 0x10, 0x01, 0x06, 0x08, 0x01, 0x06, 0x01, 0x02, 0x94, 0x00, 0xA4, 0xFA, 0x00, 0x00, 0x49, 0xF8, 0xF7, 0x95, 0x71, 0x02, 0x86, 0x5B, 0x27, 0xBE, 0xBF, 0x01 });
                return 0;
#endif
#if false
                UdsReader.FileNameResolver fileNameResolver = new UdsReader.FileNameResolver(udsReader, "WVGZZZ1TZBW000000", "EV_ECM20TDI01103L906018DQ", "003003", "03L906018DQ", "1K0907951");
                //UdsReader.FileNameResolver fileNameResolver = new UdsReader.FileNameResolver(udsReader, "WVGZZZ1TZBW000000", "EV_Kombi_UDS_VDD_RM09", "A04089", "0920881A", "1K0907951");
                List<string> fileList = fileNameResolver.GetFileList(dir);
                foreach (string fileName in fileList)
                {
                    Console.WriteLine(fileName);
                }
                return 0;
#endif
#if false
                DataReader dataReader = new DataReader();
                DataReader.FileNameResolver fileNameResolver = new DataReader.FileNameResolver(dataReader, "03L906018DQ", string.Empty, 1);
                string fileName = fileNameResolver.GetFileName(Path.Combine(rootDir, DataReader.DataDir));
                if (!string.IsNullOrEmpty(fileName))
                {
                    List<DataReader.DataInfo> info = dataReader.ExtractDataType(fileName, DataReader.DataType.LongCoding);
                    foreach (DataReader.DataInfo dataInfo in info)
                    {
                        foreach (string text in dataInfo.TextArray)
                        {
                            Console.WriteLine(text);
                        }
                    }
                }
                return 0;
#endif
                _unknownIdDict = new Dictionary<UInt32, UInt32>();

                string[] files = Directory.GetFiles(dir, searchPattern, SearchOption.AllDirectories);
                foreach (string file in files)
                {
                    try
                    {
                        string fileExt = Path.GetExtension(file);
                        if (string.Compare(fileExt, DataReader.FileExtension, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            Console.WriteLine("Parsing: {0}", file);
                            string outFileData = Path.ChangeExtension(file, ".txt");
                            if (outFileData == null)
                            {
                                Console.WriteLine("*** Invalid output file");
                            }
                            else
                            {
                                using (StreamWriter outputStream = new StreamWriter(outFileData, false, new UTF8Encoding(true)))
                                {
                                    if (!ParseDataFile(udsReader.DataReader, file, outputStream))
                                    {
                                        Console.WriteLine("*** Parsing failed: {0}", file);
                                    }
                                }
                            }
                        }
                        else if (string.Compare(fileExt, UdsReader.FileExtension, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            string baseFile = Path.GetFileNameWithoutExtension(file);
                            if (baseFile == null || InvalidFileRegex.IsMatch(baseFile))
                            {
                                Console.WriteLine("Ignoring: {0}", file);
                                continue;
                            }
                            Console.WriteLine("Parsing: {0}", file);
                            string outFileUds = Path.ChangeExtension(file, ".txt");
                            if (outFileUds == null)
                            {
                                Console.WriteLine("*** Invalid output file");
                            }
                            else
                            {
                                using (StreamWriter outputStream = new StreamWriter(outFileUds, false, new UTF8Encoding(true)))
                                {
                                    if (!ParseUdsFile(udsReader, file, outputStream))
                                    {
                                        Console.WriteLine("*** Parsing failed: {0}", file);
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("*** Exception {0}", e.Message);
                    }
                }

                if (_unknownIdDict.Count > 0)
                {
                    StringBuilder sb = new StringBuilder();
                    Console.WriteLine();
                    Console.WriteLine("Unknown IDs:");
                    foreach (UInt32 key in _unknownIdDict.Keys.OrderBy(x => x))
                    {
                        if (sb.Length > 0)
                        {
                            sb.Append(", ");
                        }
                        sb.Append($"{key}({_unknownIdDict[key]})");
                    }
                    sb.Insert(0, "Index: ");
                    Console.WriteLine(sb.ToString());
                    Console.WriteLine();

                    sb.Clear();
                    foreach (UInt32 key in _unknownIdDict.Keys.OrderByDescending(x => _unknownIdDict[x]))
                    {
                        if (sb.Length > 0)
                        {
                            sb.Append(", ");
                        }
                        sb.Append($"{key}({_unknownIdDict[key]})");
                    }
                    sb.Insert(0, "Value: ");
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

        static bool ParseDataFile(DataReader dataReader, string fileName, StreamWriter outStream)
        {
            try
            {
                foreach (DataReader.DataType dataType in Enum.GetValues(typeof(DataReader.DataType)))
                {
                    List<DataReader.DataInfo> dataInfoList = dataReader.ExtractDataType(fileName, dataType);
                    if (dataInfoList == null)
                    {
                        outStream.WriteLine("Parsing failed");
                        return false;
                    }
                    if (dataInfoList.Count == 0)
                    {
                        continue;
                    }

                    outStream.WriteLine();
                    outStream.WriteLine("Data Type: {0}", dataType.ToString());
                    outStream.WriteLine("-----------------------------------");

                    foreach (DataReader.DataInfo dataInfo in dataInfoList)
                    {
                        StringBuilder sb = new StringBuilder();
                        if (dataInfo.TextArray != null)
                        {
                            foreach (string entry in dataInfo.TextArray)
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

                            if (dataInfo.Value1.HasValue)
                            {
                                if (sb.Length > 0)
                                {
                                    sb.Append("; ");
                                }
                                sb.Append(string.Format(CultureInfo.InvariantCulture, "V1: {0}", dataInfo.Value1.Value));
                            }
                            if (dataInfo.Value2.HasValue)
                            {
                                if (sb.Length > 0)
                                {
                                    sb.Append("; ");
                                }
                                sb.Append(string.Format(CultureInfo.InvariantCulture, "V2: {0}", dataInfo.Value2.Value));
                            }
                            outStream.WriteLine(sb.ToString());
                        }

                        if (dataInfo is DataReader.DataInfoLongCoding longCoding)
                        {
                            sb.Clear();
                            if (longCoding.Byte.HasValue)
                            {
                                if (sb.Length > 0)
                                {
                                    sb.Append("; ");
                                }
                                sb.Append(string.Format(CultureInfo.InvariantCulture, "Byte: {0}", longCoding.Byte.Value));
                            }
                            if (longCoding.Bit.HasValue)
                            {
                                if (sb.Length > 0)
                                {
                                    sb.Append("; ");
                                }
                                sb.Append(string.Format(CultureInfo.InvariantCulture, "Bit: {0}", longCoding.Bit.Value));
                            }
                            if (longCoding.BitMin.HasValue)
                            {
                                if (sb.Length > 0)
                                {
                                    sb.Append("; ");
                                }
                                sb.Append(string.Format(CultureInfo.InvariantCulture, "BitMin: {0}", longCoding.BitMin.Value));
                            }
                            if (longCoding.BitMax.HasValue)
                            {
                                if (sb.Length > 0)
                                {
                                    sb.Append("; ");
                                }
                                sb.Append(string.Format(CultureInfo.InvariantCulture, "BitMax: {0}", longCoding.BitMax.Value));
                            }
                            if (longCoding.BitValue.HasValue)
                            {
                                if (sb.Length > 0)
                                {
                                    sb.Append("; ");
                                }
                                sb.Append(string.Format(CultureInfo.InvariantCulture, "BitValue: 0x{0:X02}", longCoding.BitValue.Value));
                            }
                            if (longCoding.LineNumber.HasValue)
                            {
                                if (sb.Length > 0)
                                {
                                    sb.Append("; ");
                                }
                                sb.Append(string.Format(CultureInfo.InvariantCulture, "Line: {0}", longCoding.LineNumber.Value));
                            }
                            if (longCoding.Text != null)
                            {
                                if (sb.Length > 0)
                                {
                                    sb.Append("; ");
                                }
                                sb.Append(string.Format(CultureInfo.InvariantCulture, "Text: \"{0}\"", longCoding.Text));
                            }

                            if (sb.Length > 0)
                            {
                                sb.Insert(0, "LongCoding: ");
                                outStream.WriteLine(sb.ToString());
                            }
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                try
                {
                    outStream.WriteLine("Exception: {0}", ex.Message);
                }
                catch (Exception)
                {
                    // ignored
                }
                return false;
            }
        }

        static bool ParseUdsFile(UdsReader udsReader, string fileName, StreamWriter outStream)
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

                foreach (UdsReader.SegmentType segmentType in Enum.GetValues(typeof(UdsReader.SegmentType)))
                {
                    UdsReader.SegmentInfo segmentInfo = udsReader.GetSegmentInfo(segmentType);
                    if (segmentInfo == null || segmentInfo.Ignored)
                    {
                        outStream.WriteLine();
                        outStream.WriteLine("*** Ignoring segment: {0}", segmentType.ToString());
                        continue;
                    }
                    List<UdsReader.ParseInfoBase> resultList = udsReader.ExtractFileSegment(includeFiles, segmentType);
                    if (resultList == null)
                    {
                        outStream.WriteLine("Parsing failed");
                        return false;
                    }
                    if (resultList.Count == 0)
                    {
                        continue;
                    }

                    outStream.WriteLine();
                    outStream.WriteLine("Segment Type: {0}", segmentType.ToString());
                    outStream.WriteLine("-----------------------------------");
                    foreach (UdsReader.ParseInfoBase parseInfo in resultList)
                    {
                        outStream.WriteLine("");

                        StringBuilder sb = new StringBuilder();
                        if (parseInfo.LineArray != null)
                        {
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
                        }

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

                            if (parseInfo is UdsReader.ParseInfoAdp parseInfoAdp)
                            {
                                if (parseInfoAdp.SubItem.HasValue)
                                {
                                    outStream.WriteLine(string.Format(CultureInfo.InvariantCulture, "Sub Item: {0:X02}", parseInfoAdp.SubItem.Value));
                                }
                            }
                            outStream.WriteLine(string.Format(CultureInfo.InvariantCulture, "Data ID Name: {0}", parseInfoMwb.DataIdString));

                            if (!PrintDataTypeEntry(outStream, parseInfoMwb.DataTypeEntry, parseInfo))
                            {
                                return false;
                            }

                            outStream.WriteLine(TestDataType(fileName, parseInfoMwb));
                        }

                        if (parseInfo is UdsReader.ParseInfoDtc parseInfoDtc)
                        {
                            sb.Clear();
                            outStream.WriteLine(string.Format(CultureInfo.InvariantCulture, "Error Code: {0} (0x{0:X06}), {1}", parseInfoDtc.ErrorCode, parseInfoDtc.PcodeText));
                            outStream.WriteLine(string.Format(CultureInfo.InvariantCulture, "Error Text: {0}", parseInfoDtc.ErrorText));
                            if (parseInfoDtc.DetailCode.HasValue && parseInfoDtc.DetailCode.Value > 0)
                            {
                                outStream.WriteLine(string.Format(CultureInfo.InvariantCulture, "Detail Code: {0:X02}", parseInfoDtc.DetailCode));
                            }
                            if (!string.IsNullOrEmpty(parseInfoDtc.ErrorDetail))
                            {
                                outStream.WriteLine(string.Format(CultureInfo.InvariantCulture, "Error Detail: {0}", parseInfoDtc.ErrorDetail));
                            }
                        }

                        if (parseInfo is UdsReader.ParseInfoSlv parseInfoSlv)
                        {
                            sb.Clear();
                            if (parseInfoSlv.TableKey.HasValue)
                            {
                                outStream.WriteLine(string.Format(CultureInfo.InvariantCulture, "Slave Table Key: {0}", parseInfoSlv.TableKey));
                                if (parseInfoSlv.SlaveList != null)
                                {
                                    foreach (UdsReader.ParseInfoSlv.SlaveInfo slaveInfo in parseInfoSlv.SlaveList)
                                    {
                                        outStream.WriteLine(string.Format(CultureInfo.InvariantCulture, "Min Addr: {0}, Max Addr: {1}, Name: {2}",
                                            slaveInfo.MinAddr, slaveInfo.MaxAddr, slaveInfo.Name));
                                    }
                                }
                            }
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                try
                {
                    outStream.WriteLine("Exception: {0}", ex.Message);
                }
                catch (Exception)
                {
                    // ignored
                }
                return false;
            }
        }

        static string TestDataType(string fileName, UdsReader.ParseInfoMwb parseInfoMwb)
        {
            UdsReader.DataTypeEntry dataTypeEntry = parseInfoMwb.DataTypeEntry;

            StringBuilder sb = new StringBuilder();

            sb.Append("Test: ");
            byte[] testData = null;
            string baseFileName = Path.GetFileNameWithoutExtension(fileName) ?? string.Empty;
            if (baseFileName.StartsWith("EV_ECM20TDI01103L906018DQ", true, CultureInfo.InvariantCulture))
            {
                switch (parseInfoMwb.ServiceId)
                {
                    case 0xF40C:    // Motordrehzahl
                        testData = new byte[] { 0x0F, 0xA0 };
                        break;

                    case 0x11F1:    // Getriebeeingangsdrehzahl
                        testData = new byte[] { 0x07, 0xD0 };
                        break;

                    case 0x0100:    // Status des Stellgliedtests
                        testData = new byte[] { 0x80 };
                        break;

                    case 0xF40D:    // Fahrzeuggeschwindigkeit
                        testData = new byte[] { 0x64 };
                        break;

                    case 0x2001:    // Status der Kraftstofferstbefüllung
                        testData = new byte[] { 0xC0 };
                        break;

                    case 0xF41F:    // Zeit seit Motorstart
                        testData = new byte[] { 0x27, 0x10 };
                        break;

                    case 0x100D:    // eingelegter Gang
                        testData = new byte[] { 0x02 };
                        break;

                    case 0x02A7:    // SSEUI
                       testData = new byte[] { 0xE8, 0x03, 0xE9, 0x03, 0xEA, 0x03, 0xEB, 0x03, 0xEC, 0x03, 0xED, 0x03, 0xEE, 0x03, 0xEF, 0x03, 0xF0, 0x03, 0xF1, 0x03, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
                       break;
                }
            }
            else if (baseFileName.StartsWith("EV_Kombi_UDS_VDD_RM09_A04_VW32", true, CultureInfo.InvariantCulture))
            {
                switch (parseInfoMwb.ServiceId)
                {
                    case 0xF442:    // Spannung Klemme 30
                        testData = new byte[] { 0x8F };
                        break;

                    case 0x2203:    // Wegstrecke
                        testData = new byte[] { 0x03, 0xE8 };
                        //testData = new byte[] { 0xFF, 0xFF };
                        break;
                }
            }

            if (testData != null)
            {
                sb.Append(" \"");
                sb.Append(dataTypeEntry.ToString(testData));
                sb.Append("\"");
            }
            else
            {
                sb.Append(" \"");
                sb.Append(dataTypeEntry.ToString(new byte[] { 0x10 }));
                sb.Append("\"");

                sb.Append(" \"");
                sb.Append(dataTypeEntry.ToString(new byte[] { 0x10, 0x20 }));
                sb.Append("\"");

                sb.Append(" \"");
                sb.Append(dataTypeEntry.ToString(new byte[] { 0xFF, 0x10 }));
                sb.Append("\"");

                sb.Append(" \"");
                sb.Append(dataTypeEntry.ToString(new byte[] { 0xFF, 0x10, 0x20 }));
                sb.Append("\"");

                sb.Append(" \"");
                sb.Append(dataTypeEntry.ToString(new byte[] { 0xFF, 0xAB, 0xCD }));
                sb.Append("\"");

                sb.Append(" \"");
                sb.Append(dataTypeEntry.ToString(new byte[] { 0xFF, 0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE, 0xF0, 0x12, 0x34, 0x56, 0x78, 0x9A, 0xCD, 0xEF, 0x01, 0x23, 0x45, 0x67, 0x89 }));
                sb.Append("\"");
            }

            return sb.ToString();
        }

        static bool PrintDataTypeEntry(StreamWriter outStream, UdsReader.DataTypeEntry dataTypeEntry, UdsReader.ParseInfoBase parseInfo = null)
        {
            string prefix = string.Empty;
            if (parseInfo is UdsReader.ParseInfoAdp parseInfoAdp)
            {
                prefix = "ADP: ";
            }
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
            sb.Append(prefix);
            sb.Append(string.Format(CultureInfo.InvariantCulture, "Data type: {0}", UdsReader.DataTypeEntry.DataTypeIdToString(dataTypeEntry.DataTypeId)));

            if (dataTypeEntry.FixedEncodingId != null)
            {
                sb.Append($" (Fixed {dataTypeEntry.FixedEncodingId}");
                if (dataTypeEntry.FixedEncoding == null)
                {
                    sb.Append(" ,Unknown ID");
                    if (_unknownIdDict.TryGetValue(dataTypeEntry.FixedEncodingId.Value, out UInt32 oldValue))
                    {
                        _unknownIdDict[dataTypeEntry.FixedEncodingId.Value] = oldValue + 1;
                    }
                    else
                    {
                        _unknownIdDict[dataTypeEntry.FixedEncodingId.Value] = 1;
                    }
                }
                else
                {
                    if (dataTypeEntry.FixedEncoding.ConvertFunc != null)
                    {
                        sb.Append(" ,Function");
                    }
                }
                sb.Append(")");
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

            if (dataTypeEntry.MinTelLength.HasValue)
            {
                sb.Append(string.Format(CultureInfo.InvariantCulture, "; MinTelLen: {0}", dataTypeEntry.MinTelLength.Value));
            }
            else
            {
                sb.Append("*** No MinTelLen");
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

        static bool PrintIsoErrorCode(UdsReader udsReader, uint errorCode, uint errorDetail)
        {
            return PrintErrorCode(udsReader, errorCode, errorDetail, DataReader.ErrorType.Iso9141);
        }

        static bool PrintKwpErrorCode(UdsReader udsReader, uint errorCode, uint errorDetail)
        {
            return PrintErrorCode(udsReader, errorCode, errorDetail, DataReader.ErrorType.Kwp2000);
        }

        static bool PrintSaeErrorCode(UdsReader udsReader, uint errorCode, uint errorDetail)
        {
            return PrintErrorCode(udsReader, errorCode, errorDetail, DataReader.ErrorType.Sae);
        }

        static bool PrintErrorCode(UdsReader udsReader, uint errorCode, uint errorDetail, DataReader.ErrorType errorType)
        {
            List<string> resultList = udsReader.DataReader.ErrorCodeToString(errorCode, errorDetail, errorType, udsReader);
            if (resultList == null || resultList.Count == 0)
            {
                Console.WriteLine("Error code {0} invalid", errorCode);
                return false;
            }
            foreach (string line in resultList)
            {
                Console.WriteLine(line);
            }
            Console.WriteLine();

            return true;
        }

        static bool PrintSaeErrorDetail(UdsReader udsReader, byte[] data)
        {
            List<string> resultList = udsReader.DataReader.SaeErrorDetailHeadToString(data, udsReader);
            if (resultList == null)
            {
                return false;
            }
            foreach (string line in resultList)
            {
                Console.WriteLine(line);
            }
            Console.WriteLine();

            return true;
        }

        static bool PrintUdsErrorDetail(UdsReader udsReader, byte[] data)
        {
            List<string> resultList = udsReader.ErrorDetailBlockToString(null, data);
            if (resultList == null)
            {
                return false;
            }
            foreach (string line in resultList)
            {
                Console.WriteLine(line);
            }
            Console.WriteLine();

            return true;
        }

    }
}
