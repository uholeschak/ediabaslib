using log4net;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Interop;

namespace PsdzClient.Core
{
    public class NugetLogger : ILogger
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(NugetLogger));

        // [UH] replaced
        public string CurrentMethod([CallerMemberName] string memberName = null, [CallerFilePath] string sourceFilePath = null)
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

        public void Info(string method, string msg, params object[] args)
        {
            log.Info(FormatLogMsg(method, msg, args));
        }

        public void Debug(string method, string msg, params object[] args)
        {
            log.Debug(FormatLogMsg(method, msg, args));
        }

        public void Warning(string method, string msg, params object[] args)
        {
            log.Warn(FormatLogMsg(method, msg, args));
        }

        public void WarningException(string method, Exception exception)
        {
            Warning(method, "failed with exception: {0}", exception.ToString());
        }

        public void Error(string method, string msg, params object[] args)
        {
            log.Error(FormatLogMsg(method, msg, args));
        }

        public void ErrorException(string method, Exception exception)
        {
            Error(method, "failed with exception: {0}", exception.ToString());
        }

        private string FormatLogMsg(string method, string msg, params object[] args)
        {
            string logText = string.Format(msg, args);
            if (!string.IsNullOrEmpty(method))
            {
                logText = method + ": " + logText;
            }

            return logText;
        }
    }
}
