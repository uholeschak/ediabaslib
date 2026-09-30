using BMW.Rheingold.CoreFramework;
using BMW.Rheingold.CoreFramework.DatabaseProvider;
using BMW.Rheingold.ISTA.CoreFramework.SOCAccessor;
using BMW.Rheingold.Module.ISTA;
using PsdzClient.Core;
using PsdzClient.Core.Container;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using PsdzClient;

namespace BMW.Rheingold.Module.ISTA
{
    internal class FS_LISTE_ISTA : ISTAModule
    {
        public string strDlgInfo;
        public bool bWriteLog;
        public int m_maxAnzahlFehlerkode;
        public int m_maxAnzahlFehlerarten;
        public int m_maxAnzahlUmweltbedingungen;
        public int m_AnzahlSaetze;
        public int m_maxArrayFehlerarten;
        public int m_maxArrayUmweltbedingungen;
        public bool p_WeiterButtonEnabledStack;
        public FS_LISTE_ISTA(ParameterContainer InParameter)
        {
            if (InParameter != null)
            {
                _globalModuleInParameter = InParameter;
            }

            __handleInParameter();
            strDlgInfo = "16.04.2010-FS_LISTE_ISTA";
            bWriteLog = true;
            m_maxAnzahlFehlerkode = 300;
            m_maxAnzahlFehlerarten = 8;
            m_maxAnzahlUmweltbedingungen = 15;
            m_AnzahlSaetze = 2;
            m_maxArrayFehlerarten = m_maxAnzahlFehlerkode * m_maxAnzahlFehlerarten;
            m_maxArrayUmweltbedingungen = m_maxAnzahlFehlerkode * m_maxAnzahlUmweltbedingungen * m_AnzahlSaetze;
            p_WeiterButtonEnabledStack = false;
        }

        public virtual void Prepare()
        {
        }

        public virtual void Reset()
        {
        }

        public virtual void InitializeDialog(ref string[] Fehlerkode_Text, ref int Anzahl_Fehlerspeicher, ref int[] Fehlerkode_dez, ref string[] Fehlerkode_hex, ref string[] Fehlerkode_SGBD, ref int[] Fehlerart_Symptom_NR, ref string[] Fehlerart_Symptom_Text, ref int[] Fehlerart_Vorhanden_NR, ref string[] Fehlerart_Vorhanden_Text, ref int[] Fehlerart_Ready_NR, ref string[] Fehlerart_Ready_Text, ref int[] Fehlerart_Warnung_NR, ref string[] Fehlerart_Warnung_Text, ref int[] Fehlerart_Erweitert_NR, ref string[] Fehlerart_Erweitert_Text, ref int[] Umweltbedingung_NR, ref double[] Umweltbedingung_Wert, ref int[] Kilometer_Anfang, ref int[] Kilometer_Ende, ref int[] Fehlerklasse, ref int[] Fehlerkode_Ereignis, ref int[] Fehlerkode_HFK, ref int[] Fehlerkode_HLZ, ref int[] Umweltbedingung_Anzahl)
        {
            int num = 0;
            Logger.WriteInformation("InitializeDialogcalled");
            if (bWriteLog)
            {
                Logger.WriteInformation($"IntializeDialog - {strDlgInfo}");
            }

            int Fehlerart_Erweitert_Anzahl = 0;
            int[] array = new int[m_maxAnzahlFehlerkode];
            int[] array2 = new int[m_maxAnzahlFehlerkode];
            int[] array3 = new int[m_maxAnzahlFehlerkode];
            array = new int[m_maxAnzahlFehlerkode];
            array2 = new int[m_maxAnzahlFehlerkode];
            array3 = new int[m_maxAnzahlFehlerkode];
            base._DoLoopHandling = true;
            for (int i = 0; i < m_maxAnzahlFehlerkode; i++)
            {
                array[i] = 0;
                array2[i] = 0;
                array3[i] = 0;
            }

            base._DoLoopHandling = false;
            ReadingIstaListeRG(ref Anzahl_Fehlerspeicher, ref Fehlerkode_dez, ref Fehlerkode_hex, ref Fehlerkode_Text, ref Fehlerkode_Ereignis, ref Fehlerkode_SGBD, ref Fehlerart_Symptom_NR, ref Fehlerart_Symptom_Text, ref Fehlerart_Vorhanden_NR, ref Fehlerart_Vorhanden_Text, ref Fehlerart_Ready_NR, ref Fehlerart_Ready_Text, ref Fehlerart_Warnung_NR, ref Fehlerart_Warnung_Text, ref Fehlerart_Erweitert_Anzahl, ref Fehlerart_Erweitert_NR, ref Fehlerart_Erweitert_Text, ref Fehlerkode_HFK, ref Fehlerkode_HLZ, ref array, ref Kilometer_Anfang, ref Kilometer_Ende, ref array2, ref array3, ref Umweltbedingung_Anzahl, ref Umweltbedingung_NR, ref Umweltbedingung_Wert, ref Fehlerklasse);
            Logger.WriteInformation("_ExitIndex is: {0}", num);
        }

