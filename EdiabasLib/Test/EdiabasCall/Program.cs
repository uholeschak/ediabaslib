using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Ediabas;
using NDesk.Options;

namespace EdiabasCall
{
    static class Program
    {
        public const string Api32DllName = "api32.dll";
        public const string Api64DllName = "api64.dll";

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetDllDirectory(string lpPathName);

        [DllImport(Api32DllName, EntryPoint = "__apiResultText")]
        private static extern bool __api32ResultText(uint handle, byte[] buf, string result, ushort set, string format);

        [DllImport(Api64DllName, EntryPoint = "__apiResultText")]
        private static extern bool __api64ResultText(uint handle, byte[] buf, string result, ushort set, string format);

        [DllImport(Api32DllName, EntryPoint = "__apiResultChar")]
        private static extern bool __api32ResultChar(uint handle, out byte buf, string result, ushort set);

        [DllImport(Api64DllName, EntryPoint = "__apiResultChar")]
        private static extern bool __api64ResultChar(uint handle, out byte buf, string result, ushort set);

        [DllImport(Api32DllName, EntryPoint = "__apiResultLongLong")]
        private static extern bool __api32ResultLongLong(uint handle, out long buf, string result, ushort set);

        [DllImport(Api64DllName, EntryPoint = "__apiResultLongLong")]
        private static extern bool __api64ResultLongLong(uint handle, out long buf, string result, ushort set);

        [DllImport(Api32DllName, EntryPoint = "__apiResultQWord")]
        private static extern bool __api32ResultQWord(uint handle, out ulong buf, string result, ushort set);

        [DllImport(Api64DllName, EntryPoint = "__apiResultQWord")]
        private static extern bool __api64ResultQWord(uint handle, out ulong buf, string result, ushort set);

        private static readonly CultureInfo Culture = CultureInfo.CreateSpecificCulture("en");
        private static readonly Encoding Encoding = Encoding.GetEncoding(1252);
        private static TextWriter _outputWriter;
        private static uint _apiHandle;
        private static List<API.APIRESULTFIELD> _apiResultFields;
        private static string _lastJobInfo = string.Empty;
        private static int _lastJobProgress = -1;
        private static bool _is64Bit;
        private static bool _api6;
        private static bool _api76;

