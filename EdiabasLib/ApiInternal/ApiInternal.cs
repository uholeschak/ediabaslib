using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using EdiabasLib;
// ReSharper disable InconsistentNaming
// ReSharper disable UseNullPropagation
// ReSharper disable once CheckNamespace
// ReSharper disable InlineOutVariableDeclaration
// ReSharper disable MergeCastWithTypeCheck

namespace Ediabas
{
    public class ApiInternal
    {
        public class APIRESULTFIELD
        {
            public List<Dictionary<string, EdiabasNet.ResultData>> ResultSets { get; set; }

            public APIRESULTFIELD(List<Dictionary<string, EdiabasNet.ResultData>> resultSets)
            {
                ResultSets = resultSets;
            }
        }

        public static readonly Encoding Encoding = Encoding.GetEncoding(1252);
        public static readonly CultureInfo Culture = CultureInfo.CreateSpecificCulture("en");
        protected static readonly long TickResolMs = Stopwatch.Frequency / 1000;
        private static List<Assembly> _resourceAssemblies = new List<Assembly>();
        private static bool _firstLog = true;

        private volatile EdiabasNet _ediabas;
        private string _lastIfh;
        private string _lastUnit;
        private string _lastApp;
        private string _lastConfig;
        private readonly object _apiLogLock = new object();
        private StreamWriter _swLog;
        private int _logLevelApi = -1;
        private int _busyCount;
        private volatile int _apiStateValue;
        private volatile int _localError;
        private volatile Thread _jobThread;
        private volatile string _jobName;
        private volatile string _jobEcuName;
        private volatile bool _abortJob;
        private volatile List<Dictionary<string, EdiabasNet.ResultData>> _resultSets;

        public enum ApiLogLevel
        {
            // ReSharper disable UnusedMember.Local
            Off = 0,
            Normal = 1,
            // ReSharper restore UnusedMember.Local
        };

        public const int APICOMPATIBILITYVERSION = 0x0800;
        public const int APICOMPATIBILITYVERSION_MIN = 0x0700;
        public const int APIBUSY = 0;
        public const int APIREADY = 1;
        public const int APIBREAK = 2;
        public const int APIERROR = 3;
        public const int APIMAXDEVICE = 64;
        public const int APIMAXNAME = 64;
        public const int APIMAXPARAEXT = 65536;
        public const int APIMAXPARA = 1024;
        public const int APIMAXSTDPARA = 256;
        public const int APIMAXRESULT = 256;
        public const int APIMAXFILENAME = 256;
        public const int APIMAXCONFIG = 256;
        public const int APIMAXTEXT = 1024;
        public const int APIMAXBINARYEXT = 65536;
        public const int APIMAXBINARY = 1024;
        public const int APIFORMAT_CHAR = 0;
        public const int APIFORMAT_BYTE = 1;
        public const int APIFORMAT_INTEGER = 2;
        public const int APIFORMAT_WORD = 3;
        public const int APIFORMAT_LONG = 4;
        public const int APIFORMAT_DWORD = 5;
        public const int APIFORMAT_TEXT = 6;
        public const int APIFORMAT_BINARY = 7;
        public const int APIFORMAT_REAL = 8;
        public const int APIFORMAT_LONGLONG = 9;
        public const int APIFORMAT_QWORD = 10;
        public const int EDIABAS_ERR_NONE = 0;
        public const int EDIABAS_RESERVED = 1;
        public const int EDIABAS_ERROR_CODE_OUT_OF_RANGE = 2;

        static ApiInternal()
        {
            LoadAllResourceAssemblies();

            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
            {
                string fullName = args.Name;
                if (!string.IsNullOrEmpty(fullName))
                {
                    if (_resourceAssemblies.Count > 0)
                    {
                        foreach (Assembly resourceAssembly in _resourceAssemblies)
                        {
                            try
                            {
                                if (string.Compare(resourceAssembly.FullName, fullName, StringComparison.OrdinalIgnoreCase) == 0)
                                {
                                    return resourceAssembly;
                                }
                            }
                            catch (Exception)
                            {
                                // ignored
                            }
                        }

                        return null;
                    }

                    try
                    {
                        string[] names = fullName.Split(',');
                        if (names.Length < 1)
                        {
                            return null;
                        }

                        string assemblyName = names[0];
                        string assemblyDllName = assemblyName + ".dll";
                        string assemblyDir = AssemblyDirectory;
                        if (string.IsNullOrEmpty(assemblyDir))
                        {
                            return null;
                        }

                        string assemblyFileName = Path.Combine(assemblyDir, assemblyDllName);
                        if (!File.Exists(assemblyFileName))
                        {
                            return null;
                        }

                        return Assembly.LoadFrom(assemblyFileName);
                    }
                    catch (Exception)
                    {
                        return null;
                    }
                }
                return null;
            };
        }

        public ApiInternal() : this(null)
        {
        }

        public ApiInternal(EdiabasNet ediabas)
        {
            _ediabas = ediabas;
            _busyCount = 0;
            _apiStateValue = APIREADY;
            _localError = EDIABAS_ERR_NONE;
            _jobThread = null;
            _jobName = string.Empty;
            _jobEcuName = string.Empty;
            _abortJob = false;
            _resultSets = null;
        }

        public static string AssemblyDirectory
        {
            get
            {
#if NET
                string location = Assembly.GetExecutingAssembly().Location;
                if (string.IsNullOrEmpty(location) || !File.Exists(location))
                {
                    return null;
                }
                return Path.GetDirectoryName(location);
#else
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                if (string.IsNullOrEmpty(path) || !File.Exists(path))
                {
                    return null;
                }
                return Path.GetDirectoryName(path);
#endif
            }
        }