        public virtual void StartFsListeLesenGesamt(ref int Anzahl_Fehlerspeicher, ref int[] Fehlerkode_dez, ref string[] Fehlerkode_hex, ref string[] Fehlerkode_Text, ref string[] Fehlerkode_SGBD, ref int[] Fehlerkode_Ereignis, ref int[] Fehlerart_Symptom_NR, ref string[] Fehlerart_Symptom_Text, ref int[] Fehlerart_Vorhanden_NR, ref string[] Fehlerart_Vorhanden_Text, ref int[] Fehlerart_Ready_NR, ref string[] Fehlerart_Ready_Text, ref int[] Fehlerart_Warnung_NR, ref string[] Fehlerart_Warnung_Text, ref int[] Fehlerkode_HFK, ref int[] Fehlerkode_HLZ, ref int[] Kilometer_Anfang, ref int[] Kilometer_Ende, ref int[] Fehlerklasse, ref int[] Fehlerkode_Ueberlauf, ref int[] Systemzeit_Anfang, ref int[] Systemzeit_Ende, ref int Fehlerart_Erweitert_Anzahl, ref int[] Fehlerart_Erweitert_NR, ref string[] Fehlerart_Erweitert_Text, ref int[] Umweltbedingung_Anzahl, ref int[] Umweltbedingung_NR, ref double[] Umweltbedingung_Wert)
        {
            int num = 0;
            Logger.WriteInformation("StartFsListeLesenGesamtcalled");
            if (bWriteLog)
            {
                Logger.WriteInformation($"StartFsListeLesenGesamt - {strDlgInfo}");
            }

            ReadingIstaListe(ref Anzahl_Fehlerspeicher, ref Fehlerkode_dez, ref Fehlerkode_hex, ref Fehlerkode_Text, ref Fehlerkode_Ereignis, ref Fehlerkode_SGBD, ref Fehlerart_Symptom_NR, ref Fehlerart_Symptom_Text, ref Fehlerart_Vorhanden_NR, ref Fehlerart_Vorhanden_Text, ref Fehlerart_Ready_NR, ref Fehlerart_Ready_Text, ref Fehlerart_Warnung_NR, ref Fehlerart_Warnung_Text, ref Fehlerart_Erweitert_Anzahl, ref Fehlerart_Erweitert_NR, ref Fehlerart_Erweitert_Text, ref Fehlerkode_HFK, ref Fehlerkode_HLZ, ref Fehlerkode_Ueberlauf, ref Kilometer_Anfang, ref Kilometer_Ende, ref Systemzeit_Anfang, ref Systemzeit_Ende, ref Umweltbedingung_Anzahl, ref Umweltbedingung_NR, ref Umweltbedingung_Wert, ref Fehlerklasse);
            Logger.WriteInformation("_ExitIndex is: {0}", num);
        }

