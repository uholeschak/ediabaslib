using log4net;
using System.Diagnostics;
using System.Reflection;

namespace PsdzClientLibrary.Core
{
    public class MultisourceLogger : IMultisourceLogger
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(MultisourceLogger));

        public string CurrentMethod()
        {
            string methodName = string.Empty;
            StackFrame[] stackFrames = new StackTrace().GetFrames();
            if (stackFrames != null && stackFrames.Length >= 2)
            {
                StackFrame callingframe = stackFrames[1];
                MethodBase methodBase = callingframe?.GetMethod();
                if (!string.IsNullOrEmpty(methodBase?.Name))
                {
                    methodName = methodBase.Name;
                }
            }

            return methodName;
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
