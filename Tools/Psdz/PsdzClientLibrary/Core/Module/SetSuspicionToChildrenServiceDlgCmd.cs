using BMW.Rheingold.CoreFramework;
using PsdzClient.Core;

namespace BMW.Rheingold.Module.ISTA
{
    internal class SetSuspicionToChildrenServiceDlgCmd : ServiceDialogCmdBase
    {
        public SetSuspicionToChildrenServiceDlgCmd(ISTAModule callingModule, string methodName, string path, IModuleExecutionParent globalTabModuleISTA, int elementNo) : base(callingModule, methodName, path, globalTabModuleISTA, elementNo)
        {
        }

        public override void CreateDialog(ParameterContainer inParam, ParameterContainer inoutParam)
        {
            Log.Info("SetSuspicionToChildrenServiceDlgCmd.CreateDialog()", "SetSuspicionToChildrenServiceDlg init started.");
            Display = false;
        }

        public override void DoInvoke(string method, ParameterContainer inParam, ParameterContainer outParam, ParameterContainer inoutParam)
        {
            if (CallingModule == null)
            {
                Log.Error("SetSuspicionToChildrenServiceDlgCmd.DoInvoke()", "Failed to invoke method {0}, because calling module is null.", method);
                return;
            }

            ModuleParameter value = CallingModule.__RheinGoldCoreModuleParameters__.Clone();
            inParam.Parameter.Add("__RheinGoldCoreModuleParameters__", value);
            inParam.Parameter.Add("__RheinGoldTabModuleISTA__", CallingModule.GlobalTabModuleISTA);
            inParam.Parameter.Add("__RheinGoldSOCAccessor__", CallingModule.SOCAccessor);
            if (method == "InitializeDialog")
            {
                bool bCollectiveResultSetNotOk = (bool)inParam.getParameter("bCollectiveResultSetNotOk", false);
                new SetSuspicionToChildrenServiceDlg(inParam).InitializeDialog(bCollectiveResultSetNotOk);
            }
            else
            {
                Log.Error("SetSuspicionToChildrenServiceDlgCmd.DoInvoke()", "Unsupported method {0} will be ignored.", method);
            }
        }
    }
}