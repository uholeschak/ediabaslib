using BMW.Rheingold.Psdz;
using Newtonsoft.Json;
using PsdzClient.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BMW.Rheingold.Psdz
{
    internal static class PsdzWebserviceRegistrar
    {
        private class SessionData
        {
            public IList<int> IstaProcessIds { get; set; }

            public int PsdzWebserviceProcessId { get; set; }

            public int PsdzWebServicePort { get; set; }

            public WebserviceSessionStatus Status { get; set; }
        }

        private const string PSDZ_WEBSERVICE_SESSIONS_FILENAME = "PsdzWebserviceSessions.json";

        private const string PSDZ_WEBSERVICE_SESSIONS_LOCK_FILENAME = "PsdzWebserviceSessions.lck";

        private static readonly string sessionDataFilePath;

        private static readonly string lockFilePath;

        public static int RegisterCurrentProcess()
        {
            Process process = Process.GetCurrentProcess();
            Log.Info(Log.CurrentMethod(), $"Registering ISTA process {process.ProcessName} ({process.Id}).");
            using (FileStream lockFileStream = GetReadWriteFileLock())
            {
                List<SessionData> list = ReadSessionsFromFile(lockFileStream);
                SessionData sessionData = (ConfigSettings.GetActivateSdpOnlinePatch() ? list.FirstOrDefault((SessionData session) => session.IstaProcessIds.Contains(process.Id)) : list.FirstOrDefault());
                if (sessionData == null)
                {
                    Log.Debug(Log.CurrentMethod(), "No appropriate session was found, so creating a new one.");
                    sessionData = new SessionData
                    {
                        IstaProcessIds = new List<int> { process.Id },
                        PsdzWebServicePort = ConfigSettings.getConfigint("BMW.Rheingold.Programming.PsdzWebservice.Port", -1)
                    };
                    if (sessionData.PsdzWebServicePort == -1)
                    {
                        sessionData.PsdzWebServicePort = GetPort(list);
                    }
                    list.Add(sessionData);
                }
                else
                {
                    sessionData.IstaProcessIds.AddIfNotContains(process.Id);
                }
                WriteSessionsToFile(lockFileStream, list);
                return sessionData.PsdzWebServicePort;
            }
        }

        public static bool StartAndRegisterWebserviceProcess(Process webserviceProcess)
        {
            Process istaProcess = Process.GetCurrentProcess();
            Log.Info(Log.CurrentMethod(), "Starting the process: \n{0} {1}\nISTA process: {2} ({3})", webserviceProcess.StartInfo.FileName, webserviceProcess.StartInfo.Arguments, istaProcess.ProcessName, istaProcess.Id);
            bool configStringAsBoolean = ConfigSettings.getConfigStringAsBoolean("BMW.Rheingold.Programming.PsdzWebservice.SkipProcessStart");
            using (FileStream lockFileStream = GetReadWriteFileLock())
            {
                List<SessionData> list = ReadSessionsFromFile(lockFileStream);
                SessionData sessionData = list.First((SessionData session) => session.IstaProcessIds.Contains(istaProcess.Id));
                if (sessionData.Status != WebserviceSessionStatus.Created)
                {
                    Log.Info(Log.CurrentMethod(), $"A webservice process has already been started, no need to start another. Webservice process: {sessionData.PsdzWebserviceProcessId}");
                    return false;
                }
                if (configStringAsBoolean)
                {
                    Log.Info(Log.CurrentMethod(), "SkipProcessStart is true, so we will not start the process and we'll leave the webservice PID at the default value of 0.");
                }
                else
                {
                    webserviceProcess.Start();
                    sessionData.PsdzWebserviceProcessId = webserviceProcess.Id;
                }
                sessionData.Status = WebserviceSessionStatus.ProcessStarted;
                WriteSessionsToFile(lockFileStream, list);
            }
            string text = (configStringAsBoolean ? "(no process actually started)" : webserviceProcess.Id.ToString());
            Log.Info(Log.CurrentMethod(), "Started webservice process: " + text);
            return true;
        }

        public static void SignalWebserviceInitializationCompleted()
        {
            Process process = Process.GetCurrentProcess();
            using (FileStream lockFileStream = GetReadWriteFileLock())
            {
                List<SessionData> list = ReadSessionsFromFile(lockFileStream);
                IEnumerable<SessionData> source = list.Where((SessionData s) => s.IstaProcessIds.Contains(process.Id));
                if (!source.Any())
                {
                    throw new InvalidOperationException($"No webservice session was registered for ISTA process {process.ProcessName} ({process.Id}).");
                }
                source.Single().Status = WebserviceSessionStatus.Running;
                WriteSessionsToFile(lockFileStream, list);
            }
            Log.Info(Log.CurrentMethod(), $"The webservice session for ISTA process {process.ProcessName} ({process.Id}) is now marked as running, so other threads can use it.");
        }

        public static bool DeregisterCurrentProcess()
        {
            Process process = Process.GetCurrentProcess();
            using (FileStream lockFileStream = GetReadWriteFileLock())
            {
                List<SessionData> list = ReadSessionsFromFile(lockFileStream);
                IEnumerable<SessionData> source = list.Where((SessionData s) => s.IstaProcessIds.Contains(process.Id));
                if (!source.Any())
                {
                    Log.Warning(Log.CurrentMethod(), $"No webservice session was registered for ISTA process {process.ProcessName} ({process.Id}). We were going to deregister anyway, but this may be a sign of some problem.");
                    return false;
                }
                SessionData sessionData = source.Single();
                if (sessionData.IstaProcessIds.Count == 1)
                {
                    if (ConfigSettings.getConfigStringAsBoolean("BMW.Rheingold.Programming.PsdzWebservice.SkipProcessStart"))
                    {
                        Log.Info(Log.CurrentMethod(), "SkipProcessStart is true, so it is not our responsibility to kill the webservice process.");
                    }
                    else if (sessionData.PsdzWebserviceProcessId == 0)
                    {
                        Log.Warning(Log.CurrentMethod(), $"No webservice process was registered for the session corresponding to ISTA process {process.ProcessName} ({process.Id}), so there's nothing to kill. However, this could be indicative of some problem. Session status: {sessionData.Status}");
                    }
                    else
                    {
                        try
                        {
                            Process.GetProcessById(sessionData.PsdzWebserviceProcessId).Kill();
                            Log.Info(Log.CurrentMethod(), "The Psdz Webservice has been terminated successfully.");
                        }
                        catch (Exception exception)
                        {
                            Log.WarningException(Log.CurrentMethod(), exception);
                        }
                    }
                    list.Remove(sessionData);
                    WriteSessionsToFile(lockFileStream, list);
                    return true;
                }
                Log.Info(Log.CurrentMethod(), "Shutdown has been requested, but other ISTA processes also use this session. Shutdown will not be executed yet.");
                sessionData.IstaProcessIds.Remove(process.Id);
                WriteSessionsToFile(lockFileStream, list);
                return false;
            }
        }

        public static WebserviceSessionStatus GetWebserviceStatus()
        {
            Process process = Process.GetCurrentProcess();
            List<SessionData> source;
            using (FileStream lockFileStream = GetReadonlyFileLock())
            {
                source = ReadSessionsFromFile(lockFileStream);
            }
            IEnumerable<SessionData> source2 = source.Where((SessionData s) => s.IstaProcessIds.Contains(process.Id));
            if (!source2.Any())
            {
                throw new InvalidOperationException($"No webservice session was registered for ISTA process {process.ProcessName} ({process.Id}).");
            }
            return source2.Single().Status;
        }

        private static int GetPort(IEnumerable<SessionData> allSessions)
        {
            Log.Debug(Log.CurrentMethod(), "Looking for a free port to initialize the Psdz Webservice...");
            List<int> list = (from s in allSessions
                              select s.PsdzWebServicePort into p
                              orderby p
                              select p).ToList();
            foreach (int item in list)
            {
                Log.Debug(Log.CurrentMethod(), $"Port {item} is already used by another instance.");
            }
            int num = list.LastOrDefault();
            return NetUtils.GetFirstFreePort((num == 0) ? 50000 : (num + 1), 50200);
        }

        static PsdzWebserviceRegistrar()
        {
            sessionDataFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ISTA", "PsdzWebserviceSessions.json");
            lockFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ISTA", "PsdzWebserviceSessions.lck");
            Directory.CreateDirectory(Path.GetDirectoryName(sessionDataFilePath));
            if (!File.Exists(sessionDataFilePath))
            {
                File.Create(sessionDataFilePath).Close();
            }
            if (!File.Exists(lockFilePath))
            {
                File.Create(lockFilePath).Close();
            }
        }

        private static void WriteSessionsToFile(FileStream lockFileStream, List<SessionData> allSessions)
        {
            if (!lockFileStream.CanWrite)
            {
                throw new IOException("Lockfile stream does not have write permission (or may have been closed). This means we cannot safely write to the sessions file.");
            }
            string text = JsonConvert.SerializeObject(allSessions, Formatting.Indented);
            using (StreamWriter streamWriter = new StreamWriter(sessionDataFilePath, append: false))
            {
                streamWriter.Write(text);
            }
            Log.Debug(Log.CurrentMethod(), "The following content has been written to the Session Data file: \n" + text);
        }

        private static List<SessionData> ReadSessionsFromFile(FileStream lockFileStream)
        {
            if (!lockFileStream.CanRead)
            {
                throw new IOException("Lockfile stream does not have read permission (or may have been closed). This means we cannot safely read from the sessions file.");
            }
            string text;
            using (StreamReader streamReader = new StreamReader(sessionDataFilePath, detectEncodingFromByteOrderMarks: true))
            {
                text = streamReader.ReadToEnd();
            }
            List<SessionData> list = JsonConvert.DeserializeObject<List<SessionData>>(text) ?? new List<SessionData>();
            Log.Debug(Log.CurrentMethod(), $"{list.Count} PSdZ Webservice session(s) have been read out from the file {sessionDataFilePath}: \n{text}");
            return list;
        }

        private static FileStream GetReadWriteFileLock()
        {
            return GetFileLock(FileAccess.ReadWrite, FileShare.None);
        }

        private static FileStream GetReadonlyFileLock()
        {
            return GetFileLock(FileAccess.Read, FileShare.Read);
        }

        private static FileStream GetFileLock(FileAccess fileAccess, FileShare fileShare)
        {
            int num = 15;
            while (true)
            {
                try
                {
                    return File.Open(lockFilePath, FileMode.Open, fileAccess, fileShare);
                }
                catch (IOException exception)
                {
                    Log.Warning(Log.CurrentMethod(), $"PsdzWebservice session file seems to be in use by a different thread or process (cannot obtain lock). {--num} retries left.");
                    if (num == 0)
                    {
                        Log.ErrorException(Log.CurrentMethod(), "Out of retries, so rethrowing exception: ", exception);
                        throw;
                    }
                    Task.Delay(50);
                }
            }
        }
    }
}