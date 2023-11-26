namespace PsdzClientLibrary.Core
{
    internal class LogValue
    {
        public string Method { get; private set; }

        public string Message { get; private set; }

        public object[] Arguments { get; private set; }

        public LogValue(string method, string msg, params object[] args)
        {
            Arguments = args;
            Method = method;
            Message = msg;
        }
    }
}