        static int Main(string[] args)
        {
#if NET
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
#endif
            _is64Bit = Environment.Is64BitProcess;

            string cfgString = null;
            string sgbdFile = null;
            string outFile = null;
            string searchPath = null;
            string ifhName = string.Empty;
            string deviceName = string.Empty;
            bool appendFile = false;
            bool storeResults = false;
            bool printAllTypes = false;
            bool printArgs = false;
            List<string> formatList = new List<string>();
            List<string> jobNames = new List<string>();
            bool showHelp = false;

            var p = new OptionSet()
            {
                { "cfg=", "config string.",
                  v => cfgString = v },
                { "s|sgbd=", "sgbd file.",
                  v => sgbdFile = v },
                { "o|out=", "output file name.",
                  v => outFile = v },
                { "p|path=", "dll search path.",
                  v => searchPath = v },
                { "a|append", "append output file.",
                  v => appendFile = v != null },
                { "ifh=", "interface handler.",
                  v => ifhName = v },
                { "device=", "Device name.",
                  v => deviceName = v },
                { "store", "store results.",
                  v => storeResults = v != null },
                { "alltypes", "print all value types.",
                  v => printAllTypes = v != null },
                { "args", "print args.",
                    v => printArgs = v != null },
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

            if (printArgs)
            {
                StringBuilder sbArgs = new StringBuilder();
                foreach (string arg in args)
                {
                    if (sbArgs.Length > 0)
                    {
                        sbArgs.Append(' ');
                    }

                    sbArgs.Append("\"");
                    sbArgs.Append(arg);
                    sbArgs.Append("\"");
                }
                Console.WriteLine("Args: {0}", sbArgs);
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

                if (!string.IsNullOrEmpty(searchPath) && Directory.Exists(searchPath))
                {
                    SetDllDirectory(searchPath);
                }

                _api6 = false;
                if (!API.apiCheckVersion(API.APICOMPATIBILITYVERSION, out string apiVersion))
                {
                    apiVersion = "6.0.0";
                    _api6 = true;
                }
                _outputWriter.WriteLine("API Version: " + apiVersion);

                long apiVerNum = 0;
                string[] versionParts = apiVersion.Split('.');
                if (versionParts.Length == 3)
                {
                    apiVerNum = (long.Parse(versionParts[0]) << 16) + (long.Parse(versionParts[1]) << 8) + long.Parse(versionParts[2]);
                }

                if (apiVerNum >= 0x00070600)
                {
                    _api76 = true;
                }

                string configString = "EcuPath=" + Path.GetDirectoryName(sgbdFile);
                if (!string.IsNullOrEmpty(cfgString))
                {
                    configString = cfgString;
                }

                if (_api6)
                {
                    if (!API.apiInit())
                    {
                        _outputWriter.WriteLine("Init api failed");
                        if (API.apiErrorCode() != API.EDIABAS_ERR_NONE)
                        {
                            _outputWriter.WriteLine(string.Format(Culture, "Error occured: 0x{0:X08} {1}", API.apiErrorCode(), API.apiErrorText()));
                        }
                        return 1;
                    }
                }
                else
                {
                    if (!API.apiInitExt(ifhName, deviceName, "EdiabasCall", configString))
                    {
                        _outputWriter.WriteLine("Init api failed");
                        if (API.apiErrorCode() != API.EDIABAS_ERR_NONE)
                        {
                            _outputWriter.WriteLine(string.Format(Culture, "Error occured: 0x{0:X08} {1}", API.apiErrorCode(), API.apiErrorText()));
                        }
                        return 1;
                    }
                }

                Type type = typeof(API);
                FieldInfo info = type.GetField("a", BindingFlags.NonPublic | BindingFlags.Static);
                object value = info?.GetValue(null);
                if (value is uint handle)
                {
                    _apiHandle = handle;
                }

                if (storeResults)
                {
                    _apiResultFields = new List<API.APIRESULTFIELD>();
                }

                if (!string.IsNullOrEmpty(cfgString))
                {
                    API.apiSetConfig("EcuPath", Path.GetDirectoryName(sgbdFile));
                }

                string sgbdBaseFile = Path.GetFileNameWithoutExtension(sgbdFile);
                foreach (string jobString in jobNames)
                {
                    if (jobString.Length == 0)
                    {
                        _outputWriter.WriteLine("Empty job string");
                        API.apiEnd();
                        return 1;
                    }

                    string[] parts = jobString.Split('#');
                    if ((parts.Length < 1) || (parts[0].Length == 0))
                    {
                        _outputWriter.WriteLine("Empty job name");
                        API.apiEnd();
                        return 1;
                    }
                    string jobName = parts[0];
                    string jobArgs = null;
                    byte[] jobArgsData = null;
                    string jobResults = string.Empty;
                    byte[] jobArgsStdData = null;
                    if (parts.Length >= 2)
                    {
                        string argString = parts[1];
                        if (argString.Length > 0 && argString[0] == '|')
                        {   // binary data
                            jobArgsData = HexToByteArray(argString.Substring(1));
                        }
                        else
                        {
                            jobArgs = argString;
                            jobArgsData = Encoding.GetBytes(argString);
                        }
                    }
                    if (parts.Length >= 3)
                    {
                        jobResults = parts[2];
                    }
                    if (parts.Length >= 4)
                    {
                        string argString = parts[3];
                        if (argString.Length > 0 && argString[0] == '|')
                        {   // binary data
                            jobArgsStdData = HexToByteArray(argString.Substring(1));
                        }
                        else
                        {
                            jobArgsStdData = Encoding.GetBytes(argString);
                        }
                    }
                    string sgbdFileUse = sgbdBaseFile;
                    if (parts.Length >= 5)
                    {
                        sgbdFileUse = Path.GetFileNameWithoutExtension(parts[4]);
                    }
                    _outputWriter.WriteLine("JOB: " + jobName);

                    if (jobArgsStdData != null)
                    {
                        if (jobArgsData == null)
                        {
                            jobArgsData = new byte[0];
                        }
                        API.apiJobExt(sgbdFileUse, jobName, jobArgsStdData, jobArgsStdData.Length, jobArgsData, jobArgsData.Length, jobResults, 0);
                    }
                    else if (jobArgs != null)
                    {
                        API.apiJob(sgbdFileUse, jobName, jobArgs, jobResults);
                    }
                    else
                    {
                        if (jobArgsData == null)
                        {
                            jobArgsData = new byte[0];
                        }
#if false
                        // for test of large buffer handling buffer
                        byte[] buffer = new byte[API.APIMAXBINARY];
                        Array.Copy(jobArgsData, buffer, jobArgsData.Length);
                        API.apiJobData(sgbdFileUse, jobName, buffer, jobArgsData.Length, jobResults);
#else
                        API.apiJobData(sgbdFileUse, jobName, jobArgsData, jobArgsData.Length, jobResults);
#endif
                    }

                    _lastJobInfo = string.Empty;
                    _lastJobProgress = -1;
                    while (API.apiState() == API.APIBUSY)
                    {
                        PrintProgress();
                        Thread.Sleep(10);
                    }
                    if (API.apiState() == API.APIERROR)
                    {
                        _outputWriter.WriteLine(string.Format(Culture, "Error occured: 0x{0:X08} {1}", API.apiErrorCode(), API.apiErrorText()));
                        API.apiEnd();
                        return 1;
                    }
                    PrintProgress();

                    if (_apiResultFields != null)
                    {
                        _apiResultFields.Add(API.apiResultsNew());
                    }
                    else
                    {
                        PrintResults(formatList, printAllTypes);
                    }

                    // for alive check
                    //Console.WriteLine("Press Key to continue");
                    //Console.ReadKey(true);
                }

                if (_apiResultFields != null)
                {
                    foreach (API.APIRESULTFIELD resultField in _apiResultFields)
                    {
                        API.apiResultsScope(resultField);
                        PrintResults(formatList, printAllTypes);
                        API.apiResultsDelete(resultField);
                    }
                }

                API.apiEnd();
            }
            finally
            {
                _outputWriter.Close();
            }

            return 0;
        }

        static void PrintProgress()
        {
            int jobProgress = API.apiJobInfo(out string jobInfo);
            if ((jobProgress != _lastJobProgress) || (jobInfo != _lastJobInfo))
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
                    _outputWriter.WriteLine("Progress: " + message);
                }
            }
            _lastJobProgress = jobProgress;
            _lastJobInfo = jobInfo;
        }

