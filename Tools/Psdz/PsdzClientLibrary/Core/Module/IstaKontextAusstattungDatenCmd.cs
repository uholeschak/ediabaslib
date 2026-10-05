using BMW.Rheingold.CoreFramework;
using BMW.Rheingold.Module.ISTA;
using PsdzClient.Core;
using PsdzClient.Core.Container;
using System.Collections.Generic;

namespace BMW.Rheingold.Module.ISTA
{
    internal class IstaKontextAusstattungDatenCmd : ServiceDialogCmdBase
    {
        public IstaKontextAusstattungDatenCmd(ISTAModule callingModule, string methodName, string path, IModuleExecutionParent globalTabModuleISTA, int elementNo) : base(callingModule, methodName, path, globalTabModuleISTA, elementNo)
        {
        }

        public override void CreateDialog(ParameterContainer inParam, ParameterContainer inoutParam)
        {
            Log.Info("IstaKontextAusstattungDatenCmd.CreatePage()", "ISTA_Kontext_Ausstattung_Daten init started.");
            Display = false;
        }

        public override void DoInvoke(string method, ParameterContainer inParam, ParameterContainer outParam, ParameterContainer inoutParam)
        {
            if (CallingModule == null)
            {
                Log.Error("IstaKontextAusstattungDatenCmd.DoInvoke()", "Failed to invoke method {0}, because calling module is null.", method);
                return;
            }

            ModuleParameter value = CallingModule.__RheinGoldCoreModuleParameters__.Clone();
            inParam.Parameter.Add("__RheinGoldCoreModuleParameters__", value);
            inParam.Parameter.Add("__RheinGoldTabModuleISTA__", CallingModule.GlobalTabModuleISTA);
            inParam.Parameter.Add("__RheinGoldSOCAccessor__", CallingModule.SOCAccessor);
            if ("SA_Liste".Equals(method))
            {
                List<string> SAs = new List<string>();
                int SA_Anzahl = 0;
                new ISTA_Kontext_Ausstattung_Daten(inParam).SA_Liste(ref SAs, ref SA_Anzahl);
                outParam.setParameter("SAs", SAs);
                outParam.setParameter("SA_Anzahl", SA_Anzahl);
            }
            else if ("E_Worte".Equals(method))
            {
                List<string> EWorte = new List<string>();
                int EWort_Anzahl = 0;
                new ISTA_Kontext_Ausstattung_Daten(inParam).E_Worte(ref EWorte, ref EWort_Anzahl);
                outParam.setParameter("EWorte", EWorte);
                outParam.setParameter("EWort_Anzahl", EWort_Anzahl);
            }
            else if ("K_Worte".Equals(method))
            {
                List<string> KWorte = new List<string>();
                int KWort_Anzahl = 0;
                new ISTA_Kontext_Ausstattung_Daten(inParam).K_Worte(ref KWorte, ref KWort_Anzahl);
                outParam.setParameter("EWorte", KWorte);
                outParam.setParameter("EWort_Anzahl", KWort_Anzahl);
            }
            else
            {
                Log.Error("IstaKontextAusstattungDatenCmd.Invoke()", "Unsupported method {0} will be ignored.", method);
            }
        }
    }
}