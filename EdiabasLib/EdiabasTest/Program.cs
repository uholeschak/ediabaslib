using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using EdiabasLib;
using NDesk.Options;

namespace EdiabasTest
{
    class Program
    {
        private static readonly CultureInfo culture = CultureInfo.CreateSpecificCulture("en");
        private static Encoding encoding = Encoding.GetEncoding(1252);
        private static TextWriter outputWriter;
        private static bool compareOutput = false;
        private static List<List<Dictionary<string, EdiabasNet.ResultData>>> apiResultList;

        static int Main(string[] args)
        {
            string cfgString = null;
            string sgbdFile = null;
            string comPort = null;
            string outFile = null;
            string ifhName = string.Empty;
            bool appendFile = false;
            bool storeResults = false;
            bool printAllTypes = false;
            List<string> formatList = new List<string>();
            List<string> jobNames = new List<string>();
            bool show_help = false;

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
                { "store", "store results.",
                  v => storeResults = v != null },
                { "c|compare", "compare output.",
                  v => compareOutput = v != null },
                { "alltypes", "print all value types.",
                  v => printAllTypes = v != null },
                { "f|format=", "format for specific result. <result name>=<format string>",
                  v => formatList.Add(v) },
                { "j|job=", "<job name>#<job parameters semicolon separated>#<request results semicolon separated>#<standard job parameters semicolon separated>.\nFor binary job parameters prepend the hex string with| (e.g. |A3C2)",
                  v => jobNames.Add(v) },
                { "h|help",  "show this message and exit", 
                  v => show_help = v != null },
            };

            List<string> extra;
            try
            {
                extra = p.Parse(args);
            }
            catch (OptionException e)
            {
                string thisName = Path.GetFileNameWithoutExtension(System.AppDomain.CurrentDomain.FriendlyName);
                Console.Write(thisName + ": ");
                Console.WriteLine(e.Message);
                Console.WriteLine("Try `" + thisName + " --help' for more information.");
                return 1;
            }

            if (show_help)
            {
                ShowHelp(p);
                return 0;
            }

            if (string.IsNullOrEmpty(outFile))
            {
                outputWriter = Console.Out;
            }
            else
            {
                outputWriter = new StreamWriter(outFile, appendFile, encoding);
            }

