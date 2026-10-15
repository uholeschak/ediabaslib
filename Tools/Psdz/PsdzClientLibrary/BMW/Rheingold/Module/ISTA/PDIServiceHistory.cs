using BMW.Rheingold.CoreFramework;
using System.Collections.Generic;
using PsdzClient;

#pragma warning disable CS0169
namespace BMW.Rheingold.Module.ISTA
{
    internal class PDIServiceHistory : ISTAServiceDialog
    {
        [PreserveSource(Hint = "PDIServiceHistoryHandler", Placeholder = true)]
        private PlaceholderType _pdiServiceHistoryHandler;

        public PDIServiceHistory(ParameterContainer InParameter)
        {
            if (InParameter != null)
            {
                _globalModuleInParameter = InParameter;
            }
            __handleInParameter();
        }

        public virtual void Prepare()
        {
        }

        public virtual void Reset()
        {
        }

        [PreserveSource(Cleaned = true)]
        public override void Invoke(string method, ParameterContainer inParam, ParameterContainer outParam, ParameterContainer inoutParam)
        {
            Logger.Info("PDIServiceHistory.Invoke", "entering method");
            Logger.Info("PDIServiceHistory.Invoke", "leaving method");
        }

        [PreserveSource(Cleaned = true)]
        public virtual void SendPDIEntry(ref bool Success, ref List<string> Errors)
        {
            string text = "SendPDIEntry";
            int num = 0;
            Logger.WriteInformation(text + " called");
            Logger.WriteInformation("_ExitIndex is: {0}", num);
        }
    }
}
