using System;
using System.Runtime.CompilerServices;
using PsdzClient;

namespace BMW.Rheingold.ISTA.CoreFramework
{
    public interface ILogger
    {
        [PreserveSource(Hint = "Combined", Added = true)]
        void Debug(string method, string msg, params object[] args);
        [PreserveSource(Hint = "Combined", Added = true)]
        void Error(string method, string msg, params object[] args);
        [PreserveSource(Hint = "Combined", Added = true)]
        void ErrorException(string method, Exception exception);
        [PreserveSource(Hint = "Combined", Added = true)]
        void ErrorException(string method, string msg, Exception exception);
        void Info(string method, string msg, params object[] args);
        void Warning(string method, string msg, params object[] args);
        void WarningException(string method, Exception exception);
        [PreserveSource(Hint = "Combined", Added = true)]
        void WarningException(string method, string msg, Exception exception);

        void WriteInformation(string msg, params object[] parmArray);

        void WriteError(string msg, params object[] parmArray);

        void WriteFatalError(string msg, params object[] parmArray);
        [PreserveSource(Hint = "Arguments modified", Added = true)]
        string CurrentMethod([CallerMemberName] string memberName = null, [CallerFilePath] string sourceFilePath = null);
    }
}