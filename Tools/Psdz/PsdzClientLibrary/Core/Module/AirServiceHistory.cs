using BMW.Rheingold.Module.ISTA;
using BMW.Rheingold.RheingoldSessionController;
using PsdzClient;
using PsdzClient.Core;
using PsdzClient.Core.Container;
using System;

#pragma warning disable CS0169
namespace BMW.Rheingold.Module.ISTA
{
    internal class AirServiceHistory : ISTAServiceDialog
    {
        [PreserveSource(Hint = "AirServiceHistoryHandler", Placeholder = true)]
        private PlaceholderType airServiceHistoryHandler;

        public AirServiceHistory(ParameterContainer InParameter)
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
        }

        [PreserveSource(Cleaned = true)]
        private void SH_LP2()
        {
            int num = 0;
            Logger.WriteInformation("SH_LP2 called");
            Logger.WriteInformation("_ExitIndex is: {0}", num);
        }
    }
}