            try
            {
                if (string.IsNullOrEmpty(sgbdFile))
                {
                    outputWriter.WriteLine("No sgbd file specified");
                    return 1;
                }

                if (jobNames.Count < 1)
                {
                    outputWriter.WriteLine("No jobs specified");
                    return 1;
                }

                outputWriter.WriteLine(string.Format("API Version: {0}.{1}.{2}", (EdiabasNet.EdiabasVersion >> 8) & 0xF, (EdiabasNet.EdiabasVersion >> 4) & 0xF, EdiabasNet.EdiabasVersion & 0xF));

                if (storeResults)
                {
                    apiResultList = new List<List<Dictionary<string, EdiabasNet.ResultData>>>();
                }

                using (EdiabasNet ediabas = new EdiabasNet(cfgString))
                {
                    if (string.IsNullOrEmpty(ifhName))
                    {
                        ifhName = ediabas.GetConfigProperty("Interface");
                    }
                    EdInterfaceBase edInterface = new EdInterfaceObd();
                    if (!string.IsNullOrEmpty(ifhName))
                    {
                        if (!edInterface.IsValidInterfaceName(ifhName))
                        {
                            edInterface.Dispose();
                            edInterface = new EdInterfaceAds();
                            if (!edInterface.IsValidInterfaceName(ifhName))
                            {
                                edInterface.Dispose();
                                edInterface = new EdInterfaceEnet();
                                if (!edInterface.IsValidInterfaceName(ifhName))
                                {
                                    edInterface.Dispose();
                                    outputWriter.WriteLine("Interface not valid");
                                    return 1;
                                }
                            }
                        }
                    }

                    ediabas.EdInterfaceClass = edInterface;
                    ediabas.ProgressJobFunc = ProgressJobFunc;
                    ediabas.ErrorRaisedFunc = ErrorRaisedFunc;
                    if (!string.IsNullOrEmpty(comPort))
                    {
                        if (edInterface is EdInterfaceObd)
                        {
                            ((EdInterfaceObd)edInterface).ComPort = comPort;
                        }
                        else if (edInterface is EdInterfaceAds)
                        {
                            ((EdInterfaceAds)edInterface).ComPort = comPort;
                        }
                        else if (edInterface is EdInterfaceEnet)
                        {
                            ((EdInterfaceEnet)edInterface).RemoteHost = comPort;
                        }
                    }

                    ediabas.SetConfigProperty("EcuPath", Path.GetDirectoryName(sgbdFile));

                    foreach (string jobString in jobNames)
                    {
                        if (jobString.Length == 0)
                        {
                            outputWriter.WriteLine("Empty job string");
                            return 1;
                        }

                        ediabas.ArgBinary = null;
                        ediabas.ArgBinaryStd = null;
                        ediabas.ResultsRequests = string.Empty;
                        string[] parts = jobString.Split('#');
                        if ((parts.Length < 1) || (parts[0].Length == 0))
                        {
                            outputWriter.WriteLine("Empty job name");
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

                        try
                        {
                            ediabas.ResolveSgbdFile(Path.GetFileNameWithoutExtension(sgbdFile));
                        }
                        catch (Exception ex)
                        {
                            outputWriter.WriteLine("ResolveSgbdFile failed: " + EdiabasNet.GetExceptionText(ex));
                            return 1;
                        }

                        outputWriter.WriteLine("JOB: " + jobName);
                        try
                        {
                            ediabas.ExecuteJob(jobName);
                        }
                        catch (Exception ex)
                        {
                            if (!compareOutput || ediabas.ErrorCodeLast == EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE)
                            {
                                outputWriter.WriteLine("Job execution failed: " + EdiabasNet.GetExceptionText(ex));
                            }
                            return 1;
                        }

                        List<Dictionary<string, EdiabasNet.ResultData>> resultSets = ediabas.ResultSets;
                        if (apiResultList != null)
                        {
                            apiResultList.Add(resultSets);
                        }
                        else
                        {
                            PrintResults(ediabas, formatList, printAllTypes, resultSets);
                        }

                        //Console.WriteLine("Press Key to continue");
                        //Console.ReadKey(true);
                    }

                    if (apiResultList != null)
                    {
                        foreach (List<Dictionary<string, EdiabasNet.ResultData>> resultSets in apiResultList)
                        {
                            PrintResults(ediabas, formatList, printAllTypes, resultSets);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                outputWriter.WriteLine(ex.Message);
                return 1;
            }
            finally
            {
                outputWriter.Close();
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
                outputWriter.WriteLine("Progress: " + message);
            }
        }

        static void ErrorRaisedFunc(EdiabasNet.ErrorCodes error)
        {
            string errorText = EdiabasNet.GetErrorDescription(error);
            outputWriter.WriteLine(string.Format(culture, "Error occured: 0x{0:X08} {1}", (UInt32)error, errorText));
        }

        static void PrintResults(EdiabasNet ediabas, List<string> formatList, bool printAllTypes, List<Dictionary<string, EdiabasNet.ResultData>> resultSets)
        {
            int dataSet = 0;
            if (resultSets != null)
            {
                if (resultSets.Count > 0)
                {
                    EdiabasNet.ResultData resultData;
                    if (resultSets[0].TryGetValue("VARIANTE", out resultData))
                    {
                        if (resultData.OpData.GetType() == typeof(string))
                        {
                            outputWriter.WriteLine("Variant: " + (string)resultData.OpData);
                        }
                    }
                }

                foreach (Dictionary<string, EdiabasNet.ResultData> resultDict in resultSets)
                {
                    outputWriter.WriteLine(string.Format(culture, "DATASET: {0}", dataSet));
                    foreach (string key in resultDict.Keys.OrderBy(x => x))
                    {
                        EdiabasNet.ResultData resultData = resultDict[key];
                        string resultText = string.Empty;
                        if (resultData.OpData.GetType() == typeof(string))
                        {
                            resultText = (string)resultData.OpData;
                        }
                        else if (resultData.OpData.GetType() == typeof(Double))
                        {
                            resultText = string.Format(culture, "R: {0}", (Double)resultData.OpData);
                        }
                        else if (resultData.OpData.GetType() == typeof(Int64))
                        {
                            Int64 value = (Int64)resultData.OpData;
                            switch (resultData.ResType)
                            {
                                case EdiabasNet.ResultType.TypeB:  // 8 bit
                                    resultText = string.Format(culture, "B: {0} 0x{1:X02}", value, (Byte)value);
                                    break;

                                case EdiabasNet.ResultType.TypeC:  // 8 bit char
                                    resultText = string.Format(culture, "C: {0} 0x{1:X02}", value, (Byte)value);
                                    break;

                                case EdiabasNet.ResultType.TypeW:  // 16 bit
                                    resultText = string.Format(culture, "W: {0} 0x{1:X04}", value, (UInt16)value);
                                    break;

                                case EdiabasNet.ResultType.TypeI:  // 16 bit signed
                                    resultText = string.Format(culture, "I: {0} 0x{1:X04}", value, (UInt16)value);
                                    break;

                                case EdiabasNet.ResultType.TypeD:  // 32 bit
                                    resultText = string.Format(culture, "D: {0} 0x{1:X08}", value, (UInt32)value);
                                    break;

                                case EdiabasNet.ResultType.TypeL:  // 32 bit signed
                                    resultText = string.Format(culture, "L: {0} 0x{1:X08}", value, (UInt32)value);
                                    break;

                                default:
                                    resultText = "?";
                                    break;
                            }
                        }
                        else if (resultData.OpData.GetType() == typeof(byte[]))
                        {
                            byte[] data = (byte[])resultData.OpData;
                            foreach (byte value in data)
                            {
                                resultText += string.Format(culture, "{0:X02} ", value);
                            }
                        }

                        if (printAllTypes)
                        {
                            if ((resultData.OpData.GetType() == typeof(Int64)) || (resultData.OpData.GetType() == typeof(Double)))
                            {
                                resultText += " ALL: ";
                                if (resultData.OpData.GetType() == typeof(Int64))
                                {
                                    Int64 value = (Int64)resultData.OpData;
                                    resultText += string.Format(culture, " {0}", (sbyte)value);
                                    resultText += string.Format(culture, " {0}", (byte)value);
                                    resultText += string.Format(culture, " {0}", (short)value);
                                    resultText += string.Format(culture, " {0}", (ushort)value);
                                    resultText += string.Format(culture, " {0}", (int)value);
                                    resultText += string.Format(culture, " {0}", (uint)value);
                                    resultText += string.Format(culture, " {0}", (double)value);
                                }
                                else if (resultData.OpData.GetType() == typeof(Double))
                                {
                                    Double valueDouble = (Double)resultData.OpData;
                                    Int64 value = (Int64)valueDouble;
                                    resultText += string.Format(culture, " {0}", (sbyte)value);
                                    resultText += string.Format(culture, " {0}", (byte)value);
                                    resultText += string.Format(culture, " {0}", (short)value);
                                    resultText += string.Format(culture, " {0}", (ushort)value);
                                    resultText += string.Format(culture, " {0}", (int)value);
                                    resultText += string.Format(culture, " {0}", (uint)value);
                                    resultText += string.Format(culture, " {0}", valueDouble);
                                }
                            }
                        }

                        foreach (string format in formatList)
                        {
                            string[] words = format.Split(new char[] { '=' }, 2);
                            if (words.Length == 2)
                            {
                                if (string.Compare(words[0], resultData.Name, StringComparison.OrdinalIgnoreCase) == 0)
                                {
                                    string resultString = EdiabasNet.FormatResult(resultData, words[1]);
                                    if (resultString != null)
                                    {
                                        resultText += " F: '" + resultString + "'";
                                    }
                                    else
                                    {
                                        ErrorRaisedFunc(EdiabasNet.ErrorCodes.EDIABAS_API_0005);
                                    }
                                }
                            }
                        }

                        outputWriter.WriteLine(resultData.Name + ": " + resultText);
                    }
                    dataSet++;
                }
                outputWriter.WriteLine();
            }
        }

        static void ShowHelp(OptionSet p)
        {
            Console.WriteLine("Usage: " + Path.GetFileNameWithoutExtension(System.AppDomain.CurrentDomain.FriendlyName) + " [OPTIONS]");
            Console.WriteLine("EDIABAS simulator");
            Console.WriteLine();
            Console.WriteLine("Options:");
            p.WriteOptionDescriptions(Console.Out);
        }
    }
}
