using BMW.Rheingold.Psdz;
using PsdzClient.Core;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using RestSharp;

namespace BMW.Rheingold.Psdz
{
    public class LogService : ILogService
    {
        private readonly IWebCallHandler _webCallHandler;

        private readonly string _clientId;

        private readonly string _clientLogDir;

        private readonly string _endpointService = "logging";

        private readonly IDictionary<string, string> _psdzLogFiles = new Dictionary<string, string>();

        private const PsdzLoglevel StandardPsdzLoglevel = PsdzLoglevel.FINE;

        public LogService(IWebCallHandler webCallHandler, string clientId, string clientLogDir)
        {
            _webCallHandler = webCallHandler;
            _clientId = clientId;
            _clientLogDir = clientLogDir;
        }

        public string ClosePsdzLog()
        {
            try
            {
                string text = null;
                lock (_psdzLogFiles)
                {
                    if (_psdzLogFiles.ContainsKey(_clientId))
                    {
                        _webCallHandler.ExecuteRequest(_endpointService, "closelog/" + _clientId, Method.Post);
                        text = _psdzLogFiles[_clientId];
                        Log.Debug(Log.CurrentMethod(), "Stopped writing to temporary log file '" + text + "' for Client-ID '" + _clientId + "'");
                        _psdzLogFiles.Remove(_clientId);
                    }
                    else
                    {
                        Log.Warning(Log.CurrentMethod(), "No log file available for Client-ID '" + _clientId + "'.");
                    }
                }
                return text;
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }

        public void SetLogLevel(PsdzLoglevel psdzLogLevel = PsdzLoglevel.FINE)
        {
            try
            {
                SetThresholdRequestModel requestBodyObject = new SetThresholdRequestModel
                {
                    Threshold = (int)psdzLogLevel
                };
                _webCallHandler.ExecuteRequest(_endpointService, "setthreshold", Method.Post, requestBodyObject);
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }

        public void PrepareLoggingForCurrentThread()
        {
            lock (_psdzLogFiles)
            {
                if (_psdzLogFiles.ContainsKey(_clientId))
                {
                    string currentLogIdOrDefault = GetCurrentLogIdOrDefault();
                    if (!string.Equals(_clientId, currentLogIdOrDefault, StringComparison.OrdinalIgnoreCase))
                    {
                        string text = _psdzLogFiles[_clientId];
                        Log.Debug(Log.CurrentMethod(), "Set log file for Client-ID '" + _clientId + "': " + text);
                        SetLogId();
                    }
                }
                else
                {
                    Log.Info(Log.CurrentMethod(), "clientLogDir is " + _clientLogDir);
                    CreateLogFileForClient();
                }
            }
        }

        private string GetCurrentLogIdOrDefault()
        {
            try
            {
                return _webCallHandler.ExecuteRequest<string>(_endpointService, "getcurrentlogidordefault", Method.Get).Data;
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }

        private bool SetLogId(string logFilePath = null)
        {
            try
            {
                SetLogIdRequestModel requestBodyObject = new SetLogIdRequestModel
                {
                    LogFilePath = logFilePath
                };
                return _webCallHandler.ExecuteRequest(_endpointService, "setlogid/" + _clientId, Method.Post, requestBodyObject).IsSuccessful;
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                return false;
            }
        }

        private void CreateLogFileForClient()
        {
            string loggingDir = GetLoggingDir(_clientLogDir);
            string text;
            do
            {
                string path = string.Format(CultureInfo.InvariantCulture, "psdz_{0}.log", Guid.NewGuid());
                text = Path.Combine(loggingDir, path);
            }
            while (File.Exists(text));
            Log.Debug(Log.CurrentMethod(), "Added log file for Client-ID '" + _clientId + "': " + text);
            if (SetLogId(text))
            {
                _psdzLogFiles.Add(_clientId, text);
            }
        }

        private string GetLoggingDir(string preferredLoggingDir)
        {
            if (string.IsNullOrEmpty(preferredLoggingDir))
            {
                Log.Info(Log.CurrentMethod(), "Preferred logging directory is not specified. Use following temporary path instead: '" + Path.GetTempPath() + "'");
                return Path.GetTempPath();
            }
            string fullPath = Path.GetFullPath(preferredLoggingDir);
            if (!IsDirectoryWriteable(fullPath))
            {
                Log.Warning(Log.CurrentMethod(), "Preferred logging directory ('" + fullPath + "') is not accessible. Use following temporary path instead: '" + Path.GetTempPath() + "'");
                return Path.GetTempPath();
            }
            return fullPath;
        }

        private bool IsDirectoryWriteable(string directory)
        {
            try
            {
                string path;
                do
                {
                    path = Path.Combine(directory, Guid.NewGuid().ToString());
                }
                while (File.Exists(path));
                File.WriteAllText(path, string.Empty);
                File.Delete(path);
                return true;
            }
            catch (Exception exception)
            {
                Log.WarningException(Log.CurrentMethod(), exception);
                return false;
            }
        }
    }
}