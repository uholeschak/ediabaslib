using BMW.Rheingold.CoreFramework;
using PsdzClient.Core.Container;
using System;

namespace BMW.Rheingold.Module.ISTA
{
    public interface IServiceDialogUI : IModuleExecutionStep
    {
        [Obsolete("Should eb removed.")]
        bool Done { get; set; }

        void InitDialog(ParameterContainer inParam, ParameterContainer inoutParam);

        ParameterContainer FinishDialog(ParameterContainer inoutParam);

        [Obsolete("Will be refactored soon!?")]
        bool WaitForContinue(IModuleExecutionParent parent, int timeout);
    }
}
