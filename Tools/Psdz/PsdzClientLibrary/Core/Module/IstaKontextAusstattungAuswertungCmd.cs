using BMW.Rheingold.CoreFramework;
using BMW.Rheingold.Module.ISTA;
using PsdzClient.Core;
using PsdzClient.Core.Container;
using System.Collections.Generic;

namespace BMW.Rheingold.Module.ISTA
{
    internal class IstaKontextAusstattungAuswertungCmd : ServiceDialogCmdBase
    {
        public IstaKontextAusstattungAuswertungCmd(ISTAModule callingModule, string methodName, string path, IModuleExecutionParent globalTabModuleISTA, int elementNo) : base(callingModule, methodName, path, globalTabModuleISTA, elementNo)
        {
        }

        public override void CreateDialog(ParameterContainer inParam, ParameterContainer inoutParam)
        {
            Log.Info("IstaKontextAusstattungAuswertungCmd.CreateDialog()", "ISTA_Kontext_Ausstattung_Auswertung init started.");
            Display = false;
        }

        public override void DoInvoke(string method, ParameterContainer inParam, ParameterContainer outParam, ParameterContainer inoutParam)
        {
            if (CallingModule == null)
            {
                Log.Error("IstaKontextAusstattungAuswertungCmd.Invoke()", "Failed to invoke method {0}, because calling module is null.", method);
                return;
            }

            ModuleParameter value = CallingModule.__RheinGoldCoreModuleParameters__.Clone();
            inParam.Parameter.Add("__RheinGoldCoreModuleParameters__", value);
            inParam.Parameter.Add("__RheinGoldTabModuleISTA__", CallingModule.GlobalTabModuleISTA);
            inParam.Parameter.Add("__RheinGoldSOCAccessor__", CallingModule.SOCAccessor);
            if ("SA_Liste".Equals(method))
            {
                List<string> sA_LISTE = inParam.getParameter("SA_LISTE", new List<string>()) as List<string>;
                bool SA_Vorhanden_Alle = false;
                int SA_Vorhanden_Anzahl = 0;
                string SA_Vorhanden_String = string.Empty;
                List<string> SA_Vorhanden_Liste = new List<string>();
                new ISTA_Kontext_Ausstattung_Auswertung(inParam).SA_Liste(sA_LISTE, ref SA_Vorhanden_Alle, ref SA_Vorhanden_Anzahl, ref SA_Vorhanden_String, ref SA_Vorhanden_Liste);
                outParam.setParameter("SA_Vorhanden_Alle", SA_Vorhanden_Alle);
                outParam.setParameter("SA_Vorhanden_Anzahl", SA_Vorhanden_Anzahl);
                outParam.setParameter("SA_Vorhanden_String", SA_Vorhanden_String);
                outParam.setParameter("SA_Vorhanden_Liste", SA_Vorhanden_Liste);
            }
            else
            {
                if (!"SA_Einzeln".Equals(method))
                {
                    throw new ServiceDialogMethodUnsupportedException();
                }

                string sA = inParam.getParameter("SA", null) as string;
                bool SA_Vorhanden = false;
                new ISTA_Kontext_Ausstattung_Auswertung(inParam).SA_Einzeln(sA, ref SA_Vorhanden);
                outParam.setParameter("SA_Vorhanden", SA_Vorhanden);
            }
        }
    }
}