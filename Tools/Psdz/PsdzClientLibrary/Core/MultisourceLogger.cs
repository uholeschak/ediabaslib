using log4net;
using PsdzClient;

namespace PsdzClientLibrary.Core
{
    public class MultisourceLogger : IMultisourceLogger
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(MultisourceLogger));

        public string CurrentMethod()
        {
            //return log.CurrentMethod();
            return string.Empty;
        }

        public void Info(string method, string msg, params object[] args)
        {
            log.InfoFormat(msg, args);
        }

        public void Debug(string method, string msg, params object[] args)
        {
            log.DebugFormat(msg, args);
        }

        public void Warning(string method, string msg, params object[] args)
        {
            log.WarnFormat(msg, args);
        }

        public void Error(string method, string msg, params object[] args)
        {
            log.ErrorFormat(msg, args);
        }
    }
}
