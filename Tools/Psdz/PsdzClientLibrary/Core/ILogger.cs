using System;
using System.Runtime.CompilerServices;

namespace PsdzClientLibrary.Core
{
    public interface ILogger
    {
        void Debug(string method, string msg, params object[] args);

        void Error(string method, string msg, params object[] args);

        void ErrorException(string method, Exception exception);

        void Info(string method, string msg, params object[] args);

        void Warning(string method, string msg, params object[] args);

        void WarningException(string method, Exception exception);

        string CurrentMethod([CallerMemberName] string memberName = null, [CallerFilePath] string sourceFilePath = null);
    }
}
