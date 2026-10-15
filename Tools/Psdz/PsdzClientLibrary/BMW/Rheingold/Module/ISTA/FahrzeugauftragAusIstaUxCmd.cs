using System;
using BMW.Rheingold.CoreFramework;

namespace BMW.Rheingold.Module.ISTA
{
    internal class FahrzeugauftragAusIstaUxCmd : ServiceDialogCmdBase
    {
        public FahrzeugauftragAusIstaUxCmd(ISTAModule callingModule, string methodName, string path, IModuleExecutionParent globalTabModuleISTA, int elementNo) : base(callingModule, methodName, path, globalTabModuleISTA, elementNo)
        {
        }

        public override void CreateDialog(ParameterContainer inParam, ParameterContainer inoutParam)
        {
            Log.Info("FahrzeugauftragAusIstaUxCmd.CreateDialog()", "Fahrzeugauftrag_ausISTA_UX init started.");
            Display = false;
        }

        public override void DoInvoke(string method, ParameterContainer inParam, ParameterContainer outParam, ParameterContainer inoutParam)
        {
            string[] Sonderausstattungen = null;
            if (CallingModule == null)
            {
                Log.Error("Fahrzeugauftrag_ausISTA_UXCmd.DoInvoke()", "Failed to invoke method {0}, because calling module is null.", method);
                return;
            }

            Log.Info("Fahrzeugauftrag_ausISTA_UXCmd.DoInvoke()", "FS_LISTE_ISTA_KURZ");
            try
            {
                ModuleParameter value = CallingModule.__RheinGoldCoreModuleParameters__.Clone();
                inParam.Parameter.Add("__RheinGoldCoreModuleParameters__", value);
                inParam.Parameter.Add("__RheinGoldTabModuleISTA__", CallingModule.GlobalTabModuleISTA);
                inParam.Parameter.Add("__RheinGoldSOCAccessor__", CallingModule.SOCAccessor);
                new Fahrzeugauftrag_ausISTA_UX(inParam).Sonderausstattung(ref Sonderausstattungen);
            }
            catch (Exception exception)
            {
                Log.WarningException("Fahrzeugauftrag_ausISTA_UXCmd.DoInvoke()", exception);
            }

            inoutParam.setParameter("Sonderausstattungen", Sonderausstattungen);
        }
    }
}
