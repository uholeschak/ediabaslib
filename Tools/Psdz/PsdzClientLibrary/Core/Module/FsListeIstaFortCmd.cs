using BMW.Rheingold.CoreFramework;
using BMW.Rheingold.Module.ISTA;
using PsdzClient.Core;
using PsdzClient.Core.Container;

namespace BMW.Rheingold.Module.ISTA
{
    internal class FsListeIstaFortCmd : ServiceDialogCmdBase
    {
        public FsListeIstaFortCmd(ISTAModule callingModule, string methodName, string path, IModuleExecutionParent globalTabModuleISTA, int elementNo)
            : base(callingModule, methodName, path, globalTabModuleISTA, elementNo)
        {
        }

        public override void CreateDialog(ParameterContainer inParam, ParameterContainer inoutParam)
        {
            Log.Info("FsListeIstaFortCmd.CreateDialog()", "FS_LISTE_ISTA_FORT init started.");
            base.Display = false;
        }

        public override void DoInvoke(string method, ParameterContainer inParam, ParameterContainer outParam, ParameterContainer inoutParam)
        {
            if (base.CallingModule == null)
            {
                Log.Error("FsListeIstaFortCmd.DoInvoke()", "Failed to invoke method {0}, because calling module is null.", method);
                return;
            }
            ModuleParameter value = base.CallingModule.__RheinGoldCoreModuleParameters__.Clone();
            inParam.Parameter.Add("__RheinGoldCoreModuleParameters__", value);
            inParam.Parameter.Add("__RheinGoldTabModuleISTA__", base.CallingModule.GlobalTabModuleISTA);
            inParam.Parameter.Add("__RheinGoldSOCAccessor__", base.CallingModule.SOCAccessor);
            if (method == "Details_zum_Fehlerort")
            {
                int fehlerort = (int)inParam.getParameter("Fehlerort", 0);
                int[] umweltbedingung_NR = (int[])inParam.getParameter("Umweltbedingung_NR", null);
                int Umweltbedingung_Anzahl = 0;
                double[,] Umweltbedingung_Wert = null;
                string[,] Umweltbedingung_String = null;
                string Fehlerkode_SGBD = null;
                int Fehlerart_Symptom_NR = 0;
                string Fehlerart_Symptom_Text = null;
                int Fehlerart_Vorhanden_NR = 0;
                string Fehlerart_Vorhanden_Text = null;
                int Fehlerart_Ready_NR = 0;
                string Fehlerart_Ready_Text = null;
                int Fehlerart_Warnung_NR = 0;
                string Fehlerart_Warnung_Text = null;
                int Fehlerart_Erweitert_Anzahl = 0;
                int[] Fehlerart_Erweitert_NR = null;
                string[] Fehlerart_Erweitert_Text = null;
                int Kilometer_Anfang = -1;
                int Kilometer_Ende = -1;
                int Fehlerklasse = 0;
                int Fehlerkode_Ereignis = 0;
                int Fehlerkode_HFK = 0;
                int Fehlerkode_HLZ = 0;
                int Fehlerkode_Ueberlauf = 0;
                int Systemzeit_Anfang = 0;
                int Systemzeit_Ende = 0;
                new FS_LISTE_ISTA_FORT(inParam).Details_zum_Fehlerort(fehlerort, umweltbedingung_NR, ref Umweltbedingung_Anzahl, ref Umweltbedingung_Wert, ref Umweltbedingung_String, ref Fehlerkode_SGBD, ref Fehlerart_Symptom_NR, ref Fehlerart_Symptom_Text, ref Fehlerart_Vorhanden_NR, ref Fehlerart_Vorhanden_Text, ref Fehlerart_Ready_NR, ref Fehlerart_Ready_Text, ref Fehlerart_Warnung_NR, ref Fehlerart_Warnung_Text, ref Fehlerart_Erweitert_Anzahl, ref Fehlerart_Erweitert_NR, ref Fehlerart_Erweitert_Text, ref Kilometer_Anfang, ref Kilometer_Ende, ref Fehlerklasse, ref Fehlerkode_Ereignis, ref Fehlerkode_HFK, ref Fehlerkode_HLZ, ref Fehlerkode_Ueberlauf, ref Systemzeit_Anfang, ref Systemzeit_Ende);
                outParam.setParameter("Umweltbedingung_Anzahl", Umweltbedingung_Anzahl);
                outParam.setParameter("Umweltbedingung_Wert", Umweltbedingung_Wert);
                outParam.setParameter("Umweltbedingung_String", Umweltbedingung_String);
                outParam.setParameter("Fehlerkode_SGBD", Fehlerkode_SGBD);
                outParam.setParameter("Fehlerart_Symptom_NR", Fehlerart_Symptom_NR);
                outParam.setParameter("Fehlerart_Symptom_Text", Fehlerart_Symptom_Text);
                outParam.setParameter("Fehlerart_Vorhanden_NR", Fehlerart_Vorhanden_NR);
                outParam.setParameter("Fehlerart_Vorhanden_Text", Fehlerart_Vorhanden_Text);
                outParam.setParameter("Fehlerart_Ready_NR", Fehlerart_Ready_NR);
                outParam.setParameter("Fehlerart_Ready_Text", Fehlerart_Ready_Text);
                outParam.setParameter("Fehlerart_Warnung_NR", Fehlerart_Warnung_NR);
                outParam.setParameter("Fehlerart_Warnung_Text", Fehlerart_Warnung_Text);
                outParam.setParameter("Fehlerart_Erweitert_Anzahl", Fehlerart_Erweitert_Anzahl);
                outParam.setParameter("Fehlerart_Erweitert_NR", Fehlerart_Erweitert_NR);
                outParam.setParameter("Fehlerart_Erweitert_Text", Fehlerart_Erweitert_Text);
                outParam.setParameter("Kilometer_Anfang", Kilometer_Anfang);
                outParam.setParameter("Kilometer_Ende", Kilometer_Ende);
                outParam.setParameter("Fehlerklasse", Fehlerklasse);
                outParam.setParameter("Fehlerkode_Ereignis", Fehlerkode_Ereignis);
                outParam.setParameter("Fehlerkode_HFK", Fehlerkode_HFK);
                outParam.setParameter("Fehlerkode_HLZ", Fehlerkode_HLZ);
                outParam.setParameter("Fehlerkode_Ueberlauf", Fehlerkode_Ueberlauf);
                outParam.setParameter("Systemzeit_Anfang", Systemzeit_Anfang);
                outParam.setParameter("Systemzeit_Ende", Systemzeit_Ende);
            }
            else
            {
                Log.Error("FsListeIstaFortCmd.DoInvoke()", "Unsupported method {0} will be ignored.", method);
            }
        }
    }
}
