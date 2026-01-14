using log4net;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading;

#pragma warning disable SYSLIB0005
namespace PsdzClient.Core
{
    public class Log
    {
        private enum TraceLevel
        {
            INFO,
            DEBUG,
            WARNING,
            ERROR,
            FATAL
        }

        private static readonly CultureInfo LogCulture = CultureInfo.CreateSpecificCulture("de-DE");
        public static bool LogCallerPid { get; set; }

        public static void Error(string method, string msg, params object[] args)
        {
            Error(method, msg, EventKind.T, args);
        }

        public static void Error(string method, string msg, EventKind evtKind, params object[] args)
        {
            try
            {
                WriteTraceEntry(method, msg, TraceLevel.ERROR, evtKind, args);
                Flush();
            }
            catch (Exception ex)
            {
                TraceTraceError("{0} Log.Error() - failed in method \"{2}\" while writing message \"{3}\" with exception: {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", LogCulture), ex.ToString(), method, msg);
            }
        }

        private static void TraceTraceError(string format, params object[] args)
        {
            Trace.TraceError(format, args);
            //[+] log?.ErrorFormat(format, args);
            log?.ErrorFormat(format, args);
            Flush();
        }

        public static void ErrorException(string method, Exception exception)
        {
            Error(method, "failed with exception: {0}", exception);
        }

        public static void ErrorException(string method, string msg, Exception exception)
        {
            Error(method, "failed with exception: {0} - {1}", msg, exception);
        }

        public static void Flush()
        {
            Trace.Flush();
        }

        public static void Info(string method, string msg, params object[] args)
        {
            Info(method, msg, EventKind.T, args);
        }