        public virtual void ReadingIstaListe(ref int Anzahl_Fehlerspeicher, ref int[] Fehlerkode_dez, ref string[] Fehlerkode_hex, ref string[] Fehlerkode_Text, ref int[] Fehlerkode_Ereignis, ref string[] Fehlerkode_SGBD, ref int[] Fehlerart_Symptom_NR, ref string[] Fehlerart_Symptom_Text, ref int[] Fehlerart_Vorhanden_NR, ref string[] Fehlerart_Vorhanden_Text, ref int[] Fehlerart_Ready_NR, ref string[] Fehlerart_Ready_Text, ref int[] Fehlerart_Warnung_NR, ref string[] Fehlerart_Warnung_Text, ref int Fehlerart_Erweitert_Anzahl, ref int[] Fehlerart_Erweitert_NR, ref string[] Fehlerart_Erweitert_Text, ref int[] Fehlerkode_HFK, ref int[] Fehlerkode_HLZ, ref int[] Fehlerkode_Ueberlauf, ref int[] Kilometer_Anfang, ref int[] Kilometer_Ende, ref int[] Systemzeit_Anfang, ref int[] Systemzeit_Ende, ref int[] Umweltbedingung_Anzahl, ref int[] Umweltbedingung_NR, ref double[] Umweltbedingung_Wert, ref int[] Fehlerklasse)
        {
            int num = 0;
            Logger.WriteInformation("ReadingIstaListecalled");
            if (bWriteLog)
            {
                Logger.WriteInformation(strDlgInfo);
            }

            Anzahl_Fehlerspeicher = 0;
            Fehlerkode_Text = new string[m_maxAnzahlFehlerkode];
            Fehlerkode_dez = new int[m_maxAnzahlFehlerkode];
            Fehlerkode_hex = new string[m_maxAnzahlFehlerkode];
            Fehlerkode_SGBD = new string[m_maxAnzahlFehlerkode];
            Fehlerkode_Ereignis = new int[m_maxAnzahlFehlerkode];
            Fehlerart_Symptom_NR = new int[m_maxAnzahlFehlerkode];
            Fehlerart_Symptom_Text = new string[m_maxAnzahlFehlerkode];
            Fehlerart_Vorhanden_NR = new int[m_maxAnzahlFehlerkode];
            Fehlerart_Vorhanden_Text = new string[m_maxAnzahlFehlerkode];
            Fehlerart_Ready_NR = new int[m_maxAnzahlFehlerkode];
            Fehlerart_Ready_Text = new string[m_maxAnzahlFehlerkode];
            Fehlerart_Warnung_NR = new int[m_maxAnzahlFehlerkode];
            Fehlerart_Warnung_Text = new string[m_maxAnzahlFehlerkode];
            Fehlerkode_HFK = new int[m_maxAnzahlFehlerkode];
            Fehlerkode_HLZ = new int[m_maxAnzahlFehlerkode];
            Kilometer_Anfang = new int[m_maxAnzahlFehlerkode];
            Kilometer_Ende = new int[m_maxAnzahlFehlerkode];
            Fehlerklasse = new int[m_maxAnzahlFehlerkode];
            Umweltbedingung_Anzahl = new int[m_maxAnzahlFehlerkode];
            Fehlerkode_Ueberlauf = new int[m_maxAnzahlFehlerkode];
            Systemzeit_Anfang = new int[m_maxAnzahlFehlerkode];
            Systemzeit_Ende = new int[m_maxAnzahlFehlerkode];
            base._DoLoopHandling = true;
            for (int i = 0; i < m_maxAnzahlFehlerkode; i++)
            {
                Fehlerkode_Text[i] = "";
                Fehlerkode_dez[i] = 0;
                Fehlerkode_hex[i] = "";
                Fehlerkode_SGBD[i] = "";
                Fehlerkode_Ereignis[i] = 0;
                Fehlerart_Symptom_NR[i] = 0;
                Fehlerart_Symptom_Text[i] = "";
                Fehlerart_Vorhanden_NR[i] = 0;
                Fehlerart_Vorhanden_Text[i] = "";
                Fehlerart_Ready_NR[i] = 0;
                Fehlerart_Ready_Text[i] = "";
                Fehlerart_Warnung_NR[i] = 0;
                Fehlerart_Warnung_Text[i] = "";
                Fehlerkode_HFK[i] = 0;
                Fehlerkode_HLZ[i] = 0;
                Kilometer_Anfang[i] = 0;
                Kilometer_Ende[i] = 0;
                Fehlerklasse[i] = 255;
                Umweltbedingung_Anzahl[i] = 0;
                Fehlerkode_Ueberlauf[i] = 0;
                Systemzeit_Anfang[i] = 0;
                Systemzeit_Ende[i] = 0;
            }

            base._DoLoopHandling = false;
            Fehlerart_Erweitert_NR = new int[m_maxArrayFehlerarten];
            Fehlerart_Erweitert_Text = new string[m_maxArrayFehlerarten];
            base._DoLoopHandling = true;
            for (int j = 0; j < m_maxArrayFehlerarten; j++)
            {
                Fehlerart_Erweitert_NR[j] = 0;
                Fehlerart_Erweitert_Text[j] = "";
            }

            base._DoLoopHandling = false;
            Umweltbedingung_NR = new int[m_maxArrayUmweltbedingungen];
            Umweltbedingung_Wert = new double[m_maxArrayUmweltbedingungen];
            base._DoLoopHandling = true;
            for (int k = 0; k < m_maxArrayUmweltbedingungen; k++)
            {
                Umweltbedingung_NR[k] = 0;
                Umweltbedingung_Wert[k] = 0.0;
            }

            base._DoLoopHandling = false;
            List<string> list = new List<string>();
            int num2 = 0;
            if (SOCAccessor.OrderContext.System.GetProperty("FAULT_CODES_LIST")is Hashtable hashtable)
            {
                foreach (Hashtable value in hashtable.Values)
                {
                    if (!value.ContainsKey("ECUJobs"))
                    {
                        continue;
                    }

                    foreach (Hashtable value2 in (value["ECUJobs"] as Hashtable).Values)
                    {
                        if (!value2.ContainsKey("Key") || value2["Key"].ToString().ToLower() == "virtual" || !value2.ContainsKey("ECUFaultCodes"))
                        {
                            continue;
                        }

                        foreach (Hashtable value3 in (value2["ECUFaultCodes"] as Hashtable).Values)
                        {
                            if (!Convert.ToBoolean(value3.ContainsKey("DiagnosisRelevance") ? value3["DiagnosisRelevance"] : ((object)false)))
                            {
                                continue;
                            }

                            string text = "";
                            string text2 = "";
                            string text3 = "";
                            int num3 = 255;
                            string text4 = "";
                            int num4 = 255;
                            string text5 = "";
                            int num5 = 255;
                            string text6 = "";
                            int num6 = 255;
                            string text7 = "";
                            int num7 = Convert.ToInt32(value3.ContainsKey("FaultCode") ? value3["FaultCode"] : "0");
                            int num8 = 0;
                            int num9 = 0;
                            int num10 = 255;
                            int num11 = 0;
                            int num12 = 0;
                            int num13 = 0;
                            int num14 = 0;
                            int num15 = 0;
                            if (num2 >= m_maxAnzahlFehlerkode)
                            {
                                break;
                            }

                            Fehlerkode_dez[num2] = num7;
                            if (value.ContainsKey("Group"))
                            {
                                text = value["Group"].ToString().ToLower();
                                list.Add(text);
                            }

                            if (value.ContainsKey("Variant"))
                            {
                                text2 = value["Variant"].ToString();
                                Fehlerkode_SGBD[num2] = text2;
                            }

                            if (value3.ContainsKey("FaultClasses"))
                            {
                                num10 = 255;
                            }

                            Fehlerklasse[num2] = num10;
                            value3.ContainsKey("FaultCodeTextObject");
                            Fehlerkode_Text[num2] = text3;
                            if (value3.ContainsKey("ECUFaultModes") && text.Length > 0)
                            {
                                foreach (Hashtable value4 in (value3["ECUFaultModes"] as Hashtable).Values)
                                {
                                    int num16 = 255;
                                    string text8 = "";
                                    string text9 = "";
                                    text3 = "";
                                    value4.ContainsKey("FaultModeTextObject");
                                    if (value4.ContainsKey("Names"))
                                    {
                                        List<string> list2 = value4["Names"] as List<string>;
                                        if (list2.Count > 0)
                                        {
                                            text9 = list2[0];
                                        }
                                    }

                                    if (value4.ContainsKey("FaultMode"))
                                    {
                                        List<string> list3 = value4["FaultMode"] as List<string>;
                                        if (list3.Count > 0)
                                        {
                                            num16 = __convertToInt32(list3[0], 10);
                                        }
                                    }

                                    if (text9 == "F_EREIGNIS_DTC")
                                    {
                                        num8 = num16;
                                    }

                                    if (!value4.ContainsKey("Key"))
                                    {
                                        continue;
                                    }

                                    text8 = value4["Key"].ToString();
                                    if (text8 == "Fehlerart")
                                    {
                                        switch (text9)
                                        {
                                            case "F_VORHANDEN_NR":
                                                text5 = text3;
                                                num4 = num16;
                                                break;
                                            case "F_READY_NR":
                                                text6 = text3;
                                                num5 = num16;
                                                break;
                                            case "F_WARNUNG_NR":
                                                text7 = text3;
                                                num6 = num16;
                                                break;
                                            case "F_SYMPTOM_NR":
                                                text4 = text3;
                                                num3 = num16;
                                                break;
                                        }
                                    }
                                    else if (text8 == "Fehlerart[i]" && Fehlerart_Erweitert_Anzahl < m_maxAnzahlFehlerarten)
                                    {
                                        Fehlerart_Erweitert_NR[Fehlerart_Erweitert_Anzahl * 300 + num2] = num16;
                                        Fehlerart_Erweitert_Text[Fehlerart_Erweitert_Anzahl * 300 + num2] = text3;
                                        Fehlerart_Erweitert_Anzahl++;
                                    }
                                }
                            }

                            Fehlerart_Symptom_NR[num2] = num3;
                            Fehlerart_Vorhanden_NR[num2] = num4;
                            Fehlerart_Ready_NR[num2] = num5;
                            Fehlerart_Warnung_NR[num2] = num6;
                            Fehlerart_Symptom_Text[num2] = text4;
                            Fehlerart_Vorhanden_Text[num2] = text5;
                            Fehlerart_Ready_Text[num2] = text6;
                            Fehlerart_Warnung_Text[num2] = text7;
                            int num17 = 0;
                            if (value3.ContainsKey("ECUEnvironmentalConditions"))
                            {
                                foreach (Hashtable value5 in (value3["ECUEnvironmentalConditions"] as Hashtable).Values)
                                {
                                    int num18 = 0;
                                    int num19 = 0;
                                    double num20 = 0.0;
                                    double num21 = 0.0;
                                    int num22 = 0;
                                    string text10 = "";
                                    if (value5.ContainsKey("EnvironmentalCondition"))
                                    {
                                        List<ValueLiteral> list4 = value5["EnvironmentalCondition"] as List<ValueLiteral>;
                                        if (list4.Count > 0)
                                        {
                                            num18 = __convertToInt32(list4[0].Item);
                                            num20 = __convertToDouble(list4[0].Item);
                                            if (list4.Count > 1)
                                            {
                                                num19 = __convertToInt32(list4[list4.Count - 1].Item);
                                                num21 = __convertToDouble(list4[list4.Count - 1].Item);
                                            }
                                        }
                                    }

                                    if (value5.ContainsKey("Index"))
                                    {
                                        num22 = __convertToInt32(value5["Index"]);
                                    }

                                    if (value5.ContainsKey("Names"))
                                    {
                                        List<string> list5 = value5["Names"] as List<string>;
                                        if (list5.Count > 0)
                                        {
                                            text10 = list5[0];
                                        }
                                    }

                                    if (!value5.ContainsKey("Key"))
                                    {
                                        continue;
                                    }

                                    string text11 = value5["Key"].ToString();
                                    bool flag = false;
                                    bool flag2 = false;
                                    if (text11.StartsWith("Umweltbedingung"))
                                    {
                                        flag = true;
                                    }

                                    if (text11.StartsWith("Umweltbedingung[i]"))
                                    {
                                        flag2 = true;
                                    }

                                    if (flag & flag2)
                                    {
                                        flag = false;
                                    }

                                    if (flag)
                                    {
                                        switch (text10)
                                        {
                                            case "F_HFK":
                                                num8 = num18;
                                                break;
                                            case "F_HLZ":
                                                num9 = num18;
                                                break;
                                            case "F_UEBERLAUF":
                                                num13 = num18;
                                                break;
                                            case "F_UW_KM":
                                                num11 = num18;
                                                num12 = num19;
                                                break;
                                            case "F_UW_ZEIT":
                                                num14 = num18;
                                                num15 = num19;
                                                break;
                                        }
                                    }

                                    if (flag2 && num17 < m_maxAnzahlUmweltbedingungen)
                                    {
                                        Umweltbedingung_Wert[num17 * 300 + num2] = num20;
                                        Umweltbedingung_Wert[4500 + num17 * 300 + num2] = num21;
                                        Umweltbedingung_NR[num17 * 300 + num2] = num22;
                                        Umweltbedingung_NR[4500 + num17 * 300 + num2] = num22;
                                        num17++;
                                    }
                                }
                            }

                            if (num8 == 1)
                            {
                                num12 = num11;
                            }

                            Fehlerkode_HFK[num2] = num8;
                            Fehlerkode_HLZ[num2] = num9;
                            Fehlerklasse[num2] = num10;
                            Kilometer_Anfang[num2] = num11;
                            Kilometer_Ende[num2] = num12;
                            Fehlerkode_Ueberlauf[num2] = num13;
                            Systemzeit_Anfang[num2] = num14;
                            Systemzeit_Ende[num2] = num15;
                            Umweltbedingung_Anzahl[num2] = num17;
                            num2++;
                        }
                    }
                }

                Anzahl_Fehlerspeicher = num2;
            }

            base._DoLoopHandling = true;
            for (int l = 0; l < Anzahl_Fehlerspeicher; l++)
            {
                Fehlerkode_hex[l] = $"{Fehlerkode_dez[l]:X}";
                Fehlerkode_SGBD[l] = Fehlerkode_SGBD[l].ToUpper();
            }

            base._DoLoopHandling = false;
            Logger.WriteInformation("_ExitIndex is: {0}", num);
        }

