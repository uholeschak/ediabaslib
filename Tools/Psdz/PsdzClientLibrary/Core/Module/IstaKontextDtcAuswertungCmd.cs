using BMW.Rheingold.CoreFramework;
using BMW.Rheingold.Module.ISTA;
using PsdzClient.Core;
using PsdzClient.Core.Container;
using System.Collections.Generic;

namespace BMW.Rheingold.Module.ISTA
{
    internal class IstaKontextDtcAuswertungCmd : ServiceDialogCmdBase
    {
        public IstaKontextDtcAuswertungCmd(ISTAModule callingModule, string methodName, string path, IModuleExecutionParent globalTabModuleISTA, int elementNo)
            : base(callingModule, methodName, path, globalTabModuleISTA, elementNo)
        {
        }

        public override void CreateDialog(ParameterContainer inParam, ParameterContainer inoutParam)
        {
            Log.Info("IstaKontextDtcAuswertungCmd.CreateDialog()", "ISTA_Kontext_DTC_Auswertung init started.");
            base.Display = false;
        }

        public override void DoInvoke(string method, ParameterContainer inParam, ParameterContainer outParam, ParameterContainer inoutParam)
        {
            if (base.CallingModule == null)
            {
                Log.Error("IstaKontextDtcAuswertungCmd.Invoke()", "Failed to invoke method {0}, because calling module is null.", method);
                return;
            }
            ModuleParameter value = base.CallingModule.__RheinGoldCoreModuleParameters__.Clone();
            inParam.Parameter.Add("__RheinGoldCoreModuleParameters__", value);
            inParam.Parameter.Add("__RheinGoldTabModuleISTA__", base.CallingModule.GlobalTabModuleISTA);
            inParam.Parameter.Add("__RheinGoldSOCAccessor__", base.CallingModule.SOCAccessor);
            if ("DTC_Bereich".Equals(method))
            {
                string f_ORT_NR_HEX_MIN = inParam.getParameter("F_ORT_NR_HEX_MIN", "-1") as string;
                string f_ORT_NR_HEX_MAX = inParam.getParameter("F_ORT_NR_HEX_MAX", "-1") as string;
                bool DTC_Eingetragen_Alle = false;
                int DTC_Eingetragen_Anzahl = 0;
                string DTC_Eingetragen_String = string.Empty;
                List<string> DTC_Eingetragen_Liste_HEX = new List<string>();
                List<int> DTC_Eingetragen_Liste_DEZ = new List<int>();
                new ISTA_Kontext_DTC_Auswertung(inParam).DTC_Bereich(f_ORT_NR_HEX_MIN, f_ORT_NR_HEX_MAX, ref DTC_Eingetragen_Alle, ref DTC_Eingetragen_Anzahl, ref DTC_Eingetragen_String, ref DTC_Eingetragen_Liste_HEX, ref DTC_Eingetragen_Liste_DEZ);
                outParam.setParameter("DTC_Eingetragen_Alle", DTC_Eingetragen_Alle);
                outParam.setParameter("DTC_Eingetragen_Anzahl", DTC_Eingetragen_Anzahl);
                outParam.setParameter("DTC_Eingetragen_String", DTC_Eingetragen_String);
                outParam.setParameter("DTC_Eingetragen_Liste_HEX", DTC_Eingetragen_Liste_HEX);
                outParam.setParameter("DTC_Eingetragen_Liste_DEZ", DTC_Eingetragen_Liste_DEZ);
            }
            else if ("DTC_Einzeln".Equals(method))
            {
                string f_ORT_NR_HEX = inParam.getParameter("F_ORT_NR_HEX", string.Empty) as string;
                bool DTC_Eingetragen = false;
                new ISTA_Kontext_DTC_Auswertung(inParam).DTC_Einzeln(f_ORT_NR_HEX, ref DTC_Eingetragen);
                outParam.setParameter("DTC_Eingetragen", DTC_Eingetragen);
            }
            else if ("DTC_Liste".Equals(method))
            {
                List<string> f_ORT_NR_HEX_LISTE = inParam.getParameter("F_ORT_NR_HEX_LISTE", new List<string>()) as List<string>;
                bool DTC_Eingetragen_Alle2 = false;
                int DTC_Eingetragen_Anzahl2 = 0;
                string DTC_Eingetragen_String2 = string.Empty;
                List<string> DTC_Eingetragen_Liste_HEX2 = new List<string>();
                List<int> DTC_Eingetragen_Liste_DEZ2 = new List<int>();
                new ISTA_Kontext_DTC_Auswertung(inParam).DTC_Liste(f_ORT_NR_HEX_LISTE, ref DTC_Eingetragen_Alle2, ref DTC_Eingetragen_Anzahl2, ref DTC_Eingetragen_String2, ref DTC_Eingetragen_Liste_HEX2, ref DTC_Eingetragen_Liste_DEZ2);
                outParam.setParameter("DTC_Eingetragen_Alle", DTC_Eingetragen_Alle2);
                outParam.setParameter("DTC_Eingetragen_Anzahl", DTC_Eingetragen_Anzahl2);
                outParam.setParameter("DTC_Eingetragen_String", DTC_Eingetragen_String2);
                outParam.setParameter("DTC_Eingetragen_Liste_HEX", DTC_Eingetragen_Liste_HEX2);
                outParam.setParameter("DTC_Eingetragen_Liste_DEZ", DTC_Eingetragen_Liste_DEZ2);
            }
            else
            {
                if (!"Sammelfehler_Einzeln".Equals(method))
                {
                    throw new ServiceDialogMethodUnsupportedException();
                }
                string kode = inParam.getParameter("Kode", null) as string;
                bool Kode_Eingetragen = false;
                new ISTA_Kontext_DTC_Auswertung(inParam).Sammelfehler_Einzeln(kode, ref Kode_Eingetragen);
                outParam.setParameter("Kode_Eingetragen", Kode_Eingetragen);
            }
        }
    }
}