        public static bool LoadAllResourceAssemblies()
        {
            if (_resourceAssemblies.Count > 0)
            {
                return true;
            }

            try
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                string[] resourceNames = assembly.GetManifestResourceNames();

                foreach (string resourceName in resourceNames)
                {
                    if (!resourceName.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                    {
                        if (stream != null)
                        {
                            using (var memoryStream = new MemoryStream())
                            {
                                stream.CopyTo(memoryStream);
                                Assembly loadedAssembly = Assembly.Load(memoryStream.ToArray());
                                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                                if (loadedAssembly != null)
                                {
                                    _resourceAssemblies.Add(loadedAssembly);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }

            return _resourceAssemblies.Count > 0;
        }

        public static void InterfaceDisconnect()
        {
            try
            {
                EdBluetoothInterface.InterfaceDisconnect(true);
            }
            catch (Exception)
            {
                // could happen if Bluetooth is not loaded at startup
                // resolving the assembly will fail here
            }

            try
            {
                EdCustomWiFiInterface.InterfaceDisconnect(true);
            }
            catch (Exception)
            {
                // ignored
            }
        }

        public static bool apiCheckVersion(int versionCompatibility, out string versionInfo)
        {
            versionInfo = string.Empty;
            if (versionCompatibility < APICOMPATIBILITYVERSION_MIN)
            {
                return false;
            }
            versionInfo = string.Format("{0}.{1}.{2}", (EdiabasNet.EdiabasVersion >> 8) & 0xF, (EdiabasNet.EdiabasVersion >> 4) & 0xF, EdiabasNet.EdiabasVersion & 0xF);
            return true;
        }

        public bool apiInit()
        {
            return apiInitExt(null, null, null, null);
        }

        public bool apiInitExt(string ifh, string unit, string app, string config)
        {
            if (_ediabas != null)
            {
                logFormat(ApiLogLevel.Normal, "apiInitExt({0}, {1}, {2}, {3})", ifh, unit, app, config);

                if (_lastIfh == ifh && _lastUnit == unit && _lastApp == app && _lastConfig == config)
                {
                    logFormat(ApiLogLevel.Normal, "={0} ()", true);
                    return true;
                }

                logFormat(ApiLogLevel.Normal, "Settings have changed, calling apiEnd()");
                apiEnd();
            }

            _busyCount = 0;
            _apiStateValue = APIREADY;
            _localError = EDIABAS_ERR_NONE;
            _jobThread = null;
            _jobName = string.Empty;
            _jobEcuName = string.Empty;
            _abortJob = false;
            _resultSets = null;

            setLocalError(EDIABAS_ERR_NONE);

            _ediabas = new EdiabasNet(config);
            logFormat(ApiLogLevel.Normal, "64 bit process: {0}", Environment.Is64BitProcess);
            logFormat(ApiLogLevel.Normal, "apiInitExt({0}, {1}, {2}, {3})", ifh, unit, app, config);

            if (!string.IsNullOrEmpty(unit))
            {
                if (char.IsLetter(unit[0]))
                {
                    setLocalError((int)EdiabasNet.ErrorCodes.EDIABAS_IFH_0018);
                    _ediabas.Dispose();
                    logFormat(ApiLogLevel.Normal, "Unit invalid: {0}", unit);
                    logFormat(ApiLogLevel.Normal, "={0} ()", false);
                    return false;
                }
            }

            if (!string.IsNullOrEmpty(ifh))
            {
                string[] ifhParts = ifh.Split(':');
                if (ifhParts.Length > 0 && string.Compare(ifhParts[0], "REMOTE", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    logFormat(ApiLogLevel.Normal, "Ignoring REMOTE");
                    ifh = null;
                }
#if true
                if (ifhParts.Length > 0 && string.Compare(ifhParts[0], "RPLUS", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    logFormat(ApiLogLevel.Normal, "RPLUS detected");
                    // The command line format is: RPLUS:ICOM_P:Remotehost=X.X.X.X;Port=X
                    if (ifhParts.Length >= 3 && string.Compare(ifhParts[1], "ICOM_P", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        string remoteHost = null;
                        string remotePort = null;
                        string arguments = ifhParts[2];
                        string[] argumentParts = arguments.Split(';');
                        if (argumentParts.Length >= 2)
                        {
                            foreach (string argumentPart in argumentParts)
                            {
                                string[] argumentSubParts = argumentPart.Split('=');
                                if (argumentSubParts.Length > 1)
                                {
                                    string argName = argumentSubParts[0];
                                    string argValue = argumentSubParts[1];
                                    if (string.Compare(argName, "Remotehost", StringComparison.OrdinalIgnoreCase) == 0)
                                    {
                                        remoteHost = argValue;
                                    }
                                    else if (string.Compare(argName, "Port", StringComparison.OrdinalIgnoreCase) == 0)
                                    {
                                        remotePort = argValue;
                                    }
                                }
                            }
                        }

                        if (!string.IsNullOrEmpty(remoteHost) && !string.IsNullOrEmpty(remotePort))
                        {
                            logFormat(ApiLogLevel.Normal, "Host: {0}, Port={1}", remoteHost, remotePort);

                            bool validConfig = true;
                            if (!IPAddress.TryParse(remoteHost, out _))
                            {
                                logFormat(ApiLogLevel.Normal, "Invalid Host: {0}", remoteHost);
                                validConfig = false;
                            }

                            if (!Int64.TryParse(remotePort, out Int64 portValue))
                            {
                                portValue = -1;
                            }

                            if (portValue != EdInterfaceEnet.DiagPortDefault)
                            {
                                logFormat(ApiLogLevel.Normal, "Invalid Port: {0}", portValue);
                                validConfig = false;
                            }

                            if (validConfig)
                            {
                                ifh = "ENET";
                                string enetRemoteHost = string.Format(CultureInfo.InvariantCulture, "{0}:{1}:{2}",
                                    remoteHost, EdInterfaceEnet.IcomDiagPortDefault, EdInterfaceEnet.IcomControlPortDefault);
                                _ediabas.SetConfigProperty("EnetRemoteHost", enetRemoteHost);
                                _ediabas.SetConfigProperty("EnetIcomAllocate", "0");
                                logFormat(ApiLogLevel.Normal, "redirecting RPLUS:ICOM_P to ENET: {0}", enetRemoteHost);
                            }
                        }
                    }

                    if (string.IsNullOrEmpty(ifh))
                    {
                        logFormat(ApiLogLevel.Normal, "RPLUS arguments invalid");
                    }
                }
#endif
            }

            if (string.IsNullOrEmpty(ifh))
            {
                ifh = _ediabas.GetConfigProperty("Interface");
            }

            EdInterfaceBase edInterface;
            if (!string.IsNullOrEmpty(ifh))
            {
                if (EdInterfaceObd.IsValidInterfaceNameStatic(ifh))
                {
                    edInterface = new EdInterfaceObd();
                }
                else if (EdInterfaceEdic.IsValidInterfaceNameStatic(ifh))
                {
                    edInterface = new EdInterfaceEdic();
                }
#if !Android
                else if (EdInterfaceAds.IsValidInterfaceNameStatic(ifh))
                {
                    edInterface = new EdInterfaceAds();
                }
#endif
                else if (EdInterfaceEnet.IsValidInterfaceNameStatic(ifh))
                {
                    edInterface = new EdInterfaceEnet();
                }
                else if (EdInterfaceRplus.IsValidInterfaceNameStatic(ifh))
                {
                    edInterface = new EdInterfaceRplus();
                }
                else
                {
                    setLocalError((int)EdiabasNet.ErrorCodes.EDIABAS_IFH_0027);
                    _ediabas.Dispose();
                    logFormat(ApiLogLevel.Normal, "Ifh invalid: {0}", ifh);
                    logFormat(ApiLogLevel.Normal, "={0} ()", false);
                    return false;
                }
            }
            else
            {
                edInterface = new EdInterfaceObd();
            }

            if (!edInterface.InterfaceLock(2000))
            {
                setLocalError((int)EdiabasNet.ErrorCodes.EDIABAS_API_0006);
                edInterface.Dispose();
                _ediabas.Dispose();
                logFormat(ApiLogLevel.Normal, "Interface lock failed");
                logFormat(ApiLogLevel.Normal, "={0} ()", false);
                return false;
            }

            edInterface.IfhName = ifh;
            edInterface.UnitName = unit;
            edInterface.ApplicationName = app;

            _ediabas.EdInterfaceClass = edInterface;
            _ediabas.AbortJobFunc = abortJobFunc;
            _lastIfh = ifh;
            _lastUnit = unit;
            _lastApp = app;
            _lastConfig = config;

            logFormat(ApiLogLevel.Normal, "={0} ()", true);
            return true;
        }

        public void apiEnd()
        {
            logFormat(ApiLogLevel.Normal, "apiEnd()");

            if (_ediabas != null)
            {
                try
                {
                    _abortJob = true;
                    while (_jobThread != null)
                    {
                        Thread.Sleep(10);
                    }
                    closeLog();
                    _ediabas.Dispose();
                }
                catch (Exception)
                {
                    // ignored
                }

                _ediabas = null;
            }
        }

        public bool apiSwitchDevice(string unit, string app)
        {
            logFormat(ApiLogLevel.Normal, "apiSwitchDevice({0}, {1})", unit, app);

            setLocalError(EDIABAS_ERR_NONE);
            if (!string.IsNullOrEmpty(unit))
            {
                if (char.IsLetter(unit[0]))
                {
                    setLocalError((int)EdiabasNet.ErrorCodes.EDIABAS_IFH_0018);
                    return false;
                }
            }
            return true;
        }

        public void apiJob(string ecu, string job, string para, string result)
        {
            logFormat(ApiLogLevel.Normal, "apiJob({0}, {1}, {2}, {3})", ecu, job, para, result);

            byte[] paraBytes = (para == null) ? new byte[0] : Encoding.GetBytes(para);
            executeJob(ecu, job, null, 0, paraBytes, paraBytes.Length, result);
        }

        public void apiJobData(string ecu, string job, byte[] para, int paralen, string result)
        {
            logFormat(ApiLogLevel.Normal, "apiJobData({0}, {1}, {2}, {3}, {4})", ecu, job, para, paralen, result);

            executeJob(ecu, job, null, 0, para, paralen, result);
        }

        public void apiJobExt(string ecu, string job, byte[] stdpara, int stdparalen, byte[] para, int paralen, string result, int reserved)
        {
            logFormat(ApiLogLevel.Normal, "apiJobExt({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7})", ecu, job, stdpara, stdparalen, para, paralen, result, reserved);

            executeJob(ecu, job, stdpara, stdparalen, para, paralen, result);
        }

        public int apiJobInfo(out string infoText)
        {
            logFormat(ApiLogLevel.Normal, "apiJobInfo()");

            int progressPercent = 0;
            if (_ediabas == null)
            {
                setLocalError((int)EdiabasNet.ErrorCodes.EDIABAS_API_0006);
                infoText = string.Empty;
                logFormat(ApiLogLevel.Normal, "={0} ({1})", progressPercent, infoText);
                return progressPercent;
            }
            infoText = _ediabas.InfoProgressText;

            progressPercent = _ediabas.InfoProgressPercent;

            logFormat(ApiLogLevel.Normal, "={0} ({1})", progressPercent, infoText);
            return progressPercent;
        }

        public bool apiResultChar(out char buffer, string result, ushort rset)
        {
            logFormat(ApiLogLevel.Normal, "apiResultChar({0}, {1})", result, rset);

            buffer = '\0';
            if (!waitJobFinish())
            {
                logFormat(ApiLogLevel.Normal, "={0}", false);
                return false;
            }

            setLocalError(EDIABAS_ERR_NONE);
            Int64 int64Buffer;
            if (!getResultInt64(out int64Buffer, result, rset))
            {
                return false;
            }
            buffer = (char)int64Buffer;

            logFormat(ApiLogLevel.Normal, "={0} ({1})", true, (int) buffer);
            return true;
        }

        public bool apiResultByte(out byte buffer, string result, ushort rset)
        {
            logFormat(ApiLogLevel.Normal, "apiResultByte({0}, {1})", result, rset);

            buffer = 0;
            if (!waitJobFinish())
            {
                logFormat(ApiLogLevel.Normal, "={0}", false);
                return false;
            }

            setLocalError(EDIABAS_ERR_NONE);
            Int64 int64Buffer;
            if (!getResultInt64(out int64Buffer, result, rset))
            {
                return false;
            }
            buffer = (byte)int64Buffer;

            logFormat(ApiLogLevel.Normal, "={0} ({1})", true, buffer);
            return true;
        }

        public bool apiResultInt(out short buffer, string result, ushort rset)
        {
            logFormat(ApiLogLevel.Normal, "apiResultInt({0}, {1})", result, rset);

            buffer = 0;
            if (!waitJobFinish())
            {
                logFormat(ApiLogLevel.Normal, "={0}", false);
                return false;
            }

            setLocalError(EDIABAS_ERR_NONE);
            Int64 int64Buffer;
            if (!getResultInt64(out int64Buffer, result, rset))
            {
                return false;
            }
            buffer = (short)int64Buffer;

            logFormat(ApiLogLevel.Normal, "={0} ({1})", true, buffer);
            return true;
        }

        public bool apiResultWord(out ushort buffer, string result, ushort rset)
        {
            logFormat(ApiLogLevel.Normal, "apiResultWord({0}, {1})", result, rset);

            buffer = 0;
            if (!waitJobFinish())
            {
                logFormat(ApiLogLevel.Normal, "={0}", false);
                return false;
            }

            setLocalError(EDIABAS_ERR_NONE);
            Int64 int64Buffer;
            if (!getResultInt64(out int64Buffer, result, rset))
            {
                return false;
            }
            buffer = (ushort)int64Buffer;

            logFormat(ApiLogLevel.Normal, "={0} ({1})", true, buffer);
            return true;
        }

        public bool apiResultLong(out int buffer, string result, ushort rset)
        {
            logFormat(ApiLogLevel.Normal, "apiResultLong({0}, {1})", result, rset);

            buffer = 0;
            if (!waitJobFinish())
            {
                logFormat(ApiLogLevel.Normal, "={0}", false);
                return false;
            }

            setLocalError(EDIABAS_ERR_NONE);
            Int64 int64Buffer;
            if (!getResultInt64(out int64Buffer, result, rset))
            {
                return false;
            }
            buffer = (int)int64Buffer;

            logFormat(ApiLogLevel.Normal, "={0} ({1})", true, buffer);
            return true;
        }

        public bool apiResultDWord(out uint buffer, string result, ushort rset)
        {
            logFormat(ApiLogLevel.Normal, "apiResultDWord({0}, {1})", result, rset);

            buffer = 0;
            if (!waitJobFinish())
            {
                logFormat(ApiLogLevel.Normal, "={0}", false);
                return false;
            }

            setLocalError(EDIABAS_ERR_NONE);
            Int64 int64Buffer;
            if (!getResultInt64(out int64Buffer, result, rset))
            {
                return false;
            }
            buffer = (uint)int64Buffer;

            logFormat(ApiLogLevel.Normal, "={0} ({1})", true, buffer);
            return true;
        }

        public bool apiResultLongLong(out long buffer, string result, ushort rset)
        {
            logFormat(ApiLogLevel.Normal, "apiResultLongLong({0}, {1})", result, rset);

            buffer = 0;
            if (!waitJobFinish())
            {
                logFormat(ApiLogLevel.Normal, "={0}", false);
                return false;
            }

            setLocalError(EDIABAS_ERR_NONE);
            if (!getResultInt64(out Int64 int64Buffer, out EdiabasNet.ResultType resultType, result, rset))
            {
                return false;
            }

            switch (resultType)
            {
                case EdiabasNet.ResultType.TypeQ:
                case EdiabasNet.ResultType.TypeLL:
                    buffer = int64Buffer;
                    break;

                case EdiabasNet.ResultType.TypeD:
                    buffer = (uint)int64Buffer;
                    break;

                default:
                    buffer = (int)int64Buffer;
                    break;
            }

            logFormat(ApiLogLevel.Normal, "={0} ({1})", true, buffer);
            return true;
        }

        public bool apiResultQWord(out ulong buffer, string result, ushort rset)
        {
            logFormat(ApiLogLevel.Normal, "apiResultQWord({0}, {1})", result, rset);

            buffer = 0;
            if (!waitJobFinish())
            {
                logFormat(ApiLogLevel.Normal, "={0}", false);
                return false;
            }

            setLocalError(EDIABAS_ERR_NONE);
            if (!getResultInt64(out Int64 int64Buffer, out EdiabasNet.ResultType resultType, result, rset))
            {
                return false;
            }

            switch (resultType)
            {
                case EdiabasNet.ResultType.TypeQ:
                case EdiabasNet.ResultType.TypeLL:
                    buffer = (ulong)int64Buffer;
                    break;

                default:
                    buffer = (uint)int64Buffer;
                    break;
            }

            logFormat(ApiLogLevel.Normal, "={0} ({1})", true, buffer);
            return true;
        }

        public bool apiResultReal(out double buffer, string result, ushort rset)
        {
            logFormat(ApiLogLevel.Normal, "apiResultReal({0}, {1})", result, rset);

            buffer = 0;
            if (!waitJobFinish())
            {
                logFormat(ApiLogLevel.Normal, "={0}", false);
                return false;
            }

            setLocalError(EDIABAS_ERR_NONE);
            EdiabasNet.ResultData resultData = getResultData(result, rset);
            if (resultData == null)
            {
                logFormat(ApiLogLevel.Normal, "={0}", false);
                return false;
            }
            if ((resultData.OpData is long))
            {
                Int64 value = (Int64)resultData.OpData;
                buffer = value;
            }
            else if ((resultData.OpData is double))
            {
                buffer = (Double)resultData.OpData;
            }
            else if ((resultData.OpData is string))
            {
                buffer = EdiabasNet.StringToFloat((string) resultData.OpData);
            }
            else
            {
                setLocalError((int) EdiabasNet.ErrorCodes.EDIABAS_API_0005);
                logFormat(ApiLogLevel.Normal, "={0}", false);
                return false;
            }

            logFormat(ApiLogLevel.Normal, "={0} ({1})", true, buffer);
            return true;
        }

        public bool apiResultText(out string buffer, string result, ushort rset, string format)
        {
            logFormat(ApiLogLevel.Normal, "apiResultText({0}, {1}, {2})", result, rset, format);

            buffer = string.Empty;
            if (!waitJobFinish())
            {
                logFormat(ApiLogLevel.Normal, "={0}", false);
                return false;
            }

            setLocalError(EDIABAS_ERR_NONE);
            EdiabasNet.ResultData resultData = getResultData(result, rset);
            if (resultData == null)
            {
                logFormat(ApiLogLevel.Normal, "={0}", false);
                return false;
            }

            string value = EdiabasNet.FormatResult(resultData, format);
            if (value == null)
            {
                setLocalError((int)EdiabasNet.ErrorCodes.EDIABAS_API_0005);
                logFormat(ApiLogLevel.Normal, "={0}", false);
                return false;
            }
            buffer = value;

            logFormat(ApiLogLevel.Normal, "={0} ({1})", true, buffer);
            return true;
        }

        public bool apiResultText(out char[] buffer, string result, ushort rset, string format)
        {
            logFormat(ApiLogLevel.Normal, "apiResultText[]({0}, {1}, {2})", result, rset, format);

            buffer = null;
            if (!waitJobFinish())
            {
                logFormat(ApiLogLevel.Normal, "={0}", false);
                return false;
            }

            setLocalError(EDIABAS_ERR_NONE);
            string text;
            if (!apiResultText(out text, result, rset, format))
            {
                logFormat(ApiLogLevel.Normal, "={0}", false);
                return false;
            }
            char[] charArray = text.ToCharArray();
            Array.Resize(ref charArray, charArray.Length + 1);
            charArray[charArray.Length - 1] = '\0';
            buffer = charArray;

            logFormat(ApiLogLevel.Normal, "={0} ({1})", true, buffer);
            return true;
        }

        public bool apiResultBinary(out byte[] buffer, out ushort bufferLen, string result, ushort rset)
        {
            logFormat(ApiLogLevel.Normal, "apiResultBinary({0}, {1})", result, rset);

            buffer = null;
            bufferLen = 0;
            if (!waitJobFinish())
            {
                logFormat(ApiLogLevel.Normal, "={0}", false);
                return false;
            }

            setLocalError(EDIABAS_ERR_NONE);
            EdiabasNet.ResultData resultData = getResultData(result, rset);
            if (resultData == null)
            {
                logFormat(ApiLogLevel.Normal, "={0}", false);
                return false;
            }
            if ((resultData.ResType != EdiabasNet.ResultType.TypeY) || (resultData.OpData.GetType() != typeof(byte[])))
            {
                setLocalError((int)EdiabasNet.ErrorCodes.EDIABAS_API_0005);
                logFormat(ApiLogLevel.Normal, "={0}", false);
                return false;
            }
            byte[] value = (byte[])resultData.OpData;
            buffer = new byte[APIMAXBINARY];
            int dataLength = value.Length;
            if (value.Length > buffer.Length)
            {
                dataLength = buffer.Length;
            }
            Array.Copy(value, buffer, dataLength);
            bufferLen = (ushort)dataLength;

            logFormat(ApiLogLevel.Normal, "={0} ({1})", true, value);
            return true;
        }

        public bool apiResultBinaryExt(out byte[] buffer, out uint bufferLen, uint bufferSize, string result, ushort rset)
        {
            logFormat(ApiLogLevel.Normal, "apiResultBinaryExt({0}, {1})", result, rset);

            buffer = null;
            bufferLen = 0;
            if (!waitJobFinish())
            {
                logFormat(ApiLogLevel.Normal, "={0}", false);
                return false;
            }

            setLocalError(EDIABAS_ERR_NONE);
            EdiabasNet.ResultData resultData = getResultData(result, rset);
            if (resultData == null)
            {
                logFormat(ApiLogLevel.Normal, "={0}", false);
                return false;
            }
            if ((resultData.ResType != EdiabasNet.ResultType.TypeY) || (resultData.OpData.GetType() != typeof(byte[])))
            {
                setLocalError((int)EdiabasNet.ErrorCodes.EDIABAS_API_0005);
                logFormat(ApiLogLevel.Normal, "={0}", false);
                return false;
            }
            byte[] value = (byte[])resultData.OpData;
            buffer = new byte[APIMAXBINARYEXT];
            int dataLength = value.Length;
            if (value.Length > buffer.Length)
            {
                dataLength = buffer.Length;
            }
            Array.Copy(value, buffer, dataLength);
            bufferLen = (ushort)dataLength;

            logFormat(ApiLogLevel.Normal, "={0} ({1})", true, value);
            return true;
        }

        public bool apiResultFormat(out int buffer, string result, ushort rset)
        {
            logFormat(ApiLogLevel.Normal, "apiResultFormat({0}, {1})", result, rset);

            buffer = 0;
            if (!waitJobFinish())
            {
                logFormat(ApiLogLevel.Normal, "={0}", false);
                return false;
            }

            setLocalError(EDIABAS_ERR_NONE);
            EdiabasNet.ResultData resultData = getResultData(result, rset);
            if (resultData == null)
            {
                logFormat(ApiLogLevel.Normal, "={0}", false);
                return false;
            }

            switch (resultData.ResType)
            {
                case EdiabasNet.ResultType.TypeB:
                    buffer = APIFORMAT_BYTE;
                    break;

                case EdiabasNet.ResultType.TypeW:
                    buffer = APIFORMAT_WORD;
                    break;

                case EdiabasNet.ResultType.TypeD:
                    buffer = APIFORMAT_DWORD;
                    break;

                case EdiabasNet.ResultType.TypeQ:
                    buffer = APIFORMAT_QWORD;
                    break;

                case EdiabasNet.ResultType.TypeC:
                    buffer = APIFORMAT_CHAR;
                    break;

                case EdiabasNet.ResultType.TypeI:
                    buffer = APIFORMAT_INTEGER;
                    break;

                case EdiabasNet.ResultType.TypeL:
                    buffer = APIFORMAT_LONG;
                    break;

                case EdiabasNet.ResultType.TypeLL:
                    buffer = APIFORMAT_LONGLONG;
                    break;

                case EdiabasNet.ResultType.TypeR:
                    buffer = APIFORMAT_REAL;
                    break;

                case EdiabasNet.ResultType.TypeS:
                    buffer = APIFORMAT_TEXT;
                    break;

                case EdiabasNet.ResultType.TypeY:
                    buffer = APIFORMAT_BINARY;
                    break;

                default:
                    setLocalError((int)EdiabasNet.ErrorCodes.EDIABAS_API_0005);
                    break;
            }

            logFormat(ApiLogLevel.Normal, "={0} ({1})", true, buffer);
            return true;
        }

        public bool apiResultNumber(out ushort buffer, ushort rset)
        {
            logFormat(ApiLogLevel.Normal, "apiResultNumber({0})", rset);

            buffer = 0;
            if (!waitJobFinish())
            {
                logFormat(ApiLogLevel.Normal, "={0}", false);
                return false;
            }

            setLocalError(EDIABAS_ERR_NONE);
            if (_resultSets == null)
            {
                setLocalError((int)EdiabasNet.ErrorCodes.EDIABAS_API_0014);
                logFormat(ApiLogLevel.Normal, "={0}", false);
                return false;
            }
            if (rset >= _resultSets.Count)
            {
                setLocalError((int)EdiabasNet.ErrorCodes.EDIABAS_API_0014);
                logFormat(ApiLogLevel.Normal, "={0}", false);
                return false;
            }
            buffer = (ushort)_resultSets[rset].Count;

            logFormat(ApiLogLevel.Normal, "={0} ({1})", true, buffer);
            return true;
        }

        public bool apiResultName(out string buffer, ushort index, ushort rset)
        {
            logFormat(ApiLogLevel.Normal, "apiResultName({0}, {1})", index, rset);

            buffer = string.Empty;
            if (!waitJobFinish())
            {
                logFormat(ApiLogLevel.Normal, "={0}", false);
                return false;
            }

            setLocalError(EDIABAS_ERR_NONE);
            if (_resultSets == null)
            {
                setLocalError((int)EdiabasNet.ErrorCodes.EDIABAS_API_0014);
                logFormat(ApiLogLevel.Normal, "={0}", false);
                return false;
            }
            if (rset >= _resultSets.Count)
            {
                setLocalError((int)EdiabasNet.ErrorCodes.EDIABAS_API_0014);
                logFormat(ApiLogLevel.Normal, "={0}", false);
                return false;
            }

            Dictionary<string, EdiabasNet.ResultData> resultDict = _resultSets[rset];
            if ((index < 1) || (index > resultDict.Keys.Count))
            {
                setLocalError((int)EdiabasNet.ErrorCodes.EDIABAS_API_0014);
                logFormat(ApiLogLevel.Normal, "={0}", false);
                return false;
            }

            buffer = resultDict.Values.ElementAt(index - 1).Name;
            logFormat(ApiLogLevel.Normal, "={0} ({1})", true, buffer);
            return true;
        }

        public bool apiResultSets(out ushort rsets)
        {
            logFormat(ApiLogLevel.Normal, "apiResultSets()");

            rsets = 0;
            if (!waitJobFinish())
            {
                logFormat(ApiLogLevel.Normal, "={0}", false);
                return false;
            }

            setLocalError(EDIABAS_ERR_NONE);
            if (_ediabas == null)
            {
                setLocalError((int)EdiabasNet.ErrorCodes.EDIABAS_API_0006);
                logFormat(ApiLogLevel.Normal, "={0}", false);
                return false;
            }
            if (_resultSets == null)
            {
                setLocalError((int)EdiabasNet.ErrorCodes.EDIABAS_API_0014);
                logFormat(ApiLogLevel.Normal, "={0}", false);
                return false;
            }
            if (_resultSets.Count < 1)
            {
                setLocalError((int)EdiabasNet.ErrorCodes.EDIABAS_API_0014);
                logFormat(ApiLogLevel.Normal, "={0}", false);
                return false;
            }
            rsets = (ushort)(_resultSets.Count - 1);

            logFormat(ApiLogLevel.Normal, "={0} ({1})", true, rsets);
            return true;
        }

        public bool apiResultVar(out string var)
        {
            logFormat(ApiLogLevel.Normal, "apiResultSets()");

            bool result = apiResultText(out var, "VARIANTE", 0, string.Empty);

            logFormat(ApiLogLevel.Normal, "={0} ({1})", result, var);
            return result;
        }

        public APIRESULTFIELD apiResultsNew()
        {
            logFormat(ApiLogLevel.Normal, "apiResultsNew()");

            waitJobFinish();
            APIRESULTFIELD resultField = new APIRESULTFIELD(_resultSets);

            return resultField;
        }

        public void apiResultsScope(APIRESULTFIELD resultField)
        {
            logFormat(ApiLogLevel.Normal, "apiResultsScope()");

            waitJobFinish();
            _resultSets = resultField.ResultSets;
        }

        public void apiResultsDelete(APIRESULTFIELD resultField)
        {
            logFormat(ApiLogLevel.Normal, "apiResultsDelete()");

            resultField.ResultSets = null;
        }

        public int apiState()
        {
            if (_apiStateValue != APIBUSY)
            {
                logFormat(ApiLogLevel.Normal, "apiState()={0} busy={1}", _apiStateValue, _busyCount);
                _busyCount = 0;
            }
            else
            {
                _busyCount++;
            }
            return _apiStateValue;
        }

        public int apiStateExt(int suspendTime)
        {
            int state = _apiStateValue;
            if (state == APIBUSY)
            {
                long startTime = Stopwatch.GetTimestamp();
                while ((Stopwatch.GetTimestamp() - startTime) < suspendTime * TickResolMs)
                {
                    state = _apiStateValue;
                    if (state != APIBUSY)
                    {
                        break;
                    }
                    Thread.Sleep(10);
                }
            }

            if (state != APIBUSY)
            {
                logFormat(ApiLogLevel.Normal, "apiStateExt({0})={1} busy={2}", suspendTime, state, _busyCount);
                _busyCount = 0;
            }
            else
            {
                _busyCount++;
            }
            return state;
        }

        public void apiBreak()
        {
            logFormat(ApiLogLevel.Normal, "apiBreak()");

            if (_jobThread == null)
            {
                return;
            }
            _abortJob = true;
        }

        public int apiErrorCode()
        {
            //logFormat(ApiLogLevel.Normal, "apiErrorCode()");

            if (_localError != EDIABAS_ERR_NONE)
            {
                logFormat(ApiLogLevel.Normal, "apiErrorCode()");
                logFormat(ApiLogLevel.Normal, "={0} ()", (EdiabasNet.ErrorCodes)_localError);
                return _localError;
            }
            if (_ediabas == null)
            {
                logFormat(ApiLogLevel.Normal, "apiErrorCode()");
                logFormat(ApiLogLevel.Normal, "={0} ()", (int)EdiabasNet.ErrorCodes.EDIABAS_API_0006);
                return (int)EdiabasNet.ErrorCodes.EDIABAS_API_0006;
            }

            if (_ediabas.ErrorCodeLast != EDIABAS_ERR_NONE)
            {
                logFormat(ApiLogLevel.Normal, "apiErrorCode()");
                logFormat(ApiLogLevel.Normal, "={0} ()", _ediabas.ErrorCodeLast);
            }
            return (int)_ediabas.ErrorCodeLast;
        }

        public string apiErrorText()
        {
            logFormat(ApiLogLevel.Normal, "apiErrorText()");

            string errorText = EdiabasNet.GetErrorDescription((EdiabasNet.ErrorCodes)apiErrorCode());

            logFormat(ApiLogLevel.Normal, "={0} ()", errorText);
            return errorText;
        }

        public bool apiSetConfig(string cfgName, string cfgValue)
        {
            logFormat(ApiLogLevel.Normal, "apiSetConfig({0}, {1})", cfgName, cfgValue);

            if (_ediabas == null)
            {
                setLocalError((int)EdiabasNet.ErrorCodes.EDIABAS_API_0006);
                logFormat(ApiLogLevel.Normal, "={0} ()", false);
                return false;
            }
            bool setProperty = true;
#if false   // for debugging only!
            if (string.Compare(cfgName, "ApiTrace", StringComparison.OrdinalIgnoreCase) == 0)
            {
                setProperty = false;
            }
#endif
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (setProperty)
            {
                _ediabas.SetConfigProperty(cfgName, cfgValue);
            }
            if (string.Compare(cfgName, "TracePath", StringComparison.OrdinalIgnoreCase) == 0)
            {
                closeLog();
            }
            if (string.Compare(cfgName, "ApiTrace", StringComparison.OrdinalIgnoreCase) == 0)
            {
                closeLog();
            }
            if (string.Compare(cfgName, "TraceBuffering", StringComparison.OrdinalIgnoreCase) == 0)
            {
                closeLog();
            }
            if (string.Compare(cfgName, "ApiTraceName", StringComparison.OrdinalIgnoreCase) == 0)
            {
                closeLog();
            }

            logFormat(ApiLogLevel.Normal, "={0} ()", true);
            return true;
        }

        public bool apiGetConfig(string cfgName, out string cfgValue)
        {
            logFormat(ApiLogLevel.Normal, "apiGetConfig({0})", cfgName);

            cfgValue = string.Empty;
            if (_ediabas == null)
            {
                setLocalError((int)EdiabasNet.ErrorCodes.EDIABAS_API_0006);
                logFormat(ApiLogLevel.Normal, "={0} ()", false);
                return false;
            }
            string prop = _ediabas.GetConfigProperty(cfgName);
            if (prop != null)
            {
                cfgValue = prop;
            }

            logFormat(ApiLogLevel.Normal, "={0} ({1})", true, cfgValue);
            return true;
        }

        public void apiTrace(string msg)
        {
            logFormat(ApiLogLevel.Normal, "apiTrace({0})", msg);
        }

        public static bool apiXSysSetConfig(string cfgName, string cfgValue)
        {
            //logFormat(API_LOG_LEVEL.NORMAL, "apiXSysSetConfig({0}, {1})", cfgName, cfgValue);
            return true;
        }

        public static void closeServer()
        {
            //logFormat(API_LOG_LEVEL.NORMAL, "closeServer()");
        }

        public static bool enableServer(bool onOff)
        {
            //logFormat(API_LOG_LEVEL.NORMAL, "enableServer({0})", onOff);
            return true;
        }

        public static bool enableMultiThreading(bool onOff)
        {
            //logFormat(API_LOG_LEVEL.NORMAL, "enableMultiThreading({0})", onOff);
            return true;
        }

        private void setLocalError(int error)
        {
            if (error == EDIABAS_ERR_NONE)
            {
                switch (_localError)
                {
                    case (int)EdiabasNet.ErrorCodes.EDIABAS_API_0005:
                    case (int)EdiabasNet.ErrorCodes.EDIABAS_API_0014:
                        _localError = error;
                        break;
                }
            }
            _localError = error;
        }

        private void setJobError(int error)
        {
            if (error == EDIABAS_ERR_NONE)
            {
                _localError = error;
                _apiStateValue = APIREADY;
                return;
            }

            _localError = error;
            _apiStateValue = APIERROR;
        }

        private bool waitJobFinish()
        {
            while (_apiStateValue == APIBUSY)
            {
                Thread.Sleep(10);
            }
            if (_apiStateValue != APIREADY)
            {
                return false;
            }
            return true;
        }

        private EdiabasNet.ResultData getResultData(string result, ushort rset)
        {
            if (_resultSets == null)
            {
                setLocalError((int)EdiabasNet.ErrorCodes.EDIABAS_API_0014);
                return null;
            }
            if (rset >= _resultSets.Count)
            {
                setLocalError((int)EdiabasNet.ErrorCodes.EDIABAS_API_0014);
                return null;
            }

            Dictionary<string, EdiabasNet.ResultData> resultDict = _resultSets[rset];
            EdiabasNet.ResultData resultData;
            if (!resultDict.TryGetValue(result.ToUpper(Culture), out resultData))
            {
                setLocalError((int)EdiabasNet.ErrorCodes.EDIABAS_API_0014);
                return null;
            }
            return resultData;
        }

        private bool getResultInt64(out Int64 buffer, string result, ushort rset)
        {
            return getResultInt64(out buffer, out _, result, rset);
        }

        private bool getResultInt64(out Int64 buffer, out EdiabasNet.ResultType resultType, string result, ushort rset)
        {
            buffer = 0;
            resultType = EdiabasNet.ResultType.TypeL;
            EdiabasNet.ResultData resultData = getResultData(result, rset);
            if (resultData == null)
            {
                logFormat(ApiLogLevel.Normal, "={0}", false);
                return false;
            }

            resultType = resultData.ResType;
            if ((resultData.OpData is long))
            {
                buffer = (Int64)resultData.OpData;
            }
            else if ((resultData.OpData is double))
            {
                Double value = (Double)resultData.OpData;
                buffer = (Int64)value;
            }
            else if ((resultData.OpData is string))
            {
                buffer = EdiabasNet.StringToValue((string)resultData.OpData);
            }
            else
            {
                setLocalError((int)EdiabasNet.ErrorCodes.EDIABAS_API_0005);
                logFormat(ApiLogLevel.Normal, "={0}", false);
                return false;
            }
            return true;
        }

        public void executeJob(string ecu, string job, byte[] stdpara, int stdparalen, byte[] para, int paralen, string result)
        {
            if (_ediabas == null)
            {
                setJobError((int)EdiabasNet.ErrorCodes.EDIABAS_API_0006);
                return;
            }
            // wait for last job to finish
            while (_jobThread != null)
            {
                Thread.Sleep(10);
            }

            setJobError(EDIABAS_ERR_NONE);
            _resultSets = null;

            try
            {
                if (para != null && para.Length != paralen)
                {
                    byte[] binData = new byte[paralen];
                    int copyLen = paralen;
                    if (copyLen > para.Length)
                    {
                        copyLen = para.Length;
                    }
                    Array.Copy(para, binData, copyLen);
                    _ediabas.ArgBinary = binData;
                }
                else
                {
                    _ediabas.ArgBinary = para;
                }

                if (stdpara != null && stdpara.Length != stdparalen)
                {
                    byte[] binData = new byte[stdparalen];
                    int copyLen = stdparalen;
                    if (copyLen > stdpara.Length)
                    {
                        copyLen = stdpara.Length;
                    }
                    Array.Copy(stdpara, binData, copyLen);
                    _ediabas.ArgBinaryStd = binData;
                }
                else
                {
                    _ediabas.ArgBinaryStd = stdpara;
                }

                _ediabas.ResultsRequests = result;
            }
            catch (Exception)
            {
                setJobError((int)EdiabasNet.ErrorCodes.EDIABAS_SYS_0000);
                return;
            }
            _jobName = job;
            _jobEcuName = ecu;
            _abortJob = false;
            _apiStateValue = APIBUSY;
            _jobThread = new Thread(jobThreadFunc);
            _jobThread.Start();
        }

        private void jobThreadFunc()
        {
            try
            {
                try
                {
                    _ediabas.ResolveSgbdFile(_jobEcuName);
                }
                catch (Exception ex)
                {
                    if (_abortJob)
                    {
                        _apiStateValue = APIBREAK;
                    }
                    else
                    {
                        if (_ediabas.ErrorCodeLast != EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE)
                        {
                            _apiStateValue = APIERROR;
                        }
                        else
                        {
                            if (ex is EdiabasNet.EdiabasNetException ediabasNetException)
                            {
                                setJobError((int)ediabasNetException.ErrorCode);
                            }
                            else
                            {
                                setJobError((int)EdiabasNet.ErrorCodes.EDIABAS_SYS_0002);
                            }
                        }
                    }
                    return;
                }

                _ediabas.ExecuteJob(_jobName);
                _resultSets = _ediabas.ResultSets;
                _apiStateValue = APIREADY;
            }
            catch (Exception)
            {
                if (_abortJob)
                {
                    _apiStateValue = APIBREAK;
                }
                else
                {
                    if (_ediabas.ErrorCodeLast != EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE)
                    {
                        _apiStateValue = APIERROR;
                    }
                    else
                    {
                        setJobError((int)EdiabasNet.ErrorCodes.EDIABAS_SYS_0000);
                    }
                }
            }
            finally
            {
                _jobThread = null;
                _abortJob = false;
            }
        }

        private bool abortJobFunc()
        {
            return _abortJob;
        }

        public void logFormat(ApiLogLevel logLevel, string format, params object[] args)
        {
            updateLogLevel();
            if ((int)logLevel > _logLevelApi)
            {
                return;
            }

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == null)
                {
                    continue;
                }
                if (args[i] is string)
                {
                    args[i] = "'" + (string)args[i] + "'";
                }
                if (args[i].GetType() == typeof(byte[]))
                {
                    byte[] argArray = (byte[])args[i];
                    StringBuilder stringBuilder = new StringBuilder(argArray.Length);
                    foreach (byte arg in argArray)
                    {
                        stringBuilder.Append(string.Format(Culture, "{0:X02} ", arg));
                    }

                    args[i] = "[" + stringBuilder +"]";
                }
            }
            logString(logLevel, string.Format(Culture, format, args));
        }

        public void logString(ApiLogLevel logLevel, string info)
        {
            updateLogLevel();
            if ((int)logLevel > _logLevelApi)
            {
                return;
            }

            try
            {
                lock (_apiLogLock)
                {
                    if (_swLog == null && _ediabas != null)
                    {
                        string tracePath = _ediabas.GetConfigProperty("TracePath");
                        if (tracePath != null)
                        {
                            string traceBuffering = _ediabas.GetConfigProperty("TraceBuffering");
                            Int64 buffering = 0;
                            if (traceBuffering != null)
                            {
                                buffering = EdiabasNet.StringToValue(traceBuffering);
                            }

                            int appendTrace = 0;
                            string propAppend = _ediabas.GetConfigProperty("AppendTrace");
                            if (propAppend != null)
                            {
                                appendTrace = (int)EdiabasNet.StringToValue(propAppend);
                            }

                            string traceFileName = "api.trc";
                            string propName = _ediabas.GetConfigProperty("ApiTraceName");
                            if (!string.IsNullOrWhiteSpace(propName))
                            {
                                traceFileName = propName;
                            }

                            Directory.CreateDirectory(tracePath);
                            for (int fileIdx = 0; fileIdx < 10; fileIdx++)
                            {
                                bool appendFile = appendTrace != 0;
                                string suffix = (fileIdx > 0) ? "_" + fileIdx : string.Empty;
                                string idxFileName = Path.GetFileNameWithoutExtension(traceFileName) + suffix + Path.GetExtension(traceFileName);
                                string traceFile = Path.Combine(tracePath, idxFileName);
                                try
                                {
                                    if (appendFile && File.Exists(traceFile))
                                    {
                                        DateTime lastWriteTime = File.GetLastWriteTime(traceFile);
                                        TimeSpan diffTime = DateTime.Now - lastWriteTime;
                                        if (diffTime.Hours > EdiabasNet.TraceAppendDiffHours)
                                        {
                                            appendFile = false;
                                        }
                                    }
                                }
                                catch (Exception)
                                {
                                    // ignored
                                }

                                FileMode fileMode = FileMode.Append;
                                if (_firstLog && !appendFile)
                                {
                                    fileMode = FileMode.Create;
                                }

                                try
                                {
                                    _swLog = new StreamWriter(new FileStream(traceFile, fileMode, FileAccess.Write, FileShare.ReadWrite), Encoding)
                                    {
                                        AutoFlush = buffering == 0
                                    };
                                    break;
                                }
                                catch (Exception)
                                {
                                    // ignored
                                }
                            }
                        }
                    }

                    if (_swLog != null)
                    {
                        _firstLog = false;
                        _swLog.WriteLine(info);
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private void closeLog()
        {
            lock (_apiLogLock)
            {
                if (_swLog != null)
                {
                    _swLog.Dispose();
                    _swLog = null;
                }
                _logLevelApi = -1;
            }
        }

        private void updateLogLevel()
        {
            if (_logLevelApi < 0)
            {
                lock (_apiLogLock)
                {
                    if (_ediabas != null)
                    {
                        string apiTrace = _ediabas.GetConfigProperty("ApiTrace");
                        _logLevelApi = Convert.ToInt32(apiTrace);
                    }
                }
            }
        }
    }
}