        static void PrintResults(List<string> formatList, bool printAllTypes)
        {
            if (API.apiResultVar(out string variantString))
            {
                _outputWriter.WriteLine("Variant: "+ variantString);
            }

            if (API.apiResultSets(out ushort resultSets))
            {
                for (ushort set = 0; set <= resultSets; set++)
                {
                    _outputWriter.WriteLine(string.Format(Culture, "DATASET: {0}", set));
                    if (API.apiResultNumber(out ushort results, set))
                    {
                        Dictionary<string, string> resultDict = new Dictionary<string,string>();
                        for (ushort result = 1; result <= results; result++)
                        {
                            if (API.apiResultName(out string resultName, result, set))
                            {
                                StringBuilder sbResult = new StringBuilder();

                                if (API.apiResultFormat(out int resultFormat, resultName, set))
                                {
                                    switch (resultFormat)
                                    {
                                        case API.APIFORMAT_CHAR:
                                            {
                                                if (_apiHandle == 0)
                                                {
                                                    if (API.apiResultChar(out char resultChar, resultName, set))
                                                    {
                                                        sbResult.Append(string.Format(Culture, "C: {0} 0x{0:X02}", (sbyte)resultChar));
                                                    }
                                                }
                                                else
                                                {
                                                    // ReSharper disable once InlineOutVariableDeclaration
                                                    byte resultByte;
                                                    bool apiResult = _is64Bit ? __api64ResultChar(_apiHandle, out resultByte, resultName, set) :
                                                        __api32ResultChar(_apiHandle, out resultByte, resultName, set);
                                                    if (apiResult)
                                                    {
                                                        sbResult.Append(string.Format(Culture, "C: {0} 0x{0:X02}", (sbyte)resultByte));
                                                    }
                                                }
                                                break;
                                            }

                                        case API.APIFORMAT_BYTE:
                                            {
                                                if (API.apiResultByte(out byte resultByte, resultName, set))
                                                {
                                                    sbResult.Append(string.Format(Culture, "B: {0} 0x{0:X02}", resultByte));
                                                }
                                                break;
                                            }

                                        case API.APIFORMAT_INTEGER:
                                            {
                                                if (API.apiResultInt(out short resultShort, resultName, set))
                                                {
                                                    sbResult.Append(string.Format(Culture, "I: {0} 0x{0:X04}", resultShort));
                                                }
                                                break;
                                            }

                                        case API.APIFORMAT_WORD:
                                            {
                                                if (API.apiResultWord(out ushort resultWord, resultName, set))
                                                {
                                                    sbResult.Append(string.Format(Culture, "W: {0} 0x{0:X04}", resultWord));
                                                }
                                                break;
                                            }

                                        case API.APIFORMAT_LONG:
                                            {
                                                if (API.apiResultLong(out int resultInt, resultName, set))
                                                {
                                                    sbResult.Append(string.Format(Culture, "L: {0} 0x{0:X08}", resultInt));
                                                }
                                                break;
                                            }

                                        case API.APIFORMAT_DWORD:
                                            {
                                                if (API.apiResultDWord(out uint resultUint, resultName, set))
                                                {
                                                    sbResult.Append(string.Format(Culture, "D: {0} 0x{0:X08}", resultUint));
                                                }
                                                break;
                                            }

                                        case API.APIFORMAT_REAL:
                                            {
                                                if (API.apiResultReal(out double resultDouble, resultName, set))
                                                {
                                                    sbResult.Append(string.Format(Culture, "R: {0:0.000000}", resultDouble));
                                                }
                                                break;
                                            }
#if EDIABAS
                                        case API.APIFORMAT_REAL + 1:
                                            if (_apiHandle != 0)
                                            {
                                                try
                                                {
                                                    // ReSharper disable once InlineOutVariableDeclaration
                                                    long resultLong;
                                                    bool apiResultLL = _is64Bit ? __api64ResultLongLong(_apiHandle, out resultLong, resultName, set) :
                                                        __api32ResultLongLong(_apiHandle, out resultLong, resultName, set);
                                                    if (apiResultLL)
                                                    {
                                                        sbResult.Append(string.Format(Culture, "LL: {0} 0x{0:X016}", resultLong));
                                                    }
                                                }
                                                catch (Exception ex)
                                                {
                                                    sbResult.Append(string.Format(Culture, " '{0}'", ex.Message));
                                                }
                                            }
                                            break;

                                        case API.APIFORMAT_REAL + 2:
                                            if (_apiHandle != 0)
                                            {
                                                try
                                                {
                                                    // ReSharper disable once InlineOutVariableDeclaration
                                                    ulong resultUlong;
                                                    bool apiResultQW = _is64Bit ? __api64ResultQWord(_apiHandle, out resultUlong, resultName, set) :
                                                        __api32ResultQWord(_apiHandle, out resultUlong, resultName, set);
                                                    if (apiResultQW)
                                                    {
                                                        sbResult.Append(string.Format(Culture, "QW: {0} 0x{0:X016}", resultUlong));
                                                    }
                                                }
                                                catch (Exception ex)
                                                {
                                                    sbResult.Append(string.Format(Culture, " '{0}'", ex.Message));
                                                }
                                            }
                                            break;
#else
                                        case API.APIFORMAT_LONGLONG:
                                            {
                                                if (API.apiResultLongLong(out long resultLong, resultName, set))
                                                {
                                                    sbResult.Append(string.Format(Culture, "LL: {0} 0x{0:X016}", resultLong));
                                                }
                                                break;
                                            }

                                        case API.APIFORMAT_QWORD:
                                            {
                                                if (API.apiResultQWord(out ulong resultUlong, resultName, set))
                                                {
                                                    sbResult.Append(string.Format(Culture, "QW: {0} 0x{0:X016}", resultUlong));
                                                }
                                                break;
                                            }
#endif
                                        case API.APIFORMAT_TEXT:
                                            {
                                                if (_apiHandle == 0)
                                                {
                                                    if (API.apiResultText(out string resultString, resultName, set, ""))
                                                    {
                                                        sbResult.Append(resultString);
                                                    }
                                                }
                                                else
                                                {
                                                    byte[] dataBuffer = new byte[API.APIMAXTEXT];
                                                    bool apiResult = _is64Bit ? __api64ResultText(_apiHandle, dataBuffer, resultName, set, "") :
                                                        __api32ResultText(_apiHandle, dataBuffer, resultName, set, "");
                                                    if (apiResult)
                                                    {
                                                        int length = Array.IndexOf(dataBuffer, (byte)0);
                                                        if (length < 0)
                                                        {
                                                            length = API.APIMAXTEXT;
                                                        }
                                                        sbResult.Append(Encoding.GetString(dataBuffer, 0, length));
                                                    }
                                                }
                                                break;
                                            }

                                        case API.APIFORMAT_BINARY:
                                            {
                                                byte[] resultByteArray;
                                                if (_api6)
                                                {
                                                    if (API.apiResultBinary(out resultByteArray, out ushort resultLengthShort, resultName, set))
                                                    {
                                                        for (int i = 0; i < resultLengthShort; i++)
                                                        {
                                                            sbResult.Append(string.Format(Culture, "{0:X02} ", resultByteArray[i]));
                                                        }
                                                    }
                                                    break;
                                                }
                                                if (API.apiResultBinaryExt(out resultByteArray, out uint resultLength, API.APIMAXBINARYEXT, resultName, set))
                                                {
                                                    for (int i = 0; i < resultLength; i++)
                                                    {
                                                        sbResult.Append(string.Format(Culture, "{0:X02} ", resultByteArray[i]));
                                                    }
                                                }
                                                break;
                                            }
                                    }

                                    if (printAllTypes)
                                    {
                                        switch (resultFormat)
                                        {
                                            case API.APIFORMAT_TEXT:
                                            case API.APIFORMAT_BINARY:
                                                break;

                                            default:
                                                sbResult.Append(" ALL: ");
                                                {
                                                    if (_apiHandle == 0)
                                                    {
                                                        if (API.apiResultChar(out char resultChar, resultName, set))
                                                        {
                                                            sbResult.Append(string.Format(Culture, " {0}", (sbyte) resultChar));
                                                        }
                                                    }
                                                    else
                                                    {
                                                        // ReSharper disable once InlineOutVariableDeclaration
                                                        byte resultByte;
                                                        bool apiResult = _is64Bit ? __api64ResultChar(_apiHandle, out resultByte, resultName, set) :
                                                            __api32ResultChar(_apiHandle, out resultByte, resultName, set);
                                                        if (apiResult)
                                                        {
                                                            sbResult.Append(string.Format(Culture, " {0}", (sbyte) resultByte));
                                                        }
                                                    }
                                                }
                                                {
                                                    if (API.apiResultByte(out byte resultByte, resultName, set))
                                                    {
                                                        sbResult.Append(string.Format(Culture, " {0}", resultByte));
                                                    }
                                                    if (API.apiResultInt(out short resultShort, resultName, set))
                                                    {
                                                        sbResult.Append(string.Format(Culture, " {0}", resultShort));
                                                    }
                                                    if (API.apiResultWord(out ushort resultWord, resultName, set))
                                                    {
                                                        sbResult.Append(string.Format(Culture, " {0}", resultWord));
                                                    }
                                                    if (API.apiResultLong(out int resultInt, resultName, set))
                                                    {
                                                        sbResult.Append(string.Format(Culture, " {0}", resultInt));
                                                    }
                                                    if (API.apiResultDWord(out uint resultUint, resultName, set))
                                                    {
                                                        sbResult.Append(string.Format(Culture, " {0}", resultUint));
                                                    }
                                                    if (API.apiResultReal(out double resultDouble, resultName, set))
                                                    {
                                                        sbResult.Append(string.Format(Culture, " {0}", resultDouble));
                                                    }

                                                    if (_api76)
                                                    {
#if EDIABAS
                                                        if (_apiHandle != 0)
                                                        {
                                                            try
                                                            {
                                                                // ReSharper disable once InlineOutVariableDeclaration
                                                                long resultLong;
                                                                bool apiResultLL = _is64Bit ? __api64ResultLongLong(_apiHandle, out resultLong, resultName, set) :
                                                                    __api32ResultLongLong(_apiHandle, out resultLong, resultName, set);
                                                                if (apiResultLL)
                                                                {
                                                                    sbResult.Append(string.Format(Culture, " {0}", resultLong));
                                                                }

                                                                // ReSharper disable once InlineOutVariableDeclaration
                                                                ulong resultUlong;
                                                                bool apiResultQW = _is64Bit ? __api64ResultQWord(_apiHandle, out resultUlong, resultName, set) :
                                                                    __api32ResultQWord(_apiHandle, out resultUlong, resultName, set);
                                                                if (apiResultQW)
                                                                {
                                                                    sbResult.Append(string.Format(Culture, " {0}", resultUlong));
                                                                }
                                                            }
                                                            catch (Exception ex)
                                                            {
                                                                sbResult.Append(string.Format(Culture, " '{0}'", ex.Message));
                                                            }
                                                        }
#else
                                                        if (API.apiResultLongLong(out long resultLong, resultName, set))
                                                        {
                                                            sbResult.Append(string.Format(Culture, " {0}", resultLong));
                                                        }
                                                        if (API.apiResultQWord(out ulong resultUlong, resultName, set))
                                                        {
                                                            sbResult.Append(string.Format(Culture, " {0}", resultUlong));
                                                        }
#endif
                                                    }
                                                }
                                                break;
                                        }
                                    }
                                }

                                foreach (string format in formatList)
                                {
                                    string[] words = format.Split(new[] { '=' }, 2);
                                    if (words.Length == 2)
                                    {
                                        if (string.Compare(words[0], resultName, StringComparison.OrdinalIgnoreCase) == 0)
                                        {
                                            string formatString = words[1];
                                            if (_apiHandle == 0)
                                            {
                                                if (API.apiResultText(out string resultString, resultName, set, formatString))
                                                {
                                                    sbResult.Append(string.Format(" F({0}): '{1}'", formatString, resultString));
                                                }
                                            }
                                            else
                                            {
                                                byte[] dataBuffer = new byte[API.APIMAXTEXT];
                                                bool apiResult = _is64Bit ? __api64ResultText(_apiHandle, dataBuffer, resultName, set, formatString) :
                                                    __api32ResultText(_apiHandle, dataBuffer, resultName, set, formatString);
                                                if (apiResult)
                                                {
                                                    int length = Array.IndexOf(dataBuffer, (byte)0);
                                                    if (length < 0)
                                                    {
                                                        length = API.APIMAXTEXT;
                                                    }
                                                    sbResult.Append(string.Format(" F({0}): '{1}'", formatString, Encoding.GetString(dataBuffer, 0, length)));
                                                }
                                            }
                                        }
                                    }
                                }

                                if (!resultDict.ContainsKey(resultName))
                                {
                                    resultDict.Add(resultName, sbResult.ToString());
                                }
                            }
                        }

                        foreach (string key in resultDict.Keys.OrderBy(x => x))
                        {
                            string resultText = resultDict[key];
                            _outputWriter.WriteLine(key + ": " + resultText);
                        }
                    }
                }
                _outputWriter.WriteLine();
            }
            if (API.apiErrorCode() != API.EDIABAS_ERR_NONE)
            {
                _outputWriter.WriteLine(string.Format(Culture, "Error occured: 0x{0:X08} {1}", API.apiErrorCode(), API.apiErrorText()));
            }
        }

        static byte[] HexToByteArray(string valueStr)
        {
            byte[] result;
            try
            {
                result = Enumerable.Range(0, valueStr.Length)
                 .Where(x => x % 2 == 0)
                 .Select(x => Convert.ToByte(valueStr.Substring(x, 2), 16))
                 .ToArray();
            }
            catch (Exception)
            {
                result = new byte[0];
            }

            return result;
        }

        static void ShowHelp(OptionSet p)
        {
            Console.WriteLine("Usage: " + Path.GetFileNameWithoutExtension(AppDomain.CurrentDomain.FriendlyName) + " [OPTIONS]");
            Console.WriteLine("EDIABAS call");
            Console.WriteLine();
            Console.WriteLine("Options:");
            p.WriteOptionDescriptions(Console.Out);
        }
    }
}