        public virtual void ReadingIstaListeRG(ref int Anzahl_Fehlerspeicher, ref int[] Fehlerkode_dez, ref string[] Fehlerkode_hex, ref string[] Fehlerkode_Text, ref int[] Fehlerkode_Ereignis, ref string[] Fehlerkode_SGBD, ref int[] Fehlerart_Symptom_NR, ref string[] Fehlerart_Symptom_Text, ref int[] Fehlerart_Vorhanden_NR, ref string[] Fehlerart_Vorhanden_Text, ref int[] Fehlerart_Ready_NR, ref string[] Fehlerart_Ready_Text, ref int[] Fehlerart_Warnung_NR, ref string[] Fehlerart_Warnung_Text, ref int Fehlerart_Erweitert_Anzahl, ref int[] Fehlerart_Erweitert_NR, ref string[] Fehlerart_Erweitert_Text, ref int[] Fehlerkode_HFK, ref int[] Fehlerkode_HLZ, ref int[] Fehlerkode_Ueberlauf, ref int[] Kilometer_Anfang, ref int[] Kilometer_Ende, ref int[] Systemzeit_Anfang, ref int[] Systemzeit_Ende, ref int[] Umweltbedingung_Anzahl, ref int[] Umweltbedingung_NR, ref double[] Umweltbedingung_Wert, ref int[] Fehlerklasse)
        {
            if (bWriteLog)
            {
                Logger.WriteInformation(strDlgInfo);
            }

            Anzahl_Fehlerspeicher = 0;
            Fehlerkode_Text = new string[m_maxAnzahlFehlerkode];
            Fehlerkode_dez = new int[m_maxAnzahlFehlerkode];
            Fehlerkode_hex = new string[m_maxAnzahlFehlerkode];
            Fehlerkode_SGBD = new string[m_maxAnzahlFehlerkode];
            Fehlerkode_Ereignis = new int[m_maxAnzahlFehlerkode];
            Fehlerart_Symptom_NR = new int[m_maxAnzahlFehlerkode];
            Fehlerart_Symptom_Text = new string[m_maxAnzahlFehlerkode];
            Fehlerart_Vorhanden_NR = new int[m_maxAnzahlFehlerkode];
            Fehlerart_Vorhanden_Text = new string[m_maxAnzahlFehlerkode];
            Fehlerart_Ready_NR = new int[m_maxAnzahlFehlerkode];
            Fehlerart_Ready_Text = new string[m_maxAnzahlFehlerkode];
            Fehlerart_Warnung_NR = new int[m_maxAnzahlFehlerkode];
            Fehlerart_Warnung_Text = new string[m_maxAnzahlFehlerkode];
            Fehlerkode_HFK = new int[m_maxAnzahlFehlerkode];
            Fehlerkode_HLZ = new int[m_maxAnzahlFehlerkode];
            Kilometer_Anfang = new int[m_maxAnzahlFehlerkode];
            Kilometer_Ende = new int[m_maxAnzahlFehlerkode];
            Fehlerklasse = new int[m_maxAnzahlFehlerkode];
            Umweltbedingung_Anzahl = new int[m_maxAnzahlFehlerkode];
            Fehlerkode_Ueberlauf = new int[m_maxAnzahlFehlerkode];
            Systemzeit_Anfang = new int[m_maxAnzahlFehlerkode];
            Systemzeit_Ende = new int[m_maxAnzahlFehlerkode];
            for (int i = 0; i < m_maxAnzahlFehlerkode; i++)
            {
                Fehlerkode_Text[i] = string.Empty;
                Fehlerkode_dez[i] = 0;
                Fehlerkode_hex[i] = string.Empty;
                Fehlerkode_SGBD[i] = string.Empty;
                Fehlerkode_Ereignis[i] = 0;
                Fehlerart_Symptom_NR[i] = 0;
                Fehlerart_Symptom_Text[i] = string.Empty;
                Fehlerart_Vorhanden_NR[i] = 0;
                Fehlerart_Vorhanden_Text[i] = string.Empty;
                Fehlerart_Ready_NR[i] = 0;
                Fehlerart_Ready_Text[i] = string.Empty;
                Fehlerart_Warnung_NR[i] = 0;
                Fehlerart_Warnung_Text[i] = string.Empty;
                Fehlerkode_HFK[i] = 0;
                Fehlerkode_HLZ[i] = 0;
                Kilometer_Anfang[i] = 0;
                Kilometer_Ende[i] = 0;
                Fehlerklasse[i] = 255;
                Umweltbedingung_Anzahl[i] = 0;
                Fehlerkode_Ueberlauf[i] = 0;
                Systemzeit_Anfang[i] = 0;
                Systemzeit_Ende[i] = 0;
            }

            Fehlerart_Erweitert_NR = new int[m_maxArrayFehlerarten];
            Fehlerart_Erweitert_Text = new string[m_maxArrayFehlerarten];
            for (int j = 0; j < m_maxArrayFehlerarten; j++)
            {
                Fehlerart_Erweitert_NR[j] = 0;
                Fehlerart_Erweitert_Text[j] = string.Empty;
            }

            Umweltbedingung_NR = new int[m_maxArrayUmweltbedingungen];
            Umweltbedingung_Wert = new double[m_maxArrayUmweltbedingungen];
            for (int k = 0; k < m_maxArrayUmweltbedingungen; k++)
            {
                Umweltbedingung_NR[k] = 0;
                Umweltbedingung_Wert[k] = 0.0;
            }

            int num = 0;
            foreach (ECU item in Vehicle.ECU)
            {
                foreach (DTC dtc in item.FEHLER)
                {
                    try
                    {
                        if (dtc.Relevance != true || !(dtc.F_ORT is long))
                        {
                            continue;
                        }

                        if (num >= m_maxAnzahlFehlerkode)
                        {
                            break;
                        }

                        //[-] ICollection<XEP_FAULTMODELABELS> ecuFaultAdditionalLabel = DatabaseProviderFactory.Instance.GetEcuFaultAdditionalLabel(dtc.F_ORT.ToString(), item.VARIANTE);
                        //[+] ICollection<XEP_FAULTMODELABELS> ecuFaultAdditionalLabel = ClientContext.GetDatabase(Vehicle)?.GetEcuFaultAdditionalLabel(dtc.F_ORT.ToString(), item.VARIANTE);
                        ICollection<XEP_FAULTMODELABELS> ecuFaultAdditionalLabel = ClientContext.GetDatabase(Vehicle)?.GetEcuFaultAdditionalLabel(dtc.F_ORT.ToString(), item.VARIANTE);
                        Fehlerkode_dez[num] = (int)(dtc.F_ORT.HasValue ? dtc.F_ORT.Value : 0);
                        Fehlerkode_hex[num] = $"{dtc.F_ORT:X}";
                        Fehlerkode_SGBD[num] = ((!string.IsNullOrEmpty(item.VARIANTE)) ? item.VARIANTE.ToUpper() : item.ECU_GRUPPE);
                        string text = FaultCodeConverters.LocalizedFaultLabel(item, dtc, Vehicle, FFMResolver);
                        Fehlerkode_Text[num] = (string.IsNullOrEmpty(text) ? dtc.F_ORT_TEXT : text);
                        Fehlerklasse[num] = (dtc.F_FEHLERKLASSE_NR.HasValue ? dtc.F_FEHLERKLASSE_NR.Value : 255);
                        Fehlerkode_Ereignis[num] = (dtc.F_EREIGNIS_DTC.HasValue ? dtc.F_EREIGNIS_DTC.Value : 0);
                        Fehlerkode_HFK[num] = (int)(dtc.F_HFK.HasValue ? dtc.F_HFK.Value : 0);
                        Fehlerkode_HLZ[num] = (int)(dtc.F_HLZ.HasValue ? dtc.F_HLZ.Value : 0);
                        Fehlerkode_Ueberlauf[num] = (int)(dtc.F_UEBERLAUF.HasValue ? dtc.F_UEBERLAUF.Value : 0);
                        Kilometer_Anfang[num] = (int)dtc.F_UW_KM_Min;
                        Kilometer_Ende[num] = (int)(dtc.F_UW_KM.HasValue ? dtc.F_UW_KM.Value : (-1));
                        XEP_FAULTMODELABELS xEP_FAULTMODELABELS = null;
                        Fehlerart_Symptom_NR[num] = (dtc.F_SYMPTOM_NR.HasValue ? dtc.F_SYMPTOM_NR.Value : 255);
                        xEP_FAULTMODELABELS = ecuFaultAdditionalLabel.FirstOrDefault((XEP_FAULTMODELABELS x) => Convert.ToInt64(x.Code) == dtc.F_SYMPTOM_NR);
                        Fehlerart_Symptom_Text[num] = ((xEP_FAULTMODELABELS != null) ? xEP_FAULTMODELABELS.Title : dtc.F_SYMPTOM_TEXT);
                        Fehlerart_Vorhanden_NR[num] = (dtc.F_VORHANDEN_NR.HasValue ? dtc.F_VORHANDEN_NR.Value : 255);
                        xEP_FAULTMODELABELS = ecuFaultAdditionalLabel.FirstOrDefault((XEP_FAULTMODELABELS x) => Convert.ToInt64(x.Code) == dtc.F_VORHANDEN_NR);
                        Fehlerart_Vorhanden_Text[num] = ((xEP_FAULTMODELABELS != null) ? xEP_FAULTMODELABELS.Title : dtc.F_VORHANDEN_TEXT);
                        Fehlerart_Ready_NR[num] = (dtc.F_READY_NR.HasValue ? dtc.F_READY_NR.Value : 255);
                        xEP_FAULTMODELABELS = ecuFaultAdditionalLabel.FirstOrDefault((XEP_FAULTMODELABELS x) => Convert.ToInt64(x.Code) == dtc.F_READY_NR);
                        Fehlerart_Ready_Text[num] = ((xEP_FAULTMODELABELS != null) ? xEP_FAULTMODELABELS.Title : dtc.F_READY_TEXT);
                        Fehlerart_Warnung_NR[num] = (dtc.F_WARNUNG_NR.HasValue ? dtc.F_WARNUNG_NR.Value : 255);
                        xEP_FAULTMODELABELS = ecuFaultAdditionalLabel.FirstOrDefault((XEP_FAULTMODELABELS x) => Convert.ToInt64(x.Code) == dtc.F_WARNUNG_NR);
                        Fehlerart_Warnung_Text[num] = ((xEP_FAULTMODELABELS != null) ? xEP_FAULTMODELABELS.Title : dtc.F_WARNUNG_TEXT);
                        Fehlerart_Erweitert_Anzahl = 0;
                        Umweltbedingung_Anzahl[num] = 0;
                        int num2 = 0;
                        if (dtc.DTCContext != null)
                        {
                            typeDTCContext first = dtc.First;
                            typeDTCContext current2 = dtc.Current;
                            if (first != null && current2 != null && current2.F_UW_ANZ.HasValue)
                            {
                                int num3 = current2.F_UW_ANZ.Value;
                                if (num3 > m_maxAnzahlUmweltbedingungen)
                                {
                                    num3 = m_maxAnzahlUmweltbedingungen;
                                }

                                for (int num4 = 0; num4 < num3; num4++)
                                {
                                    try
                                    {
                                        if (num2 < m_maxAnzahlUmweltbedingungen)
                                        {
                                            Umweltbedingung_Wert[num2 * 300 + num] = GetUmweltbedingungWert(first.F_UW[num4]);
                                            Umweltbedingung_Wert[4500 + num2 * 300 + num] = GetUmweltbedingungWert(current2.F_UW[num4]);
                                            Umweltbedingung_NR[num2 * 300 + num] = (int)first.F_UW[num4].F_UW_NR.Value;
                                            Umweltbedingung_NR[4500 + num2 * 300 + num] = (int)current2.F_UW[num4].F_UW_NR.Value;
                                            num2++;
                                        }
                                    }
                                    catch (Exception exception)
                                    {
                                        Log.WarningException("FS_LISTE_ISTA_FORT.Details_zum_FehlerortRG()", exception);
                                    }
                                }
                            }

                            Umweltbedingung_Anzahl[num] = num2;
                        }

                        Systemzeit_Anfang[num] = (int)((dtc.First != null && dtc.First.F_UW_ZEIT_SUPREME.HasValue) ? ((int)dtc.First.F_UW_ZEIT_SUPREME.Value) : ((dtc.First != null && dtc.First.F_UW_ZEIT.HasValue) ? dtc.First.F_UW_ZEIT.Value : (-1)));
                        Systemzeit_Ende[num] = (int)((dtc.Current != null && dtc.Current.F_UW_ZEIT_SUPREME.HasValue) ? ((int)dtc.Current.F_UW_ZEIT_SUPREME.Value) : ((dtc.Current != null && dtc.Current.F_UW_ZEIT.HasValue) ? dtc.Current.F_UW_ZEIT.Value : (-1)));
                        Kilometer_Anfang[num] = (int)((dtc.First != null && dtc.First.F_UW_KM_SUPREME.HasValue) ? ((int)dtc.First.F_UW_KM_SUPREME.Value) : ((dtc.First != null && dtc.First.F_UW_KM.HasValue) ? dtc.First.F_UW_KM.Value : (-1)));
                        Kilometer_Ende[num] = (int)((dtc.Current != null && dtc.Current.F_UW_KM_SUPREME.HasValue) ? ((int)dtc.Current.F_UW_KM_SUPREME.Value) : ((dtc.Current != null && dtc.Current.F_UW_KM.HasValue) ? dtc.Current.F_UW_KM.Value : Kilometer_Anfang[num]));
                        Fehlerkode_Ueberlauf[num] = 0;
                        Anzahl_Fehlerspeicher++;
                        num++;
                    }
                    catch (Exception exception2)
                    {
                        Log.WarningException("FS_LISTE_ISTA.ReadingIstaListe()", exception2);
                    }
                }
            }
        }

