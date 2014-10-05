using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using NDesk.Options;
using Ediabas;

namespace EdiabasCall
{
    class Program
    {
        [DllImport("api32.dll", EntryPoint = "__apiResultText")]
        private static extern bool __api32ResultText(uint handle, byte[] buf, string result, ushort set, string format);

        [DllImport("api32.dll", EntryPoint = "__apiResultChar")]
        private static extern bool __api32ResultChar(uint handle, out byte buf, string result, ushort set);

        private static readonly CultureInfo culture = CultureInfo.CreateSpecificCulture("en");
        private static Encoding encoding = Encoding.GetEncoding(1252);
        private static TextWriter outputWriter;
        private static bool compareOutput = false;
        private static uint apiHandle = 0;
        private static string lastJobInfo = string.Empty;
        private static int lastJobProgress = -1;

        static int Main(string[] args)
        {
            string sgbdFile = null;
            string outFile = null;
            bool appendFile = false;
            List<string> jobNames = new List<string>();
            bool show_help = false;

            var p = new OptionSet()
            {
                { "s|sgbd=", "sgbd file.",
                  v => sgbdFile = v },
                { "o|out=", "output file name.",
                  v => outFile = v },
                { "a|append", "append output file.",
                  v => appendFile = v != null },
                { "c|compare", "compare output.",
                  v => compareOutput = v != null },
                { "j|job=", "<job name>#<job parameters semicolon separated>#<request results semicolon separated>.",
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

            if (outFile == null)
            {
                outputWriter = Console.Out;
            }
            else
            {
                outputWriter = new StreamWriter(outFile, appendFile, encoding);
            }

            try
            {
                if (sgbdFile == null)
                {
                    outputWriter.WriteLine("No sgbd file specified");
                    return 1;
                }

                if (jobNames.Count < 1)
                {
                    outputWriter.WriteLine("No jobs specified");
                    return 1;
                }

                if (!API.apiInit())
                {
                    outputWriter.WriteLine("Init api failed");
                    return 1;
                }

                Type type = typeof(API);
                FieldInfo info = type.GetField("a", BindingFlags.NonPublic | BindingFlags.Static);
                if (info != null)
                {
                    object value = info.GetValue(null);
                    if (value.GetType() == typeof(uint))
                    {
                        apiHandle = (uint)value;
                    }
                }

                API.apiSetConfig("EcuPath", Path.GetDirectoryName(sgbdFile));

                string sgbdBaseFile = Path.GetFileNameWithoutExtension(sgbdFile);
                foreach (string jobString in jobNames)
                {
                    if (jobString.Length == 0)
                    {
                        outputWriter.WriteLine("Empty job string");
                        API.apiEnd();
                        return 1;
                    }

                    string[] parts = jobString.Split('#');
                    if ((parts.Length < 1) || (parts[0].Length == 0))
                    {
                        outputWriter.WriteLine("Empty job name");
                        API.apiEnd();
                        return 1;
                    }
                    string jobName = parts[0];
                    string jobArgs = string.Empty;
                    string jobResults = string.Empty;
                    if (parts.Length >= 2)
                    {
                        jobArgs = parts[1];
                    }
                    if (parts.Length >= 3)
                    {
                        jobResults = parts[2];
                    }
                    outputWriter.WriteLine("JOB: " + jobName);

                    API.apiJob(sgbdBaseFile, jobName, jobArgs, jobResults);

                    lastJobInfo = string.Empty;
                    lastJobProgress = -1;
                    while (API.apiState() == API.APIBUSY)
                    {
                        PrintProgress();
                        Thread.Sleep(10);
                    }
                    if (API.apiState() == API.APIERROR)
                    {
                        if (compareOutput)
                        {
                            outputWriter.WriteLine(string.Format(culture, "Error occured: 0x{0:X08}", API.apiErrorCode()));
                        }
                        else
                        {
                            outputWriter.WriteLine(string.Format(culture, "Error occured: {0}", API.apiErrorText()));
                        }
                        API.apiEnd();
                        return 1;
                    }
                    PrintProgress();
                    PrintResults(true);
                }

                API.apiEnd();
            }
            finally
            {
                outputWriter.Close();
            }

            return 0;
        }

        static void PrintProgress()
        {
            string jobInfo = string.Empty;
            int jobProgress = API.apiJobInfo(out jobInfo);
            if ((jobProgress != lastJobProgress) || (jobInfo != lastJobInfo))
            {
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
            lastJobProgress = jobProgress;
            lastJobInfo = jobInfo;
        }

        static void PrintResults(bool ignoreSet0)
        {
            ushort resultSets;
            if (API.apiResultSets(out resultSets))
            {
                for (ushort set = 0; set <= resultSets; set++)
                {
                    if (API.apiErrorCode() != API.EDIABAS_ERR_NONE)
                    {
                        break;
                    }
                    if (ignoreSet0 && set == 0)
                    {
                        continue;
                    }
                    outputWriter.WriteLine(string.Format(culture, "DATASET: {0}", set));
                    ushort results;
                    if (API.apiResultNumber(out results, set))
                    {
                        Dictionary<string, string> resultDict = new Dictionary<string,string>();
                        for (ushort result = 1; result <= results; result++)
                        {
                            if (API.apiErrorCode() != API.EDIABAS_ERR_NONE)
                            {
                                break;
                            }
                            string resultName;
                            if (API.apiResultName(out resultName, result, set))
                            {
                                string resultText = string.Empty;
                                int resultFormat;

                                if (API.apiResultFormat(out resultFormat, resultName, set))
                                {
                                    switch (resultFormat)
                                    {
                                        case API.APIFORMAT_CHAR:
                                            {
                                                if (apiHandle == 0)
                                                {
                                                    char resultChar;
                                                    if (API.apiResultChar(out resultChar, resultName, set))
                                                    {
                                                        resultText = string.Format(culture, "{0} 0x{0:X02}", (sbyte)resultChar);
                                                    }
                                                }
                                                else
                                                {
                                                    byte resultByte;
                                                    if (__api32ResultChar(apiHandle, out resultByte, resultName, set))
                                                    {
                                                        resultText = string.Format(culture, "{0} 0x{0:X02}", (sbyte)resultByte);
                                                    }
                                                }
                                                break;
                                            }

                                        case API.APIFORMAT_BYTE:
                                            {
                                                byte resultByte;
                                                if (API.apiResultByte(out resultByte, resultName, set))
                                                {
                                                    resultText = string.Format(culture, "{0} 0x{0:X02}", resultByte);
                                                }
                                                break;
                                            }

                                        case API.APIFORMAT_INTEGER:
                                            {
                                                short resultShort;
                                                if (API.apiResultInt(out resultShort, resultName, set))
                                                {
                                                    resultText = string.Format(culture, "{0} 0x{0:X04}", resultShort);
                                                }
                                                break;
                                            }

                                        case API.APIFORMAT_WORD:
                                            {
                                                ushort resultWord;
                                                if (API.apiResultWord(out resultWord, resultName, set))
                                                {
                                                    resultText = string.Format(culture, "{0} 0x{0:X04}", resultWord);
                                                }
                                                break;
                                            }

                                        case API.APIFORMAT_LONG:
                                            {
                                                int resultInt;
                                                if (API.apiResultLong(out resultInt, resultName, set))
                                                {
                                                    resultText = string.Format(culture, "{0} 0x{0:X08}", resultInt);
                                                }
                                                break;
                                            }

                                        case API.APIFORMAT_DWORD:
                                            {
                                                uint resultUint;
                                                if (API.apiResultDWord(out resultUint, resultName, set))
                                                {
                                                    resultText = string.Format(culture, "{0} 0x{0:X08}", resultUint);
                                                }
                                                break;
                                            }

                                        case API.APIFORMAT_REAL:
                                            {
                                                double resultDouble;
                                                if (API.apiResultReal(out resultDouble, resultName, set))
                                                {
                                                    resultText = string.Format(culture, "{0}", resultDouble);
                                                }
                                                break;
                                            }

                                        case API.APIFORMAT_TEXT:
                                            {
                                                if (apiHandle == 0)
                                                {
                                                    string resultString;
                                                    if (API.apiResultText(out resultString, resultName, set, ""))
                                                    {
                                                        resultText = resultString;
                                                    }
                                                }
                                                else
                                                {
                                                    byte[] dataBuffer = new byte[API.APIMAXTEXT];
                                                    if (__api32ResultText(apiHandle, dataBuffer, resultName, set, ""))
                                                    {
                                                        int length = Array.IndexOf(dataBuffer, (byte)0);
                                                        if (length < 0)
                                                        {
                                                            length = API.APIMAXTEXT;
                                                        }
                                                        resultText = encoding.GetString(dataBuffer, 0, length);
                                                    }
                                                }
                                                break;
                                            }

                                        case API.APIFORMAT_BINARY:
                                            {
                                                byte[] resultByteArray;
                                                uint resultLength;
                                                if (API.apiResultBinaryExt(out resultByteArray, out resultLength, API.APIMAXBINARYEXT, resultName, set))
                                                {
                                                    for (int i = 0; i < resultLength; i++)
                                                    {
                                                        resultText += string.Format(culture, "{0:X02} ", resultByteArray[i]);
                                                    }
                                                }
                                                break;
                                            }
                                    }
                                }

                                if (!resultDict.ContainsKey(resultName))
                                {
                                    resultDict.Add(resultName, resultText);
                                }
                            }
                        }

                        foreach (string key in resultDict.Keys.OrderBy(x => x))
                        {
                            string resultText = resultDict[key];
                            outputWriter.WriteLine(key + ": " + resultText);
                        }
                    }
                }
                outputWriter.WriteLine();
            }
        }

        static void ShowHelp(OptionSet p)
        {
            Console.WriteLine("Usage: " + Path.GetFileNameWithoutExtension(System.AppDomain.CurrentDomain.FriendlyName) + " [OPTIONS]");
            Console.WriteLine("EDIABAS call");
            Console.WriteLine();
            Console.WriteLine("Options:");
            p.WriteOptionDescriptions(Console.Out);
        }
    }
}
