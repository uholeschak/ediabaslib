using log4net;
using PsdzClient;
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
        [PreserveSource(Hint = "Arguments changed", OriginalHash = "B2DDF7AEEAB7F72B627236C48A296719")]
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