        public static void Info(string method, string msg, EventKind evtKind, params object[] args)
        {
            try
            {
                WriteTraceEntry(method, msg, TraceLevel.INFO, evtKind, args);
            }
            catch (Exception ex)
            {
                TraceTraceError("{0} Log.Info() - failed in method \"{2}\" while writing message \"{3}\" with exception: {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", LogCulture), ex.ToString(), method, msg);
            }
        }

        public static void LoadedAssemblies()
        {
            try
            {
                Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
                foreach (Assembly assembly in assemblies)
                {
                    Info("Log.LoadedAssemblies()", "found loaded assembly: {0} {1} {2} GAC: {3}", assembly.FullName, assembly.ImageRuntimeVersion, assembly.IsDynamic ? "(internal)" : assembly.Location, assembly.GlobalAssemblyCache);
                }
            }
            catch (Exception exception)
            {
                WarningException("Log.LoadedAssemblies()", exception);
            }
        }

        public static void ThreadStarted(string method, Thread thread)
        {
            if (thread == null)
            {
                Info(method, "Thread started [{0}, {1}].", "null", "null");
            }
            else
            {
                Info(method, "Thread started [{0}, {1}].", thread.ManagedThreadId, thread.Name);
            }
        }

        public static void ThreadStopped(string method, Thread thread)
        {
            if (thread == null)
            {
                Info(method, "Thread stopped [{0}, {1}].", "null", "null");
            }
            else
            {
                Info(method, "Thread stopped [{0}, {1}], is alive: {2}.", thread.ManagedThreadId, thread.Name, thread.IsAlive);
            }
        }

        public static void ThreadRemoved(string method, Thread thread)
        {
            if (thread == null)
            {
                Info(method, "Thread removed [{0}, {1}].", "null", "null");
            }
            else
            {
                Info(method, "Thread removed [{0}, {1}], is alive: {2}.", thread.ManagedThreadId, thread.Name, thread.IsAlive);
            }
        }

        public static void Threads()
        {
            Thread currentThread = Thread.CurrentThread;
            Info("Log.Threads()", "Thread with ID=\"{0}\" and Name=\"{1}\"", currentThread.ManagedThreadId, currentThread.Name);
        }

        public static void LocalIP()
        {
            IEnumerable<IPAddress> values = Dns.GetHostEntry(Dns.GetHostName()).AddressList.Where((IPAddress a) => a.AddressFamily == AddressFamily.InterNetwork);
            Info("Log.LocalIP()", "local ip addresses: {0}", string.Join("; ", values));
        }

        public static void Warning(string method, string msg, params object[] args)
        {
            Warning(method, msg, EventKind.T, args);
        }

        public static void Warning(string method, string msg, EventKind evtKind, params object[] args)
        {
            try
            {
                WriteTraceEntry(method, msg, TraceLevel.WARNING, evtKind, args);
            }
            catch (Exception ex)
            {
                TraceTraceError("{0} Log.Warning() - failed in method \"{2}\" while writing message \"{3}\" with exception: {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", LogCulture), ex.ToString(), method, msg);
            }
        }

        public static void Fatal(string method, string msg, params object[] args)
        {
            try
            {
                string text = BuildEntry(TraceLevel.FATAL, EventKind.T, method, msg);
                //[+] log?.Error((args == null) ? text : string.Format(text, args));
                log?.Error((args == null) ? text : string.Format(text, args));
                Trace.Fail((args == null) ? text : string.Format(text, args));
            }
            catch (Exception ex)
            {
                TraceTraceError("{0} Log.Fatal() - failed in method \"{2}\" while writing message \"{3}\" with exception: {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", LogCulture), ex.ToString(), method, msg);
            }
        }

        [PreserveSource(Hint = "Using DebugLevel, logThreshhold=6", SignatureModified = true)]
        public static void Debug(string method, string msg, params object[] args)
        {
            //[-] Debug(CoreFramework.DebugLevel, 1, method, msg, args);
            //[+] Debug(CoreFramework.DebugLevel, 6, method, msg, args);
            Debug(CoreFramework.DebugLevel, 6, method, msg, args);
        }

        [PreserveSource(Hint = "Unchanged", SignatureModified = true)]
        public static void Debug(int logState, string method, string msg, params object[] args)
        {
            Debug(logState, 1, method, msg, args);
        }

        [PreserveSource(Hint = "Unchanged", SignatureModified = true)]
        public static void Debug(int logState, int logThreshhold, string method, string msg, params object[] args)
        {
            Debug(logState, logThreshhold, method, msg, EventKind.T, args);
        }

        [PreserveSource(Hint = "Fixed logging", SignatureModified = true)]
        public static void Debug(int logState, int logThreshhold, string method, string msg, EventKind evtKind, params object[] args)
        {
            if (logState < logThreshhold)
            {
                return;
            }

            try
            {
                //[+] WriteTraceEntry(method, msg, TraceLevel.DEBUG, evtKind, args);
                WriteTraceEntry(method, msg, TraceLevel.DEBUG, evtKind, args);
                //[-] string format = BuildEntry(TraceLevel.DEBUG, evtKind, method, msg);
                //[-] if (args != null && args.Any())
                //[-] {
                //[-] string.Format(format, args);
                //[-] }
            }
            catch (Exception ex)
            {
                TraceTraceError("{0} Log.Debug() - failed in method \"{2}\" while writing message \"{3}\" with exception: {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", LogCulture), ex.ToString(), method, msg);
            }
        }

        public static void WarningException(string method, Exception exception)
        {
            Warning(method, "failed with exception: {0}", exception.ToString());
        }

        public static void WarningException(string method, string msg, Exception exception)
        {
            Warning(method, "failed with exception: {0} - {1}", msg, exception);
        }

        public static void SystemRessources()
        {
            Process currentProcess = Process.GetCurrentProcess();
            Info("Log.SystemRessources()", "Process system memory process context usage: {0} ({1})", Environment.WorkingSet, Environment.WorkingSet.ToFileSize());
            Info("Log.SystemRessources()", "Process physical memory usage: {0} ({1})", currentProcess.WorkingSet64, currentProcess.WorkingSet64.ToFileSize());
            Info("Log.SystemRessources()", "Process NonpagedSystemMemorySize64 memory usage: {0} ({1})", currentProcess.NonpagedSystemMemorySize64, currentProcess.NonpagedSystemMemorySize64.ToFileSize());
            Info("Log.SystemRessources()", "Process PagedMemorySize64 memory usage: {0} ({1})", currentProcess.PagedMemorySize64, currentProcess.PagedMemorySize64.ToFileSize());
            Info("Log.SystemRessources()", "Process PrivateMemorySize64 memory usage: {0} ({1})", currentProcess.PrivateMemorySize64, currentProcess.PrivateMemorySize64.ToFileSize());
            Info("Log.SystemRessources()", "Process handle count: " + currentProcess.HandleCount);
            Info("Log.SystemRessources()", "Base priority of the associated process: " + currentProcess.BasePriority);
            Info("Log.SystemRessources()", "Priority class of the associated process: " + currentProcess.PriorityClass);
            Info("Log.SystemRessources()", "User Processor Time: " + currentProcess.UserProcessorTime.ToString());
            Info("Log.SystemRessources()", "Privileged Processor Time: " + currentProcess.PrivilegedProcessorTime.ToString());
            Info("Log.SystemRessources()", "Total Processor Time: " + currentProcess.TotalProcessorTime.ToString());
        }

        public static void SystemInformation()
        {
            try
            {
                string name = "System\\CurrentControlSet\\Control\\Windows";
                RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(name);
                string name2 = "ShutdownTime";
                long fileTime = BitConverter.ToInt64((byte[])registryKey.GetValue(name2), 0);
                Info(CurrentMethod(), "Last reboot time: {0}", DateTime.FromFileTime(fileTime));
                RdpSessionHistory();
                GetCurrentProcessInformation();
            }
            catch (Exception exception)
            {
                ErrorException(CurrentMethod(), exception);
            }
        }

        public static void RdpSessionHistory()
        {
            string query = "*[System/EventID=1149]";
            int num = 0;
            EventLogReader eventLogReader = new EventLogReader(new EventLogQuery("Microsoft-Windows-TerminalServices-RemoteConnectionManager/Operational", PathType.LogName, query) { ReverseDirection = true });
            for (EventRecord eventRecord = eventLogReader.ReadEvent(); eventRecord != null; eventRecord = eventLogReader.ReadEvent())
            {
                DateTime? timeCreated = eventRecord.TimeCreated;
                Info(CurrentMethod(), "RDP session was activated at: {0}", timeCreated);
                num++;
                if (num == 5)
                {
                    break;
                }
            }
        }

        public static void GetCurrentProcessInformation()
        {
            string processName = Process.GetCurrentProcess().ProcessName;
            DateTime startTime = Process.GetCurrentProcess().StartTime;
            Info(CurrentMethod(), "Process {0} started at {1}", processName, startTime);
        }

        private static int GetCallerPid(MessageHeaders headers)
        {
            if (headers == null)
            {
                return Process.GetCurrentProcess().Id;
            }

            int num = headers.FindHeader("PID", string.Empty);
            if (num >= 0)
            {
                return headers.GetHeader<int>(num);
            }

            return Process.GetCurrentProcess().Id;
        }

        private static string BuildEntry(TraceLevel level, EventKind kind, string method, string msg)
        {
            try
            {
                string text = (string.IsNullOrEmpty(method) ? "'method_not_set'" : method);
                string text2 = (string.IsNullOrEmpty(msg) ? "'msg was empty'" : msg.Replace("\"", "'"));
                if (LogCallerPid)
                {
                    int callerPid = GetCallerPid(OperationContext.Current?.IncomingMessageHeaders);
                    return string.Format(LogCulture, "{0} {1} [{2}] Thread-ID: [{3}] Caller-PID: [{4}] {5} - {6}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", LogCulture), level, kind, Thread.CurrentThread.ManagedThreadId, callerPid, text, text2);
                }

                return string.Format(LogCulture, "{0} {1} [{2}] ISTA: [{3}] {4} - {5}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", LogCulture), level, kind, Thread.CurrentThread.ManagedThreadId, text, text2);
            }
            catch (Exception ex)
            {
                TraceTraceError("{0} Log.BuildEntry() - failed with exception: {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", LogCulture), ex.ToString());
            }

            return "Log.BuildEntry() - failed";
        }

        [PreserveSource(Hint = "Replaced", OriginalHash = "C398BE0F33BAA776C088C861D41C9E99")]
        public static string CurrentMethod([CallerMemberName] string memberName = null, [CallerFilePath] string sourceFilePath = null)
        {
            StringBuilder sb = new StringBuilder();
            if (!string.IsNullOrEmpty(sourceFilePath))
            {
                sb.Append(Path.GetFileName(sourceFilePath));
            }

            if (!string.IsNullOrEmpty(memberName))
            {
                if (sb.Length > 0)
                {
                    sb.Append(": ");
                }

                sb.Append(memberName);
            }

            return sb.ToString();
        }

        private static void WriteTraceEntry(string method, string msg, TraceLevel level, EventKind evtKind, params object[] args)
        {
            string text = BuildEntry(level, evtKind, method, msg);
            Trace.WriteLine((args != null && args.Any()) ? string.Format(text, args) : text);
            //[+] if (log != null)
            if (log != null)
            //[+] {
            {
                //[+] string formattedMessage = (args != null && args.Any()) ? string.Format(text, args) : text;
                string formattedMessage = (args != null && args.Any()) ? string.Format(text, args) : text;
                //[+] switch (level)
                switch (level)
                //[+] {
                {
                    //[+] case TraceLevel.INFO:
                    case TraceLevel.INFO:
                        //[+] log.Info(formattedMessage);
                        log.Info(formattedMessage);
                        //[+] break;
                        break;
                    //[+] case TraceLevel.DEBUG:
                    case TraceLevel.DEBUG:
                        //[+] log.Debug(formattedMessage);
                        log.Debug(formattedMessage);
                        //[+] break;
                        break;
                    //[+] case TraceLevel.WARNING:
                    case TraceLevel.WARNING:
                        //[+] log.Warn(formattedMessage);
                        log.Warn(formattedMessage);
                        //[+] break;
                        break;
                    //[+] case TraceLevel.ERROR:
                    case TraceLevel.ERROR:
                    //[+] case TraceLevel.FATAL:
                    case TraceLevel.FATAL:
                        //[+] log.Error(formattedMessage);
                        log.Error(formattedMessage);
                        //[+] break;
                        break;
                //[+] }
                }
            //[+] }
            }
        }

        [PreserveSource(Hint = "log variable added")]
        private static readonly ILog log = LogManager.GetLogger(typeof(Log));
    }
}