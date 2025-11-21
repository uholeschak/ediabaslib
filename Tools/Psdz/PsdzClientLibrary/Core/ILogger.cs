using PsdzClient;
using System;
using System.Runtime.CompilerServices;

namespace PsdzClient.Core
{
    public interface ILogger
    {
        void Debug(string method, string msg, params object[] args);
        void Error(string method, string msg, params object[] args);
        void ErrorException(string method, Exception exception);
        void ErrorException(string method, string msg, Exception exception);
        void Info(string method, string msg, params object[] args);
        void Warning(string method, string msg, params object[] args);
        void WarningException(string method, Exception exception);
        void WarningException(string method, string msg, Exception exception);
        [PreserveSource(Hint = "Modified")]
        string CurrentMethod([CallerMemberName] string memberName = null, [CallerFilePath] string sourceFilePath = null);
    }
}