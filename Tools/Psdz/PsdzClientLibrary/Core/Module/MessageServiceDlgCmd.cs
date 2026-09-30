using BMW.Rheingold.CoreFramework;
using BMW.Rheingold.Module.ISTA;
using PsdzClient.Core.Container;

namespace BMW.Rheingold.Module.ISTA
{
    internal class MessageServiceDlgCmd : ServiceDialogCmdBase
    {
        public MessageServiceDlgCmd(ISTAModule callingModule, string methodName, string path, IModuleExecutionParent globalTabModuleISTA, int elementNo)
            : base(callingModule, methodName, path, globalTabModuleISTA, elementNo)
        {
        }

        public override void InitializeInput(string method, ParameterContainer inParam, ParameterContainer inoutParam)
        {
            if ("HideDialog".Equals(method))
            {
                base.Display = false;
            }
        }
    }
}
