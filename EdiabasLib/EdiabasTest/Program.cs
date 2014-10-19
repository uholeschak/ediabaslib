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

        static int Main(string[] args)
        {
            string cfgString = null;
            string sgbdFile = null;
            string comPort = null;
            string outFile = null;
            bool appendFile = false;
            string logFile = null;
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
                { "c|compare", "compare output.",
                  v => compareOutput = v != null },
                { "l|log=", "log file name.",
                  v => logFile = v },
                { "j|job=", "<job name>#<job parameters semicolon separated>#<request results semicolon separated>#<standard job parameters semicolon separated>.\nFor binary job parameters perpend the hex string with| (e.g. |A3C2)",
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

                if (string.IsNullOrEmpty(comPort))
                {
                    outputWriter.WriteLine("No COM port specified");
                    return 1;
                }

                if (jobNames.Count < 1)
                {
                    outputWriter.WriteLine("No jobs specified");
                    return 1;
                }

                using (EdiabasNet ediabas = new EdiabasNet(cfgString))
                {
                    EdCommObd edCommBwmFast = new EdCommObd(ediabas);
                    edCommBwmFast.ComPort = comPort;
                    ediabas.EdCommClass = edCommBwmFast;
                    ediabas.ProgressJobFunc = ProgressJobFunc;
                    ediabas.ErrorRaisedFunc = ErrorRaisedFunc;

                    ediabas.SetConfigProperty("EcuPath", Path.GetDirectoryName(sgbdFile));
                    if (logFile != null)
                    {
                        ediabas.SwLog = new StreamWriter(logFile);
                    }

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

                        int dataSet = 0;
                        List<Dictionary<string, EdiabasNet.ResultData>> resultSets = ediabas.ResultSets;
                        if (resultSets != null)
                        {
                            foreach (Dictionary<string, EdiabasNet.ResultData> resultDict in resultSets)
                            {
                                outputWriter.WriteLine(string.Format(culture, "DATASET: {0}", dataSet));
                                if (ediabas.SwLog != null)
                                {
                                    ediabas.SwLog.WriteLine(string.Format(culture, "DATASET: {0}", dataSet));
                                }
                                foreach (string key in resultDict.Keys.OrderBy(x => x))
                                {
                                    EdiabasNet.ResultData resultData = resultDict[key];
                                    string resultText = string.Empty;
                                    if (resultData.opData.GetType() == typeof(string))
                                    {
                                        resultText = (string)resultData.opData;
                                    }
                                    else if (resultData.opData.GetType() == typeof(Double))
                                    {
                                        resultText = string.Format(culture, "R: {0}", (Double)resultData.opData);
                                    }
                                    else if (resultData.opData.GetType() == typeof(Int64))
                                    {
                                        Int64 value = (Int64)resultData.opData;
                                        switch (resultData.type)
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
                                    else if (resultData.opData.GetType() == typeof(byte[]))
                                    {
                                        byte[] data = (byte[])resultData.opData;
                                        foreach (byte value in data)
                                        {
                                            resultText += string.Format(culture, "{0:X02} ", value);
                                        }
                                    }
                                    outputWriter.WriteLine(resultData.name + ": " + resultText);
                                    if (ediabas.SwLog != null)
                                    {
                                        ediabas.SwLog.WriteLine(resultData.name + ": " + resultText);
                                    }
                                }
                                dataSet++;
                            }
                            outputWriter.WriteLine();
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
