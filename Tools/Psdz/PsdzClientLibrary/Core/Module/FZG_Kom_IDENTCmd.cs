using BMW.Rheingold.CoreFramework;
using BMW.Rheingold.CoreFramework.Contracts;
using BMW.Rheingold.Module.ISTA;

namespace BMW.Rheingold.Module.ISTA
{
    internal class FZG_Kom_IDENTCmd : ServiceDialogCmdBase
    {
        public FZG_Kom_IDENTCmd(ISTAModule callingModule, string methodName, string path, IModuleExecutionParent globalTabModuleISTA, int elementNo) : base(callingModule, methodName, path, globalTabModuleISTA, elementNo)
        {
        }

        public override void CreateDialog(ParameterContainer inParam, ParameterContainer inoutParam)
        {
            Log.Info("FZG_Kom_IDENTCmd.CreateDialog()", $"{ServiceDialogConfig.Name} init started.");
            Display = false;
        }

        public override void DoInvoke(string method, ParameterContainer inParam, ParameterContainer outParam, ParameterContainer inoutParam)
        {
            if (CallingModule == null)
            {
                Log.Error("FZG_Kom_IDENTCmd.DoInvoke()", "Failed to invoke method {0}, because calling module is null.", method);
                return;
            }

            Log.Info("FZG_Kom_IDENTCmd.DoInvoke()", "called with method: {0}", method);
            ModuleParameter value = CallingModule.__RheinGoldCoreModuleParameters__.Clone();
            inParam.Parameter.Add("__RheinGoldCoreModuleParameters__", value);
            inParam.Parameter.Add("__RheinGoldTabModuleISTA__", CallingModule.GlobalTabModuleISTA);
            inParam.Parameter.Add("__RheinGoldSOCAccessor__", CallingModule.SOCAccessor);
            if (method == "SG_20")
            {
                string[] sG_gruppe = (string[])inParam.getParameter("SG_gruppe", new string[0]);
                ITextLocator[] steuergeraet = (ITextLocator[])inParam.getParameter("Steuergeraet", new TextLocator[0]);
                string[] verdacht_Versorgung = (string[])inParam.getParameter("Verdacht_Versorgung", new string[0]);
                string[] Status_Ident = null;
                string[] Sgbd = null;
                string Status_Ident_ges = null;
                new FZG_Kom_IDENT(inParam).SG_20(sG_gruppe, steuergeraet, verdacht_Versorgung, ref Status_Ident, ref Sgbd, ref Status_Ident_ges);
                outParam.setParameter("Status_Ident", Status_Ident);
                outParam.setParameter("Sgbd", Sgbd);
                outParam.setParameter("Status_Ident_ges", Status_Ident_ges);
            }
            else
            {
                Log.Error("FZG_Kom_IDENTCmd.DoInvoke()", "Unsupported method {0} will be ignored.", method);
            }
        }
    }
}
