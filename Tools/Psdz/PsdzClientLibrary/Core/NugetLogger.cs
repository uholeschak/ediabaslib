using System;
using System.Runtime.CompilerServices;

namespace PsdzClient.Core
{
    public class NugetLogger : ILogger
    {
        [PreserveSource(Hint = "Arguments added", SignatureModified = true)]
        public string CurrentMethod([CallerMemberName] string memberName = null, [CallerFilePath] string sourceFilePath = null)
        {
            //[-] return Log.CurrentMethod(2);
            //[+] return Log.CurrentMethod(memberName, sourceFilePath);
            return Log.CurrentMethod(memberName, sourceFilePath);
        }

        public void Info(string method, string msg, params object[] args)
        {
            Log.Info(method, msg, args);
        }

        public void Debug(string method, string msg, params object[] args)
        {
            Log.Debug(method, msg, args);
        }

        public void Warning(string method, string msg, params object[] args)
        {
            Log.Warning(method, msg, args);
        }

        public void Error(string method, string msg, params object[] args)
        {
            Log.Error(method, msg, args);
        }

        public void ErrorException(string method, Exception exception)
        {
            Log.ErrorException(method, exception);
        }

        public void ErrorException(string method, string msg, Exception exception)
        {
            Log.ErrorException(method, msg, exception);
        }

        public void WarningException(string method, Exception exception)
        {
            Log.WarningException(method, exception);
        }

        public void WarningException(string method, string msg, Exception exception)
        {
            Log.WarningException(method, msg, exception);
        }
    }
}