using PsdzClient.Core;

namespace PsdzClient.Psdz
{
    public abstract class ProgrammingMessageListener
    {
        private readonly IProgMsgListener progMsgListener;

        internal ProgrammingMessageListener(IProgMsgListener progMsgListener)
        {
            this.progMsgListener = progMsgListener;
        }

        protected void LogDebug(string method, string msg, params object[] args)
        {
            Log.Debug(method, msg, args);
            if (progMsgListener != null)
            {
                progMsgListener.DebugMsg(string.Format(msg, args));
            }
        }

        protected void LogError(string method, string msg, params object[] args)
        {
            Log.Error(method, msg, args);
            if (progMsgListener != null)
            {
                progMsgListener.ErrorMsg(string.Format(msg, args));
            }
        }

        protected void LogInfo(string method, string msg, params object[] args)
        {
            Log.Info(method, msg, args);
            if (progMsgListener != null)
            {
                progMsgListener.InfoMsg(string.Format(msg, args));
            }
        }

        protected void LogWarn(string method, string msg, params object[] args)
        {
            Log.Warning(method, msg, args);
            if (progMsgListener != null)
            {
                progMsgListener.WarnMsg(string.Format(msg, args));
            }
        }
    }
}