using log4net;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace PsdzClientLibrary.Core
{
    public class MultisourceLogger : IMultisourceLogger
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(MultisourceLogger));

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

        public void Error(string method, string msg, params object[] args)
        {
            log.Error(FormatLogMsg(method, msg, args));
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
