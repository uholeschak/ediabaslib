using BMW.Rheingold.CoreFramework;
using BMW.Rheingold.Module.ISTA;
using PsdzClient.Core;
using PsdzClient.Core.Container;

namespace BMW.Rheingold.ISTA.CoreFramework.ServiceDialoge.Multisession
{
    public class IstaOperationServiceDialogUi : IServiceDialogUI, IModuleExecutionStep
    {
        public bool IsDialogShown { get; set; }

        public object DataContext { get; set; }

        public bool Done { get; set; }

        public IstaOperationServiceDialogUi(ParameterContainer inParam)
        {
            Log.Info("IstaOperationServiceDialogUi.IstaOperationServiceDialogUi()", "called.");
        }

        public void InitDialog(ParameterContainer inParam, ParameterContainer inoutParam)
        {
            Log.Info("IstaOperationServiceDialogUi.InitDialog()", "called.");
        }

        public ParameterContainer FinishDialog(ParameterContainer inoutParam)
        {
            Log.Info("IstaOperationServiceDialogUi.FinishDialog()", "called.");
            return null;
        }

        public bool WaitForContinue(IModuleExecutionParent parent, int timeout)
        {
            Log.Info("IstaOperationServiceDialogUi.WaitForContinue()", "called.");
            return true;
        }
    }
}
