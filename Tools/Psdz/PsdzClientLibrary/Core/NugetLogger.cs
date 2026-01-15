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
        [PreserveSource(Hint = "Arguments added", SignatureModified = true)]
        public string CurrentMethod([CallerMemberName] string memberName = null, [CallerFilePath] string sourceFilePath = null)
        {
            //[-] return Log.CurrentMethod(2);
            //[+] StringBuilder sb = new StringBuilder();
            StringBuilder sb = new StringBuilder();
            //[+] if (!string.IsNullOrEmpty(sourceFilePath))
            if (!string.IsNullOrEmpty(sourceFilePath))
            //[+] {
            {
                //[+] sb.Append(Path.GetFileName(sourceFilePath));
                sb.Append(Path.GetFileName(sourceFilePath));
                //[+] }
            }
            //[+] if (!string.IsNullOrEmpty(memberName))
            if (!string.IsNullOrEmpty(memberName))
            //[+] {
            {
                //[+] if (sb.Length > 0)
                if (sb.Length > 0)
                //[+] {
                {
                    //[+] sb.Append(": ");
                    sb.Append(": ");
                    //[+] }
                }
                //[+] sb.Append(memberName);
                sb.Append(memberName);
                //[+] }
            }
            //[+] return sb.ToString();
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