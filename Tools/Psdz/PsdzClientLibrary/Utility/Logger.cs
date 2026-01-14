using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Threading;
using log4net;
using log4net.Core;
using log4net.Repository.Hierarchy;

namespace PsdzClient.Utility
{
    public class Logger
    {
        private readonly ILog _logger;

        private readonly string _iCsVersion;

        private static bool _initGuard = false;

        private static readonly object _initLock = new object();

        private static Logger loggerInstance;

        internal Logger()
        {
            _logger = LogManager.GetLogger(typeof(Logger));
            _iCsVersion = GetCommonServiceVersionString();
        }

        internal static string GetCommonServiceVersionString()
        {
            Version version = Assembly.GetExecutingAssembly().GetName().Version;
            if (!(version != null))
            {
                return " (iCS Version unknown)";
            }
            return " (iCS Version:" + version?.ToString() + ")";
        }

        public static Logger Instance()
        {
            if (loggerInstance != null)
            {
                return loggerInstance;
            }
            lock (_initLock)
            {
                if (loggerInstance != null)
                {
                    return loggerInstance;
                }
                if (_initGuard)
                {
                    return null;
                }
                _initGuard = true;
                Logger logger = new Logger();
                Thread.MemoryBarrier();
                loggerInstance = logger;
            }
            return loggerInstance;
        }

        public void Log(ICSEventId icsEventId, string location, string logMessage, EventKind kind, LogLevel logLevel)
        {
            Log(GetLogMessage(icsEventId, location, logMessage), kind, icsEventId.ToText(), logLevel);
        }

        public void LogEncrypted(ICSEventId icsEventId, string location, string logMessage, EventKind kind, LogLevel logLevel)
        {
            string logMessage2 = Encryption.EncryptSensitveContent(logMessage);
            Log(GetLogMessage(icsEventId, location, logMessage2), kind, icsEventId.ToText(), logLevel);
        }

        public void Log(ICSEventId icsEventId, string location, string logMessage, EventKind kind, LogLevel logLevel, Exception exception)
        {
            Log(GetLogMessage(icsEventId, location, logMessage), kind, icsEventId.ToText(), logLevel, exception);
        }

        private void Log(string logMessage, EventKind kind, string eventId, LogLevel logLevel)
        {
            Log(logMessage, kind, eventId, logLevel, null);
        }

        private void Log(string logMessage, EventKind kind, string eventId, LogLevel logLevel, Exception exception)
        {
            ThreadContext.Properties["EventKind"] = kind.ToString().ToUpper()[0];
            ThreadContext.Properties["EventId"] = eventId;
            ThreadContext.Properties["ComponentName"] = "_ICS";
            logMessage = ((logLevel >= LogLevel.Warning) ? (logMessage + _iCsVersion) : logMessage);
            switch (logLevel)
            {
                case LogLevel.Debug:
                    if (exception == null)
                    {
                        _logger.Debug(logMessage);
                    }
                    else
                    {
                        _logger.Debug(logMessage, exception);
                    }
                    break;
                case LogLevel.Error:
                    if (exception == null)
                    {
                        _logger.Error(logMessage);
                    }
                    else
                    {
                        _logger.Error(logMessage, exception);
                    }
                    break;
                case LogLevel.Fatal:
                    if (exception == null)
                    {
                        _logger.Fatal(logMessage);
                    }
                    else
                    {
                        _logger.Fatal(logMessage, exception);
                    }
                    break;
                case LogLevel.Info:
                    if (exception == null)
                    {
                        _logger.Info(logMessage);
                    }
                    else
                    {
                        _logger.Info(logMessage, exception);
                    }
                    break;
                case LogLevel.Warning:
                    if (exception == null)
                    {
                        _logger.Warn(logMessage);
                    }
                    else
                    {
                        _logger.Warn(logMessage, exception);
                    }
                    break;
            }
        }

        private string GetLogMessage(ICSEventId icsEventId, string location, string logMessage)
        {
            return ": " + GetResourceMessage(icsEventId, EventIdCatalog.ResourceManager) + ": " + location + ": " + logMessage;
        }

        private string GetResourceMessage<T>(T errCode, params ResourceManager[] resManagers)
        {
            return GetResourceMessage(errCode, new CultureInfo("en"), resManagers);
        }

        private string GetResourceMessage<T>(T errCode, CultureInfo cultureInfo, params ResourceManager[] resManagers)
        {
            string result = null;
            try
            {
                result = resManagers.Select((ResourceManager m) => TryResourceManager(errCode, cultureInfo, m)).FirstOrDefault((string s) => !string.IsNullOrEmpty(s));
            }
            catch (InvalidOperationException)
            {
            }
            catch (MissingSatelliteAssemblyException)
            {
            }
            catch (MissingManifestResourceException)
            {
            }
            return result;
        }

        private static string TryResourceManager<T>(T errCode, CultureInfo cultureInfo, ResourceManager rm)
        {
            if (errCode == null || cultureInfo == null || rm == null)
            {
                return string.Empty;
            }
            object obj = rm.GetString(errCode.ToString(), cultureInfo);
            if (obj == null)
            {
                obj = rm.GetString(errCode.ToText(), cultureInfo);
                if (obj == null)
                {
                    T val = errCode;
                    obj = rm.GetString("_" + val, cultureInfo) ?? rm.GetString("_" + errCode.ToText(), cultureInfo);
                }
            }
            return (string)obj;
        }

        public void ChangeLogLevel(string logLevel)
        {
            Level level;
            switch (logLevel)
            {
                case "Info":
                    level = Level.Info;
                    break;
                case "Debug":
                    level = Level.Debug;
                    break;
                case "Warning":
                    level = Level.Warn;
                    break;
                case "Error":
                    level = Level.Error;
                    break;
                case "Fatal":
                    level = Level.Fatal;
                    break;
                default:
                    level = Level.Info;
                    break;
            }
            Log(ICSEventId.ICS0022, "ChangeLogLevel:ChangeLogLevel", "Switching Loglevel to " + level.DisplayName, EventKind.Technical, LogLevel.Info);
            ((Hierarchy)LogManager.GetRepository()).Root.Level = level;
            ((Hierarchy)LogManager.GetRepository()).RaiseConfigurationChanged(EventArgs.Empty);
        }
    }
}