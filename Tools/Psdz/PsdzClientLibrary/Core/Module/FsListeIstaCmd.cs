using BMW.Rheingold.CoreFramework;
using BMW.Rheingold.Module.ISTA;
using PsdzClient.Core;
using PsdzClient.Core.Container;

namespace BMW.Rheingold.Module.ISTA
{
    internal class FsListeIstaCmd : ServiceDialogCmdBase
    {
        public FsListeIstaCmd(ISTAModule callingModule, string methodName, string path, IModuleExecutionParent globalTabModuleISTA, int elementNo) : base(callingModule, methodName, path, globalTabModuleISTA, elementNo)
        {
        }

        public override void CreateDialog(ParameterContainer inParam, ParameterContainer inoutParam)
        {
            Log.Info("FsListeIstaKurzCmd.CreateDialog()", $"{ServiceDialogConfig.Name} init started.");
            Display = false;
        }

        public override void DoInvoke(string method, ParameterContainer inParam, ParameterContainer outParam, ParameterContainer inoutParam)
        {
            if (CallingModule == null)
            {
                Log.Error("FsListeIstaCmd.DoInvoke()", "Failed to invoke method {0}, because calling module is null.", method);
                return;
            }

            Log.Info("FsListeIstaCmd.DoInvoke()", "FS_LISTE_ISTA");
            ModuleParameter value = CallingModule.__RheinGoldCoreModuleParameters__.Clone();
            inParam.Parameter.Add("__RheinGoldCoreModuleParameters__", value);
            inParam.Parameter.Add("__RheinGoldTabModuleISTA__", CallingModule.GlobalTabModuleISTA);
            inParam.Parameter.Add("__RheinGoldSOCAccessor__", CallingModule.SOCAccessor);
            if ("InitializeDialog".Equals(method))
            {
                int num = 300;
                int num2 = 8;
                int num3 = 15;
                int num4 = 2;
                int num5 = num * num2;
                int num6 = num * num3 * num4;
                string[] array = null;
                string[] array2 = null;
                string[] array3 = null;
                string[] array4 = null;
                string[] array5 = null;
                string[] array6 = null;
                string[] array7 = null;
                string[] array8 = null;
                int[] array9 = null;
                int[] array10 = null;
                int[] array11 = null;
                int[] array12 = null;
                int[] array13 = null;
                int[] array14 = null;
                int[] array15 = null;
                int[] array16 = null;
                int[] array17 = null;
                int[] array18 = null;
                int[] array19 = null;
                int num7 = 0;
                int[] array20 = null;
                int[] array21 = null;
                int[] array22 = null;
                double[] array23 = null;
                num7 = 0;
                array2 = new string[num];
                array15 = new int[num];
                array = new string[num];
                array3 = new string[num];
                array16 = new int[num];
                array10 = new int[num];
                array4 = new string[num];
                array9 = new int[num];
                array5 = new string[num];
                array11 = new int[num];
                array6 = new string[num];
                array12 = new int[num];
                array7 = new string[num];
                array13 = new int[num];
                array14 = new int[num];
                array17 = new int[num];
                array18 = new int[num];
                array19 = new int[num];
                array22 = new int[num];
                for (int i = 0; i < num; i++)
                {
                    array2[i] = string.Empty;
                    array15[i] = 0;
                    array[i] = string.Empty;
                    array3[i] = string.Empty;
                    array16[i] = 0;
                    array10[i] = 0;
                    array4[i] = string.Empty;
                    array9[i] = 0;
                    array5[i] = string.Empty;
                    array11[i] = 0;
                    array6[i] = string.Empty;
                    array12[i] = 0;
                    array7[i] = string.Empty;
                    array13[i] = 0;
                    array14[i] = 0;
                    array17[i] = 0;
                    array18[i] = 0;
                    array19[i] = 255;
                    array22[i] = 0;
                }

                array20 = new int[num5];
                array8 = new string[num5];
                for (int j = 0; j < num5; j++)
                {
                    array20[j] = 0;
                    array8[j] = string.Empty;
                }

                array21 = new int[num6];
                array23 = new double[num6];
                for (int k = 0; k < num6; k++)
                {
                    array21[k] = 0;
                    array23[k] = 0.0;
                }

                new FS_LISTE_ISTA(inParam).InitializeDialog(ref array2, ref num7, ref array15, ref array, ref array3, ref array10, ref array4, ref array9, ref array5, ref array11, ref array6, ref array12, ref array7, ref array20, ref array8, ref array21, ref array23, ref array17, ref array18, ref array19, ref array16, ref array13, ref array14, ref array22);
                inoutParam.setParameter("Fehlerkode_Text", array2);
                inoutParam.setParameter("Anzahl_Fehlerspeicher", num7);
                inoutParam.setParameter("Fehlerkode_dez", array15);
                inoutParam.setParameter("Fehlerkode_hex", array);
                inoutParam.setParameter("Fehlerkode_SGBD", array3);
                inoutParam.setParameter("Fehlerart_Symptom_NR", array10);
                inoutParam.setParameter("Fehlerart_Symptom_Text", array4);
                inoutParam.setParameter("Fehlerart_Vorhanden_NR", array9);
                inoutParam.setParameter("Fehlerart_Vorhanden_Text", array5);
                inoutParam.setParameter("Fehlerart_Ready_NR", array11);
                inoutParam.setParameter("Fehlerart_Ready_Text", array6);
                inoutParam.setParameter("Fehlerart_Warnung_NR", array12);
                inoutParam.setParameter("Fehlerart_Warnung_Text", array7);
                inoutParam.setParameter("Fehlerart_Erweitert_NR", array20);
                inoutParam.setParameter("Fehlerart_Erweitert_Text", array8);
                inoutParam.setParameter("Umweltbedingung_NR", array21);
                inoutParam.setParameter("Umweltbedingung_Wert", array23);
                inoutParam.setParameter("Kilometer_Anfang", array17);
                inoutParam.setParameter("Kilometer_Ende", array18);
                inoutParam.setParameter("Fehlerklasse", array19);
                inoutParam.setParameter("Fehlerkode_Ereignis", array16);
                inoutParam.setParameter("Fehlerkode_HFK", array13);
                inoutParam.setParameter("Fehlerkode_HLZ", array14);
                inoutParam.setParameter("Umweltbedingung_Anzahl", array22);
            }
            else
            {
                if (!"StartFsListeLesenGesamt".Equals(method))
                {
                    throw new ServiceDialogMethodUnsupportedException();
                }

                int Anzahl_Fehlerspeicher = (int)inoutParam.getParameter("Anzahl_Fehlerspeicher");
                int[] Fehlerkode_dez = (int[])inoutParam.getParameter("Fehlerkode_dez");
                string[] Fehlerkode_hex = (string[])inoutParam.getParameter("Fehlerkode_hex");
                string[] Fehlerkode_Text = (string[])inoutParam.getParameter("Fehlerkode_Text");
                string[] Fehlerkode_SGBD = (string[])inoutParam.getParameter("Fehlerkode_SGBD");
                int[] Fehlerkode_Ereignis = (int[])inoutParam.getParameter("Fehlerkode_Ereignis");
                int[] Fehlerart_Symptom_NR = (int[])inoutParam.getParameter("Fehlerart_Symptom_NR");
                string[] Fehlerart_Symptom_Text = (string[])inoutParam.getParameter("Fehlerart_Symptom_Text");
                int[] Fehlerart_Vorhanden_NR = (int[])inoutParam.getParameter("Fehlerart_Vorhanden_NR");
                string[] Fehlerart_Vorhanden_Text = (string[])inoutParam.getParameter("Fehlerart_Vorhanden_Text");
                int[] Fehlerart_Ready_NR = (int[])inoutParam.getParameter("Fehlerart_Ready_NR");
                string[] Fehlerart_Ready_Text = (string[])inoutParam.getParameter("Fehlerart_Ready_Text");
                int[] Fehlerart_Warnung_NR = (int[])inoutParam.getParameter("Fehlerart_Warnung_NR");
                string[] Fehlerart_Warnung_Text = (string[])inoutParam.getParameter("Fehlerart_Warnung_Text");
                int[] Fehlerkode_HFK = (int[])inoutParam.getParameter("Fehlerkode_HFK");
                int[] Fehlerkode_HLZ = (int[])inoutParam.getParameter("Fehlerkode_HLZ");
                int[] Kilometer_Anfang = (int[])inoutParam.getParameter("Kilometer_Anfang");
                int[] Kilometer_Ende = (int[])inoutParam.getParameter("Kilometer_Ende");
                int[] Fehlerklasse = (int[])inoutParam.getParameter("Fehlerklasse");
                int[] Fehlerkode_Ueberlauf = (int[])inoutParam.getParameter("Fehlerkode_Ueberlauf");
                int[] Systemzeit_Anfang = (int[])inoutParam.getParameter("Systemzeit_Anfang");
                int[] Systemzeit_Ende = (int[])inoutParam.getParameter("Systemzeit_Ende");
                int Fehlerart_Erweitert_Anzahl = (int)inoutParam.getParameter("Fehlerart_Erweitert_Anzahl");
                int[] Fehlerart_Erweitert_NR = (int[])inoutParam.getParameter("Fehlerart_Erweitert_NR");
                string[] Fehlerart_Erweitert_Text = (string[])inoutParam.getParameter("Fehlerart_Erweitert_Text");
                int[] Umweltbedingung_Anzahl = (int[])inoutParam.getParameter("Umweltbedingung_Anzahl");
                int[] Umweltbedingung_NR = (int[])inoutParam.getParameter("Umweltbedingung_NR");
                double[] Umweltbedingung_Wert = (double[])inoutParam.getParameter("Umweltbedingung_Wert");
                new FS_LISTE_ISTA(inParam).StartFsListeLesenGesamt(ref Anzahl_Fehlerspeicher, ref Fehlerkode_dez, ref Fehlerkode_hex, ref Fehlerkode_Text, ref Fehlerkode_SGBD, ref Fehlerkode_Ereignis, ref Fehlerart_Symptom_NR, ref Fehlerart_Symptom_Text, ref Fehlerart_Vorhanden_NR, ref Fehlerart_Vorhanden_Text, ref Fehlerart_Ready_NR, ref Fehlerart_Ready_Text, ref Fehlerart_Warnung_NR, ref Fehlerart_Warnung_Text, ref Fehlerkode_HFK, ref Fehlerkode_HLZ, ref Kilometer_Anfang, ref Kilometer_Ende, ref Fehlerklasse, ref Fehlerkode_Ueberlauf, ref Systemzeit_Anfang, ref Systemzeit_Ende, ref Fehlerart_Erweitert_Anzahl, ref Fehlerart_Erweitert_NR, ref Fehlerart_Erweitert_Text, ref Umweltbedingung_Anzahl, ref Umweltbedingung_NR, ref Umweltbedingung_Wert);
                inoutParam.setParameter("Anzahl_Fehlerspeicher", Anzahl_Fehlerspeicher);
                inoutParam.setParameter("Fehlerkode_dez", Fehlerkode_dez);
                inoutParam.setParameter("Fehlerkode_hex", Fehlerkode_hex);
                inoutParam.setParameter("Fehlerkode_Text", Fehlerkode_Text);
                inoutParam.setParameter("Fehlerkode_SGBD", Fehlerkode_SGBD);
                inoutParam.setParameter("Fehlerkode_Ereignis", Fehlerkode_Ereignis);
                inoutParam.setParameter("Fehlerart_Symptom_NR", Fehlerart_Symptom_NR);
                inoutParam.setParameter("Fehlerart_Symptom_Text", Fehlerart_Symptom_Text);
                inoutParam.setParameter("Fehlerart_Vorhanden_NR", Fehlerart_Vorhanden_NR);
                inoutParam.setParameter("Fehlerart_Vorhanden_Text", Fehlerart_Vorhanden_Text);
                inoutParam.setParameter("Fehlerart_Ready_NR", Fehlerart_Ready_NR);
                inoutParam.setParameter("Fehlerart_Ready_Text", Fehlerart_Ready_Text);
                inoutParam.setParameter("Fehlerart_Warnung_NR", Fehlerart_Warnung_NR);
                inoutParam.setParameter("Fehlerart_Warnung_Text", Fehlerart_Warnung_Text);
                inoutParam.setParameter("Fehlerkode_HFK", Fehlerkode_HFK);
                inoutParam.setParameter("Fehlerkode_HLZ", Fehlerkode_HLZ);
                inoutParam.setParameter("Kilometer_Anfang", Kilometer_Anfang);
                inoutParam.setParameter("Kilometer_Ende", Kilometer_Ende);
                inoutParam.setParameter("Fehlerklasse", Fehlerklasse);
                inoutParam.setParameter("Fehlerkode_Ueberlauf", Fehlerkode_Ueberlauf);
                inoutParam.setParameter("Systemzeit_Anfang", Systemzeit_Anfang);
                inoutParam.setParameter("Systemzeit_Ende", Systemzeit_Ende);
                inoutParam.setParameter("Fehlerart_Erweitert_Anzahl", Fehlerart_Erweitert_Anzahl);
                inoutParam.setParameter("Fehlerart_Erweitert_NR", Fehlerart_Erweitert_NR);
                inoutParam.setParameter("Fehlerart_Erweitert_Text", Fehlerart_Erweitert_Text);
                inoutParam.setParameter("Umweltbedingung_Anzahl", Umweltbedingung_Anzahl);
                inoutParam.setParameter("Umweltbedingung_NR", Umweltbedingung_NR);
                inoutParam.setParameter("Umweltbedingung_Wert", Umweltbedingung_Wert);
            }
        }
    }
}