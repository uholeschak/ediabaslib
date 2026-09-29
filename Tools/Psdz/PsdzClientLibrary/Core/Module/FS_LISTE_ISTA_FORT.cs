using BMW.Rheingold.CoreFramework;
using BMW.Rheingold.CoreFramework.DatabaseProvider;
using BMW.Rheingold.Module.ISTA;
using PsdzClient.Core;
using PsdzClient.Core.Container;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace BMW.Rheingold.Module.ISTA
{
    internal class FS_LISTE_ISTA_FORT : ISTAModule
    {
        public int m_AnzahlSaetze;

        public int m_maxAnzahlFehlerarten;

        public int m_maxAnzahlUmweltbedingungen;

        public bool p_WeiterButtonEnabledStack;

        public bool bWriteLog;

        public string strDlgInfo;

        public FS_LISTE_ISTA_FORT(ParameterContainer InParameter)
        {
            if (InParameter != null)
            {
                _globalModuleInParameter = InParameter;
            }
            __handleInParameter();
            m_AnzahlSaetze = 2;
            m_maxAnzahlFehlerarten = 8;
            m_maxAnzahlUmweltbedingungen = 100;
            p_WeiterButtonEnabledStack = false;
            bWriteLog = true;
            strDlgInfo = "V1.0, 25.11.2010-FS_LISTE_ISTA_FORT";
        }

        public virtual void Prepare()
        {
        }

        public virtual void Reset()
        {
        }

        private double? GetUmweltbedingungWert(F_UW uw)
        {
            if (uw == null)
            {
                return null;
            }
            try
            {
                if ("hex".Equals(uw.F_UW_EINH, StringComparison.OrdinalIgnoreCase))
                {
                    return Convert.ToInt64(uw.F_UW_WERT.ToString(), 16);
                }
                return Convert.ToDouble(uw.F_UW_WERT, CultureInfo.InvariantCulture);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private int IndexOfNR(int uwNR, ICollection<F_UW> uwArray)
        {
            int num = 0;
            if (uwArray != null)
            {
                foreach (F_UW item in uwArray)
                {
                    if (item.F_UW_NR == uwNR)
                    {
                        return num;
                    }
                    num++;
                }
            }
            return -1;
        }

        public virtual void Details_zum_Fehlerort(int Fehlerort, int[] Umweltbedingung_NR, ref int Umweltbedingung_Anzahl, ref double[,] Umweltbedingung_Wert, ref string[,] Umweltbedingung_String, ref string Fehlerkode_SGBD, ref int Fehlerart_Symptom_NR, ref string Fehlerart_Symptom_Text, ref int Fehlerart_Vorhanden_NR, ref string Fehlerart_Vorhanden_Text, ref int Fehlerart_Ready_NR, ref string Fehlerart_Ready_Text, ref int Fehlerart_Warnung_NR, ref string Fehlerart_Warnung_Text, ref int Fehlerart_Erweitert_Anzahl, ref int[] Fehlerart_Erweitert_NR, ref string[] Fehlerart_Erweitert_Text, ref int Kilometer_Anfang, ref int Kilometer_Ende, ref int Fehlerklasse, ref int Fehlerkode_Ereignis, ref int Fehlerkode_HFK, ref int Fehlerkode_HLZ, ref int Fehlerkode_Ueberlauf, ref int Systemzeit_Anfang, ref int Systemzeit_Ende)
        {
            int num = 0;
            Logger.WriteInformation("Details_zum_Fehlerortcalled");
            if (bWriteLog)
            {
                Logger.WriteInformation($"Details_zum_Fehlerort - {strDlgInfo}");
            }
            int[] array = new int[100];
            if (Umweltbedingung_NR == null)
            {
                Umweltbedingung_NR = array;
            }
            Umweltbedingung_Anzahl = 0;
            Umweltbedingung_Wert = (double[,])IstaModuleBase.__initArray<double>(new int[2] { m_maxAnzahlUmweltbedingungen, m_maxAnzahlUmweltbedingungen }, 0.0);
            Umweltbedingung_String = (string[,])IstaModuleBase.__initArray<string>(new int[2] { m_maxAnzahlUmweltbedingungen, m_maxAnzahlUmweltbedingungen }, "");
            Fehlerkode_SGBD = "";
            Fehlerart_Symptom_NR = 255;
            Fehlerart_Symptom_Text = "";
            Fehlerart_Vorhanden_NR = 255;
            Fehlerart_Vorhanden_Text = "";
            Fehlerart_Ready_NR = 255;
            Fehlerart_Ready_Text = "";
            Fehlerart_Warnung_NR = 255;
            Fehlerart_Warnung_Text = "";
            Fehlerart_Erweitert_Anzahl = 0;
            Fehlerart_Erweitert_NR = (int[])IstaModuleBase.__initArray<int>(new int[1] { m_maxAnzahlFehlerarten }, 255);
            Fehlerart_Erweitert_Text = (string[])IstaModuleBase.__initArray<string>(new int[1] { m_maxAnzahlFehlerarten }, "");
            Kilometer_Anfang = 0;
            Kilometer_Ende = 0;
            Fehlerklasse = 255;
            Fehlerkode_Ereignis = 0;
            Fehlerkode_HFK = 0;
            Fehlerkode_HLZ = 0;
            Fehlerkode_Ueberlauf = 0;
            Systemzeit_Anfang = 0;
            Systemzeit_Ende = 0;
            base._DoLoopHandling = true;
            for (int i = 0; i < m_maxAnzahlUmweltbedingungen && Umweltbedingung_NR[i] == 0; i++)
            {
            }
            base._DoLoopHandling = false;
            ECU eCU = null;
            DTC dTC = null;
            foreach (ECU item in Vehicle.ECU)
            {
                foreach (DTC item2 in item.FEHLER)
                {
                    if (item2.F_ORT == Fehlerort && item2.Relevance == true)
                    {
                        eCU = item;
                        dTC = item2;
                        break;
                    }
                }
                if (dTC != null && eCU != null)
                {
                    break;
                }
            }
            if (dTC == null || eCU == null)
            {
                Log.Info("FS_LISTE_ISTA_FORT.Details_zum_Fehlerort()", "no relevant ecu/dtc combination found in session context regarding to Fehlerort: {0}", Fehlerort);
                return;
            }
            Fehlerkode_SGBD = eCU.VARIANTE;
            Fehlerart_Symptom_NR = (dTC.F_SYMPTOM_NR.HasValue ? dTC.F_SYMPTOM_NR.Value : 0);
            Fehlerart_Symptom_Text = dTC.F_SYMPTOM_TEXT;
            Fehlerart_Vorhanden_NR = (dTC.F_VORHANDEN_NR.HasValue ? dTC.F_VORHANDEN_NR.Value : 0);
            Fehlerart_Vorhanden_Text = ((dTC.F_VORHANDEN_TEXT != null) ? dTC.F_VORHANDEN_TEXT : "");
            Fehlerart_Ready_NR = (dTC.F_READY_NR.HasValue ? dTC.F_READY_NR.Value : 0);
            Fehlerart_Ready_Text = ((dTC.F_READY_TEXT != null) ? dTC.F_READY_TEXT : "");
            Fehlerart_Warnung_NR = (dTC.F_WARNUNG_NR.HasValue ? dTC.F_WARNUNG_NR.Value : 0);
            Fehlerart_Warnung_Text = ((dTC.F_WARNUNG_TEXT != null) ? dTC.F_WARNUNG_TEXT : "");
            Fehlerart_Erweitert_Anzahl = 0;
            int num2;
            if (dTC != null)
            {
                typeDTCContext first = dTC.First;
                if (first != null && first.F_UW_KM_SUPREME.HasValue)
                {
                    num2 = (int)dTC.First.F_UW_KM_SUPREME.Value;
                    goto IL_03c2;
                }
            }
            if (dTC != null)
            {
                typeDTCContext first2 = dTC.First;
                if (first2 != null && first2.F_UW_KM.HasValue)
                {
                    num2 = (int)dTC.First.F_UW_KM.Value;
                    goto IL_03c2;
                }
            }
            num2 = -1;
            goto IL_03c2;
            IL_04a6:
            int num3;
            Systemzeit_Anfang = num3;
            int num4;
            if (dTC != null)
            {
                typeDTCContext current3 = dTC.Current;
                if (current3 != null && current3.F_UW_ZEIT_SUPREME.HasValue)
                {
                    num4 = (int)dTC.Current.F_UW_ZEIT_SUPREME.Value;
                    goto IL_0518;
                }
            }
            if (dTC != null)
            {
                typeDTCContext current4 = dTC.Current;
                if (current4 != null && current4.F_UW_ZEIT.HasValue)
                {
                    num4 = (int)dTC.Current.F_UW_ZEIT.Value;
                    goto IL_0518;
                }
            }
            num4 = 0;
            goto IL_0518;
            IL_03c2:
            Kilometer_Anfang = num2;
            int num5;
            if (dTC != null)
            {
                typeDTCContext current5 = dTC.Current;
                if (current5 != null && current5.F_UW_KM_SUPREME.HasValue)
                {
                    num5 = (int)dTC.Current.F_UW_KM_SUPREME.Value;
                    goto IL_0434;
                }
            }
            if (dTC != null)
            {
                typeDTCContext current6 = dTC.Current;
                if (current6 != null && current6.F_UW_KM.HasValue)
                {
                    num5 = (int)dTC.Current.F_UW_KM.Value;
                    goto IL_0434;
                }
            }
            num5 = -1;
            goto IL_0434;
            IL_0434:
            Kilometer_Ende = num5;
            if (dTC != null)
            {
                typeDTCContext first3 = dTC.First;
                if (first3 != null && first3.F_UW_ZEIT_SUPREME.HasValue)
                {
                    num3 = (int)dTC.First.F_UW_ZEIT_SUPREME.Value;
                    goto IL_04a6;
                }
            }
            if (dTC != null)
            {
                typeDTCContext first4 = dTC.First;
                if (first4 != null && first4.F_UW_ZEIT.HasValue)
                {
                    num3 = (int)dTC.First.F_UW_ZEIT.Value;
                    goto IL_04a6;
                }
            }
            num3 = 0;
            goto IL_04a6;
            IL_0518:
            Systemzeit_Ende = num4;
            Fehlerkode_HFK = (int)(dTC.F_HFK.HasValue ? dTC.F_HFK.Value : 0);
            Fehlerkode_HLZ = (int)(dTC.F_HLZ.HasValue ? dTC.F_HLZ.Value : 0);
            Fehlerkode_Ereignis = (dTC.F_EREIGNIS_DTC.HasValue ? dTC.F_EREIGNIS_DTC.Value : 0);
            Fehlerkode_Ueberlauf = (int)(dTC.F_UEBERLAUF.HasValue ? dTC.F_UEBERLAUF.Value : 0);
            Fehlerklasse = (dTC.F_FEHLERKLASSE_NR.HasValue ? dTC.F_FEHLERKLASSE_NR.Value : 0);
            Umweltbedingung_Anzahl = 0;
            int num6 = 0;
            int[] array2 = Umweltbedingung_NR;
            foreach (int uwNR in array2)
            {
                try
                {
                    int num7 = IndexOfNR(uwNR, dTC?.Current?.F_UW);
                    if (num7 > -1 && num7 < dTC?.Current?.F_UW.Count && num7 < dTC?.First?.F_UW.Count)
                    {
                        F_UW f_UW = dTC?.First?.F_UW[num7];
                        F_UW f_UW2 = dTC?.Current?.F_UW[num7];
                        double? umweltbedingungWert = GetUmweltbedingungWert(f_UW);
                        double? umweltbedingungWert2 = GetUmweltbedingungWert(f_UW2);
                        Umweltbedingung_Wert[num6, 0] = (umweltbedingungWert.HasValue ? umweltbedingungWert.Value : 0.0);
                        Umweltbedingung_Wert[num6, 1] = (umweltbedingungWert2.HasValue ? umweltbedingungWert2.Value : 0.0);
                        Umweltbedingung_String[num6, 0] = ((f_UW == null) ? string.Empty : f_UW?.F_UW_TEXT);
                        Umweltbedingung_String[num6, 1] = ((f_UW2 == null) ? string.Empty : f_UW2?.F_UW_TEXT);
                    }
                    num6 = (Umweltbedingung_Anzahl = num6 + 1);
                }
                catch (Exception exception)
                {
                    Log.WarningException("FS_LISTE_ISTA_FORT.Details_zum_FehlerortRG()", exception);
                }
            }
            if (ConfigSettings.getConfigStringAsBoolean("BMW.Rheingold.Module.FS_LISTE_ISTA_FORT.Logging", defaultValue: false))
            {
                try
                {
                    StringBuilder stringBuilder = new StringBuilder();
                    StringBuilder stringBuilder2 = new StringBuilder();
                    stringBuilder.Append("- Fehlerort[hex]\n");
                    stringBuilder2.Append($"{Fehlerort:X}" + "\n");
                    stringBuilder.Append("- Fehlerart_Symptom\n");
                    stringBuilder2.Append(Fehlerart_Symptom_NR + "\n");
                    stringBuilder.Append("- Fehlerart_Vorhanden\n");
                    stringBuilder2.Append(Fehlerart_Vorhanden_NR + "\n");
                    stringBuilder.Append("- Kilometer_Anfang\n");
                    stringBuilder2.Append(Kilometer_Anfang + "\n");
                    stringBuilder.Append("- Kilometer_Ende\n");
                    stringBuilder2.Append(Kilometer_Ende + "\n");
                    stringBuilder.Append("- Fehlerkode_HFK\n");
                    stringBuilder2.Append(Fehlerkode_HFK + "\n");
                    stringBuilder.Append("- Fehlerkode_HLZ\n");
                    stringBuilder2.Append(Fehlerkode_HLZ + "\n");
                    for (int k = 0; k < Math.Min(m_maxAnzahlUmweltbedingungen, Umweltbedingung_NR.Length); k++)
                    {
                        if (Umweltbedingung_NR[k] > 0)
                        {
                            stringBuilder.Append("- Umweltbedingung[" + k + "]:" + Umweltbedingung_NR[k] + "\n");
                            if (Umweltbedingung_String[k, 0].Length == 0)
                            {
                                stringBuilder2.Append(Umweltbedingung_String[k, 0] + "," + Umweltbedingung_String[k, 1] + "\n");
                            }
                            else
                            {
                                stringBuilder2.Append(Umweltbedingung_Wert[k, 0] + "," + Umweltbedingung_Wert[k, 1] + "\n");
                            }
                        }
                    }
                    LogStatement("Test_Message", "Time", DateTime.UtcNow, "Dialog", "FS_LISTE_ISTA_FORT", stringBuilder.ToString(), stringBuilder2.ToString());
                }
                catch (Exception exception2)
                {
                    Log.WarningException("FS_LISTE_ISTA_FORT.Details_zum_Fehlerort()", exception2);
                }
            }
            Logger.WriteInformation("_ExitIndex is: {0}", num);
        }

        public virtual void Details_zum_FehlerortRG(int Fehlerort, int[] Umweltbedingung_NR, ref int Umweltbedingung_Anzahl, ref double[,] Umweltbedingung_Wert, ref string[,] Umweltbedingung_String, ref string Fehlerkode_SGBD, ref int Fehlerart_Symptom_NR, ref string Fehlerart_Symptom_Text, ref int Fehlerart_Vorhanden_NR, ref string Fehlerart_Vorhanden_Text, ref int Fehlerart_Ready_NR, ref string Fehlerart_Ready_Text, ref int Fehlerart_Warnung_NR, ref string Fehlerart_Warnung_Text, ref int Fehlerart_Erweitert_Anzahl, ref int[] Fehlerart_Erweitert_NR, ref string[] Fehlerart_Erweitert_Text, ref int Kilometer_Anfang, ref int Kilometer_Ende, ref int Fehlerklasse, ref int Fehlerkode_Ereignis, ref int Fehlerkode_HFK, ref int Fehlerkode_HLZ, ref int Fehlerkode_Ueberlauf, ref int Systemzeit_Anfang, ref int Systemzeit_Ende)
        {
            int num = 0;
            Logger.WriteInformation("Details_zum_Fehlerortcalled");
            if (bWriteLog)
            {
                Logger.WriteInformation($"Details_zum_Fehlerort - {strDlgInfo}");
            }
            int[] array = new int[100];
            if (Umweltbedingung_NR == null)
            {
                Umweltbedingung_NR = array;
            }
            Umweltbedingung_Anzahl = 0;
            Umweltbedingung_Wert = (double[,])IstaModuleBase.__initArray<double>(new int[2] { m_maxAnzahlUmweltbedingungen, m_maxAnzahlUmweltbedingungen }, 0.0);
            Umweltbedingung_String = (string[,])IstaModuleBase.__initArray<string>(new int[2] { m_maxAnzahlUmweltbedingungen, m_maxAnzahlUmweltbedingungen }, "");
            Fehlerkode_SGBD = "";
            Fehlerart_Symptom_NR = 255;
            Fehlerart_Symptom_Text = "";
            Fehlerart_Vorhanden_NR = 255;
            Fehlerart_Vorhanden_Text = "";
            Fehlerart_Ready_NR = 255;
            Fehlerart_Ready_Text = "";
            Fehlerart_Warnung_NR = 255;
            Fehlerart_Warnung_Text = "";
            Fehlerart_Erweitert_Anzahl = 0;
            Fehlerart_Erweitert_NR = (int[])IstaModuleBase.__initArray<int>(new int[1] { m_maxAnzahlFehlerarten }, 255);
            Fehlerart_Erweitert_Text = (string[])IstaModuleBase.__initArray<string>(new int[1] { m_maxAnzahlFehlerarten }, "");
            Kilometer_Anfang = 0;
            Kilometer_Ende = 0;
            Fehlerklasse = 255;
            Fehlerkode_Ereignis = 0;
            Fehlerkode_HFK = 0;
            Fehlerkode_HLZ = 0;
            Fehlerkode_Ueberlauf = 0;
            Systemzeit_Anfang = 0;
            Systemzeit_Ende = 0;
            base._DoLoopHandling = true;
            for (int i = 0; i < m_maxAnzahlUmweltbedingungen && Umweltbedingung_NR[i] == 0; i++)
            {
            }
            base._DoLoopHandling = false;
            foreach (ECU item in Vehicle.ECU)
            {
                foreach (DTC item2 in item.FEHLER)
                {
                    try
                    {
                        if (item2.Relevance != true || item2.F_ORT != Fehlerort || item2.IsVirtual)
                        {
                            continue;
                        }
                        Fehlerklasse = (item2.F_FEHLERKLASSE_NR.HasValue ? item2.F_FEHLERKLASSE_NR.Value : 255);
                        Fehlerkode_Ereignis = (item2.F_EREIGNIS_DTC.HasValue ? item2.F_EREIGNIS_DTC.Value : 0);
                        Fehlerkode_HFK = (int)(item2.F_HFK.HasValue ? item2.F_HFK.Value : 0);
                        Fehlerkode_HLZ = (int)(item2.F_HLZ.HasValue ? item2.F_HLZ.Value : 0);
                        Fehlerkode_Ueberlauf = (int)(item2.F_UEBERLAUF.HasValue ? item2.F_UEBERLAUF.Value : 0);
                        Kilometer_Anfang = (int)item2.F_UW_KM_Min;
                        Kilometer_Ende = (int)item2.F_UW_KM.Value;
                        Fehlerkode_SGBD = ((!string.IsNullOrEmpty(item.VARIANTE)) ? item.VARIANTE : item.ECU_GRUPPE);
                        Fehlerart_Symptom_NR = (item2.F_SYMPTOM_NR.HasValue ? item2.F_SYMPTOM_NR.Value : 255);
                        Fehlerart_Symptom_Text = item2.F_SYMPTOM_TEXT;
                        Fehlerart_Vorhanden_NR = (item2.F_VORHANDEN_NR.HasValue ? item2.F_VORHANDEN_NR.Value : 255);
                        Fehlerart_Vorhanden_Text = item2.F_VORHANDEN_TEXT;
                        Fehlerart_Ready_NR = (item2.F_READY_NR.HasValue ? item2.F_READY_NR.Value : 255);
                        Fehlerart_Ready_Text = item2.F_READY_TEXT;
                        Fehlerart_Warnung_NR = (item2.F_WARNUNG_NR.HasValue ? item2.F_WARNUNG_NR.Value : 255);
                        Fehlerart_Warnung_Text = item2.F_WARNUNG_TEXT;
                        Fehlerart_Erweitert_Anzahl = 0;
                        Umweltbedingung_Anzahl = 0;
                        if (item2.DTCContext != null)
                        {
                            foreach (typeDTCContext item3 in item2.DTCContext)
                            {
                                int num2 = 0;
                                foreach (F_UW item4 in item3.F_UW)
                                {
                                    try
                                    {
                                        Umweltbedingung_Wert[Umweltbedingung_Anzahl, num2] = Convert.ToDouble(item4.F_UW_WERT);
                                    }
                                    catch (Exception exception)
                                    {
                                        Log.WarningException("FS_LISTE_ISTA_FORT.Details_zum_FehlerortRG()", exception);
                                    }
                                    Umweltbedingung_String[Umweltbedingung_Anzahl, num2] = item4.F_UW_TEXT;
                                    num2++;
                                }
                            }
                            Umweltbedingung_Anzahl++;
                        }
                        Systemzeit_Anfang = (int)((item2.First != null && item2.First.F_UW_ZEIT_SUPREME.HasValue) ? ((int)item2.First.F_UW_ZEIT_SUPREME.Value) : ((item2.First != null && item2.First.F_UW_ZEIT.HasValue) ? item2.First.F_UW_ZEIT.Value : (-1)));
                        Systemzeit_Ende = (int)((item2.Current != null && item2.Current.F_UW_ZEIT_SUPREME.HasValue) ? ((int)item2.Current.F_UW_ZEIT_SUPREME.Value) : ((item2.Current != null && item2.Current.F_UW_ZEIT.HasValue) ? item2.Current.F_UW_ZEIT.Value : (-1)));
                        Kilometer_Anfang = (int)((item2.First != null && item2.First.F_UW_KM_SUPREME.HasValue) ? ((int)item2.First.F_UW_KM_SUPREME.Value) : ((item2.First != null && item2.First.F_UW_KM.HasValue) ? item2.First.F_UW_KM.Value : (-1)));
                        Kilometer_Ende = (int)((item2.Current != null && item2.Current.F_UW_KM_SUPREME.HasValue) ? ((int)item2.Current.F_UW_KM_SUPREME.Value) : ((item2.Current != null && item2.Current.F_UW_KM.HasValue) ? item2.Current.F_UW_KM.Value : (-1)));
                    }
                    catch (Exception exception2)
                    {
                        Log.WarningException("FS_LISTE_ISTA_FORT.Details_zum_FehlerortRG()", exception2);
                    }
                }
            }
            Fehlerkode_SGBD.ToUpper();
            try
            {
                StringBuilder stringBuilder = new StringBuilder();
                StringBuilder stringBuilder2 = new StringBuilder();
                stringBuilder.Append("- Fehlerort[hex]\n");
                stringBuilder2.Append($"{Fehlerort:X}" + "\n");
                stringBuilder.Append("- Fehlerart_Symptom\n");
                stringBuilder2.Append(Fehlerart_Symptom_NR + "\n");
                stringBuilder.Append("- Fehlerart_Vorhanden\n");
                stringBuilder2.Append(Fehlerart_Vorhanden_NR + "\n");
                stringBuilder.Append("- Kilometer_Anfang\n");
                stringBuilder2.Append(Kilometer_Anfang + "\n");
                stringBuilder.Append("- Kilometer_Ende\n");
                stringBuilder2.Append(Kilometer_Ende + "\n");
                stringBuilder.Append("- Fehlerkode_HFK\n");
                stringBuilder2.Append(Fehlerkode_HFK + "\n");
                stringBuilder.Append("- Fehlerkode_HLZ\n");
                stringBuilder2.Append(Fehlerkode_HLZ + "\n");
                for (int j = 0; j < Math.Min(m_maxAnzahlUmweltbedingungen, Umweltbedingung_NR.Length); j++)
                {
                    if (Umweltbedingung_NR[j] > 0)
                    {
                        stringBuilder.Append("- Umweltbedingung[" + j + "]:" + Umweltbedingung_NR[j] + "\n");
                        if (Umweltbedingung_String[j, 0].Length == 0)
                        {
                            stringBuilder2.Append(Umweltbedingung_String[j, 0] + "," + Umweltbedingung_String[j, 1] + "\n");
                        }
                        else
                        {
                            stringBuilder2.Append(Umweltbedingung_Wert[j, 0] + "," + Umweltbedingung_Wert[j, 1] + "\n");
                        }
                    }
                }
                Trace.TraceInformation("Test_Message", "\"\"", "DateTime.UtcNow", "\"FS_LISTE_ISTA_FORT\"", "varProtokollMessageText.ToString()", "varProtokollAntwortText.ToString()");
            }
            catch (Exception)
            {
            }
            Logger.WriteInformation("_ExitIndex is: {0}", num);
        }
    }
}
