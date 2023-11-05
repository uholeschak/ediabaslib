namespace PsdzClientLibrary.Core
{
    public interface IMultisourceLogger
    {
        void Info(string method, string msg, params object[] args);

        void Debug(string method, string msg, params object[] args);

        void Warning(string method, string msg, params object[] args);

        void Error(string method, string msg, params object[] args);

        string CurrentMethod();
    }
}
