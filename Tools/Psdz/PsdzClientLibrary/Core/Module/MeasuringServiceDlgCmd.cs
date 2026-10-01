using BMW.Rheingold.CoreFramework;
using BMW.Rheingold.Module.ISTA;
using PsdzClient.Core;
using PsdzClient.Core.Container;

namespace BMW.Rheingold.Module.ISTA
{
    internal class MeasuringServiceDlgCmd : ServiceDialogCmdBase
    {
        public MeasuringServiceDlgCmd(ISTAModule callingModule, string methodName, string path, IModuleExecutionParent globalTabModuleISTA, int elementNo)
            : base(callingModule, methodName, path, globalTabModuleISTA, elementNo)
        {
        }

        public override void DoInvoke(string method, ParameterContainer inParam, ParameterContainer outParam, ParameterContainer inoutParam)
        {
            if (base.MeasurmentService != null && base.MeasurmentService.IsConnectedToImib)
            {
                Log.Info("ServiceDialog.InitializeDialog()", "ReserveIMIBAdapter: nothing to do because MIB already connected.");
            }
            else
            {
                CheckConnectionToImib();
            }
            base.DoInvoke(method, inParam, outParam, inoutParam);
        }
    }

}
