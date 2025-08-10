using System;

namespace PsdzClientLibrary.Psdz
{
    public interface IProgMsgListener
    {
        void DebugMsg(string msg);

        void InfoMsg(string msg);

        void WarnMsg(string msg);

        void ErrorMsg(string msg);

        void CriticalMsg(string msg);

        void CriticalMsg(string msg, Exception e);
    }
}