        private double GetUmweltbedingungWert(F_UW uw)
        {
            if (uw == null)
            {
                return 0.0;
            }

            try
            {
                if ("hex".Equals(uw.F_UW_EINH, StringComparison.OrdinalIgnoreCase))
                {
                    return Convert.ToInt64(uw.F_UW_WERT.ToString(), 16);
                }

                if ("text".Equals(uw.F_UW_EINH, StringComparison.OrdinalIgnoreCase))
                {
                    Log.Info("FS_LISTE_ISTA.GetUmweltbedingungWert()", "Failed to convert F_UW_WERT '{0}' to double, thus returning 0. F_UW_EINH='{1}', F_UW_TEXT='{2}'", uw.F_UW_WERT, uw.F_UW_EINH, uw.F_UW_TEXT);
                    return 0.0;
                }

                return Convert.ToDouble(uw.F_UW_WERT, CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {
                Log.Warning("FS_LISTE_ISTA.GetUmweltbedingungWert()", "Failed to convert F_UW_WERT \"{0}\" to double, thus returning 0. F_UW_EINH=\"{1}\", F_UW_TEXT=\"{2}\", exception message: {3}", uw.F_UW_WERT, uw.F_UW_EINH, uw.F_UW_TEXT, ex.Message);
                return 0.0;
            }
        }
    }
}