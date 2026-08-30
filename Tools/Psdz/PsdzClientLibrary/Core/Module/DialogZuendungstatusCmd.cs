using System;
using BMW.Rheingold.CoreFramework;
using BMW.Rheingold.Module.ISTA;
using PsdzClient.Core;
using PsdzClient.Core.Container;

namespace BMW.Rheingold.Module.ISTA
{
    internal class DialogZuendungstatusCmd : ServiceDialogCmdBase
    {
        public DialogZuendungstatusCmd(ISTAModule callingModule, string methodName, string path, IModuleExecutionParent globalTabModuleISTA, int elementNo)
            : base(callingModule, methodName, path, globalTabModuleISTA, elementNo)
        {
        }

        public override void CreateDialog(ParameterContainer inParam, ParameterContainer inoutParam)
        {
            Log.Info("DialogZuendungstatusCmd.CreateDialog()", "Dialog_Zuendungstatus init started.");
            base.Display = false;
        }

        public override void DoInvoke(string method, ParameterContainer inParam, ParameterContainer outParam, ParameterContainer inoutParam)
        {
            if (base.CallingModule == null)
            {
                Log.Error("DialogZuendungstatusCmd.DoInvoke()", "Failed to invoke method {0}, because calling module is null.", method);
                return;
            }
            Log.Info("DialogZuendungstatusCmd.DoInvoke()", "Dialog_Zuendungstatus");
            try
            {
                ModuleParameter value = base.CallingModule.__RheinGoldCoreModuleParameters__.Clone();
                inParam.Parameter.Add("__RheinGoldCoreModuleParameters__", value);
                inParam.Parameter.Add("__RheinGoldTabModuleISTA__", base.CallingModule.GlobalTabModuleISTA);
                inParam.Parameter.Add("__RheinGoldSOCAccessor__", base.CallingModule.SOCAccessor);
                bool i_automatic = (bool)inParam.getParameter("i_automatic", true);
                bool i_PopUp = (bool)inParam.getParameter("i_PopUp", false);
                string i_hilfsvariable = inParam.getParameter("i_hilfsvariable", string.Empty) as string;
                short i_KL15spg = 0;
                Dialog_Zuendungstatus dialog_Zuendungstatus = new Dialog_Zuendungstatus(inParam);
                if (method == "ZuendungEin")
                {
                    ITextLocator i_ZuendungEinText = inParam.getParameter("i_ZuendungEinText", dialog_Zuendungstatus.__Text("61002196747")) as ITextLocator;
                    dialog_Zuendungstatus.ZuendungEin(i_automatic, i_PopUp, i_ZuendungEinText, i_hilfsvariable, ref i_KL15spg);
                }
                else if (method == "ZuendungAus")
                {
                    ITextLocator i_ZuendungAusText = inParam.getParameter("i_ZuendungAusText", dialog_Zuendungstatus.__Text("61002350091")) as ITextLocator;
                    dialog_Zuendungstatus.ZuendungAus(i_automatic, i_PopUp, i_ZuendungAusText, i_hilfsvariable, ref i_KL15spg);
                }
                else
                {
                    Log.Error("DialogZuendungstatusCmd.DoInvoke()", "Unsupported method {0} will be ignored.", method);
                }
                outParam.setParameter("i_KL15spg", i_KL15spg);
            }
            catch (Exception exception)
            {
                Log.WarningException("DialogZuendungstatusCmd.DoInvoke()", exception);
            }
        }
    }
}
