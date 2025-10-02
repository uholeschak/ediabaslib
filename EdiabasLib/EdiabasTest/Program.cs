using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using EdiabasLib;
using NDesk.Options;

// ReSharper disable LoopCanBeConvertedToQuery
// ReSharper disable CanBeReplacedWithTryCastAndCheckForNull

namespace EdiabasTest
{
    static class Program
    {
        private static readonly CultureInfo Culture = CultureInfo.CreateSpecificCulture("en");
        private static readonly Encoding Encoding = Encoding.GetEncoding(1252);
        private static TextWriter _outputWriter;
        private static bool _compareOutput;
        private static List<List<Dictionary<string, EdiabasNet.ResultData>>> _apiResultList;
        private static bool _api76;
        private static bool _errorPrinted;

        static int Main(string[] args)
        {
#if NET
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
#endif
            string cfgString = null;
            string sgbdFile = null;
            string comPort = null;
            string outFile = null;
            string ifhName = string.Empty;
            string deviceName = string.Empty;
            bool appendFile = false;
            bool storeResults = false;
            bool printAllTypes = false;
            List<string> formatList = new List<string>();
            List<string> jobNames = new List<string>();
            bool showHelp = false;

            var p = new OptionSet()
            {
                { "cfg=", "config string.",
                  v => cfgString = v },
                { "s|sgbd=", "sgbd file.",
                  v => sgbdFile = v },
                { "p|port=", "COM port.",
                  v => comPort = v },
                { "o|out=", "output file name.",
                  v => outFile = v },
                { "a|append", "append output file.",
                  v => appendFile = v != null },
                { "ifh=", "interface handler.",
                  v => ifhName = v },
                { "device=", "Device name.",
                    v => deviceName = v },
                { "store", "store results.",
                  v => storeResults = v != null },
                { "c|compare", "compare output.",
                  v => _compareOutput = v != null },
                { "alltypes", "print all value types.",
                  v => printAllTypes = v != null },
                { "f|format=", "format for specific result. <result name>=<format string>",
                  v => formatList.Add(v) },
                { "j|job=", "<job name>#<job parameters semicolon separated>#<request results semicolon separated>#<standard job parameters semicolon separated>.\nFor binary job parameters prepend the hex string with| (e.g. |A3C2)",
                  v => jobNames.Add(v) },
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

            _outputWriter = string.IsNullOrEmpty(outFile) ? Console.Out : new StreamWriter(outFile, appendFile, Encoding);

            try
            {
                if (string.IsNullOrEmpty(sgbdFile))
                {
                    _outputWriter.WriteLine("No sgbd file specified");
                    return 1;
                }

                if (jobNames.Count < 1)
                {
                    _outputWriter.WriteLine("No jobs specified");
                    return 1;
                }

                _outputWriter.WriteLine("API Version: {0}", EdiabasNet.EdiabasVersionString);

                if (EdiabasNet.IsMinVersion760)
                {
                    _api76 = true;
                }

                if (storeResults)
                {
                    _apiResultList = new List<List<Dictionary<string, EdiabasNet.ResultData>>>();
                }

                using (EdiabasNet ediabas = new EdiabasNet(cfgString))
                {
                    if (string.IsNullOrEmpty(ifhName))
                    {
                        ifhName = ediabas.GetConfigProperty("Interface");
                    }

                    EdInterfaceBase edInterface;
                    if (!string.IsNullOrEmpty(ifhName))
                    {
                        if (EdInterfaceObd.IsValidInterfaceNameStatic(ifhName))
                        {
                            edInterface = new EdInterfaceObd();
                        }
                        else if (EdInterfaceEdic.IsValidInterfaceNameStatic(ifhName))
                        {
                            edInterface = new EdInterfaceEdic();
                        }
                        else if (EdInterfaceAds.IsValidInterfaceNameStatic(ifhName))
                        {
                            edInterface = new EdInterfaceAds();
                        }
                        else if (EdInterfaceEnet.IsValidInterfaceNameStatic(ifhName))
                        {
                            edInterface = new EdInterfaceEnet();
                        }
                        else if (EdInterfaceRplus.IsValidInterfaceNameStatic(ifhName))
                        {
                            edInterface = new EdInterfaceRplus();
                        }
                        else
                        {
                            _outputWriter.WriteLine("Interface not valid");
                            return 1;
                        }
                    }
                    else
                    {
                        edInterface = new EdInterfaceObd();
                    }

                    edInterface.IfhName = ifhName;
                    edInterface.UnitName = deviceName;
                    edInterface.ApplicationName = "EdiabasTest";

                    ediabas.EdInterfaceClass = edInterface;
                    ediabas.ProgressJobFunc = ProgressJobFunc;
                    ediabas.ErrorRaisedFunc = ErrorRaisedFunc;
                    if (!string.IsNullOrEmpty(comPort))
                    {
                        // ReSharper disable ConditionIsAlwaysTrueOrFalse
                        // ReSharper disable IsExpressionAlwaysTrue
                        // ReSharper disable HeuristicUnreachableCode
                        if (edInterface is EdInterfaceObd)
                        {
                            ((EdInterfaceObd)edInterface).ComPort = comPort;
                        }
                        else if (edInterface is EdInterfaceEdic)
                        {
                            ((EdInterfaceEdic)edInterface).ComPort = comPort;
                        }
                        else if (edInterface is EdInterfaceAds)
                        {
                            ((EdInterfaceAds)edInterface).ComPort = comPort;
                        }
                        else if (edInterface is EdInterfaceEnet)
                        {
                            ((EdInterfaceEnet)edInterface).RemoteHost = comPort;
                        }
                        // ReSharper restore ConditionIsAlwaysTrueOrFalse
                        // ReSharper restore once IsExpressionAlwaysTrue
                        // ReSharper restore once HeuristicUnreachableCode
                    }

                    ediabas.SetConfigProperty("EcuPath", Path.GetDirectoryName(sgbdFile));

                    foreach (string jobString in jobNames)
                    {
                        if (jobString.Length == 0)
                        {
                            _outputWriter.WriteLine("Empty job string");
                            return 1;
                        }

                        _errorPrinted = false;
                        ediabas.ArgBinary = null;
                        ediabas.ArgBinaryStd = null;
                        ediabas.ResultsRequests = string.Empty;
                        string[] parts = jobString.Split('#');
                        if ((parts.Length < 1) || (parts[0].Length == 0))
                        {
                            _outputWriter.WriteLine("Empty job name");
                            return 1;
                        }
                        string jobName = parts[0];
                        if (parts.Length >= 2)
                        {
                            string argString = parts[1];
                            if (argString.Length > 0 && argString[0] == '|')
                            {   // binary data
                                ediabas.ArgBinary = EdiabasNet.HexToByteArray(argString.Substring(1));
                            }
                            else
                            {
                                ediabas.ArgString = argString;
                            }
                        }
                        if (parts.Length >= 3)
                        {
                            ediabas.ResultsRequests = parts[2];
                        }
                        if (parts.Length >= 4)
                        {
                            string argString = parts[3];
                            if (argString.Length > 0 && argString[0] == '|')
                            {   // binary data
                                ediabas.ArgBinaryStd = EdiabasNet.HexToByteArray(argString.Substring(1));
                            }
                            else
                            {
                                ediabas.ArgStringStd = argString;
                            }
                        }
                        string sgbdFileUse = sgbdFile;
                        if (parts.Length >= 5)
                        {
                            sgbdFileUse = parts[4];
                        }

                        _outputWriter.WriteLine("JOB: " + jobName);
                        try
                        {
                            ediabas.ResolveSgbdFile(Path.GetFileNameWithoutExtension(sgbdFileUse));
                        }
                        catch (Exception ex)
                        {
                            if (!_compareOutput)
                            {
                                _outputWriter.WriteLine("ResolveSgbdFile failed: " + EdiabasNet.GetExceptionText(ex));
                            }
                            return 1;
                        }

                        try
                        {
                            ediabas.ExecuteJob(jobName);
                        }
                        catch (Exception ex)
                        {
                            if (!_compareOutput || ediabas.ErrorCodeLast == EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE)
                            {
                                _outputWriter.WriteLine("Job execution failed: " + EdiabasNet.GetExceptionText(ex));
                            }
                            return 1;
                        }

                        List<Dictionary<string, EdiabasNet.ResultData>> resultSets = ediabas.ResultSets;
                        if (_apiResultList != null)
                        {
                            _apiResultList.Add(resultSets);
                        }
                        else
                        {
                            PrintResults(formatList, printAllTypes, resultSets);
                        }

                        //Console.WriteLine("Press Key to continue");
                        //Console.ReadKey(true);
                    }

                    if (_apiResultList != null)
                    {
                        foreach (List<Dictionary<string, EdiabasNet.ResultData>> resultSets in _apiResultList)
                        {
                            PrintResults(formatList, printAllTypes, resultSets);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _outputWriter.WriteLine(ex.Message);
                return 1;
            }
            finally
            {
                _outputWriter.Close();
            }

            return 0;
        }

        static void ProgressJobFunc(EdiabasNet ediabas)
        {
            string jobInfo = ediabas.InfoProgressText;
            int jobProgress = ediabas.InfoProgressPercent;

            string message = string.Empty;
            if (jobProgress >= 0)
            {
                message += string.Format("{0,3}% ", jobProgress);
            }
            if (jobInfo.Length > 0)
            {
                message += string.Format("'{0}'", jobInfo);
            }
            if (message.Length > 0)
            {
                _outputWriter.WriteLine("Progress: " + message);
            }
        }

        static void ErrorRaisedFunc(EdiabasNet.ErrorCodes error)
        {
            if (_errorPrinted)
            {
                return;
            }

            string errorText = EdiabasNet.GetErrorDescription(error);
            _outputWriter.WriteLine(string.Format(Culture, "Error occured: 0x{0:X08} {1}", (UInt32)error, errorText));
            _errorPrinted = true;
        }

        static void PrintResults(List<string> formatList, bool printAllTypes, List<Dictionary<string, EdiabasNet.ResultData>> resultSets)
        {
            int dataSet = 0;
            if (resultSets != null)
            {
                if (resultSets.Count > 0)
                {
                    if (resultSets[0].TryGetValue("VARIANTE", out EdiabasNet.ResultData resultData))
                    {
                        if (resultData.OpData is string data)
                        {
                            _outputWriter.WriteLine("Variant: " + data);
                        }
                    }
                }

                foreach (Dictionary<string, EdiabasNet.ResultData> resultDict in resultSets)
                {
                    _outputWriter.WriteLine(string.Format(Culture, "DATASET: {0}", dataSet));
                    foreach (string key in resultDict.Keys.OrderBy(x => x))
                    {
                        EdiabasNet.ResultData resultData = resultDict[key];
                        StringBuilder sbResult = new StringBuilder();

                        if (resultData.OpData is string)
                        {
                            sbResult.Append((string)resultData.OpData);
                        }
                        else if (resultData.OpData is double)
                        {
                            sbResult.Append(string.Format(Culture, "R: {0:0.000000}", (Double)resultData.OpData));
                        }
                        else if (resultData.OpData is long)
                        {
                            Int64 value = (Int64)resultData.OpData;
                            switch (resultData.ResType)
                            {
                                case EdiabasNet.ResultType.TypeB:  // 8 bit
                                    sbResult.Append(string.Format(Culture, "B: {0} 0x{1:X02}", value, (Byte)value));
                                    break;

                                case EdiabasNet.ResultType.TypeC:  // 8 bit char
                                    sbResult.Append(string.Format(Culture, "C: {0} 0x{1:X02}", value, (Byte)value));
                                    break;

                                case EdiabasNet.ResultType.TypeW:  // 16 bit
                                    sbResult.Append(string.Format(Culture, "W: {0} 0x{1:X04}", value, (UInt16)value));
                                    break;

                                case EdiabasNet.ResultType.TypeI:  // 16 bit signed
                                    sbResult.Append(string.Format(Culture, "I: {0} 0x{1:X04}", value, (UInt16)value));
                                    break;

                                case EdiabasNet.ResultType.TypeD:  // 32 bit
                                    sbResult.Append(string.Format(Culture, "D: {0} 0x{1:X08}", value, (UInt32)value));
                                    break;

                                case EdiabasNet.ResultType.TypeL:  // 32 bit signed
                                    sbResult.Append(string.Format(Culture, "L: {0} 0x{1:X08}", value, (UInt32)value));
                                    break;

                                case EdiabasNet.ResultType.TypeQ:  // 64 bit
                                    sbResult.Append(string.Format(Culture, "QW: {0} 0x{1:X016}", value, (UInt64)value));
                                    break;

                                case EdiabasNet.ResultType.TypeLL:  // 64 bit signed
                                    sbResult.Append(string.Format(Culture, "L: {0} 0x{1:X016}", value, (UInt64)value));
                                    break;

                                default:
                                    sbResult.Append("?");
                                    break;
                            }
                        }
                        else if (resultData.OpData.GetType() == typeof(byte[]))
                        {
                            byte[] data = (byte[])resultData.OpData;
                            foreach (byte value in data)
                            {
                                sbResult.Append(string.Format(Culture, "{0:X02} ", value));
                            }
                        }

                        if (printAllTypes)
                        {
                            if ((resultData.OpData is long) || (resultData.OpData is double))
                            {
                                sbResult.Append(" ALL: ");
                                if (resultData.OpData is long)
                                {
                                    Int64 value = (Int64)resultData.OpData;
                                    sbResult.Append(string.Format(Culture, " {0}", (sbyte)value));
                                    sbResult.Append(string.Format(Culture, " {0}", (byte)value));
                                    sbResult.Append(string.Format(Culture, " {0}", (short)value));
                                    sbResult.Append(string.Format(Culture, " {0}", (ushort)value));
                                    sbResult.Append(string.Format(Culture, " {0}", (int)value));
                                    sbResult.Append(string.Format(Culture, " {0}", (uint)value));
                                    sbResult.Append(string.Format(Culture, " {0}", (double)value));
                                    if (_api76)
                                    {
                                        switch (resultData.ResType)
                                        {
                                            case EdiabasNet.ResultType.TypeQ:
                                            case EdiabasNet.ResultType.TypeLL:
                                                sbResult.Append(string.Format(Culture, " {0}", (ulong)value));
                                                sbResult.Append(string.Format(Culture, " {0}", (long)value));
                                                break;

                                            case EdiabasNet.ResultType.TypeD:
                                                sbResult.Append(string.Format(Culture, " {0}", (uint)value));
                                                sbResult.Append(string.Format(Culture, " {0}", (uint)value));
                                                break;

                                            default:
                                                sbResult.Append(string.Format(Culture, " {0}", (int)value));
                                                sbResult.Append(string.Format(Culture, " {0}", (uint)value));
                                                break;
                                        }
                                    }
                                }
                                else
                                {
                                    Double valueDouble = (Double)resultData.OpData;
                                    Int64 value = (Int64)valueDouble;
                                    sbResult.Append(string.Format(Culture, " {0}", (sbyte)value));
                                    sbResult.Append(string.Format(Culture, " {0}", (byte)value));
                                    sbResult.Append(string.Format(Culture, " {0}", (short)value));
                                    sbResult.Append(string.Format(Culture, " {0}", (ushort)value));
                                    sbResult.Append(string.Format(Culture, " {0}", (int)value));
                                    sbResult.Append(string.Format(Culture, " {0}", (uint)value));
                                    sbResult.Append(string.Format(Culture, " {0}", valueDouble));
                                    if (_api76)
                                    {
                                        switch (resultData.ResType)
                                        {
                                            case EdiabasNet.ResultType.TypeQ:
                                            case EdiabasNet.ResultType.TypeLL:
                                                sbResult.Append(string.Format(Culture, " {0}", (ulong)value));
                                                sbResult.Append(string.Format(Culture, " {0}", (long)value));
                                                break;

                                            default:
                                                sbResult.Append(string.Format(Culture, " {0}", (int)value));
                                                sbResult.Append(string.Format(Culture, " {0}", (uint)value));
                                                break;
                                        }
                                    }
                                }
                            }
                        }

                        foreach (string format in formatList)
                        {
                            string[] words = format.Split(new[] { '=' }, 2);
                            if (words.Length == 2)
                            {
                                if (string.Compare(words[0], resultData.Name, StringComparison.OrdinalIgnoreCase) == 0)
                                {
                                    string formatString = words[1];
                                    string resultString = EdiabasNet.FormatResult(resultData, formatString);
                                    if (resultString != null)
                                    {
                                        sbResult.Append(string.Format(" F({0}): '{1}'", formatString, resultString));
                                    }
                                }
                            }
                        }

                        _outputWriter.WriteLine(resultData.Name + ": " + sbResult);
                    }
                    dataSet++;
                }
                _outputWriter.WriteLine();
            }
        }

        static void ShowHelp(OptionSet p)
        {
            Console.WriteLine("Usage: " + Path.GetFileNameWithoutExtension(AppDomain.CurrentDomain.FriendlyName) + " [OPTIONS]");
            Console.WriteLine("EDIABAS simulator");
            Console.WriteLine();
            Console.WriteLine("Options:");
            p.WriteOptionDescriptions(Console.Out);
        }
    }
}
