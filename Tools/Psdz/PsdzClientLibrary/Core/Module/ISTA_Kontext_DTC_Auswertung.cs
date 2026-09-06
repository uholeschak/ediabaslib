using BMW.Rheingold.CoreFramework;
using BMW.Rheingold.CoreFramework.DatabaseProvider;
using BMW.Rheingold.Module.ISTA;
using PsdzClient.Core.Container;
using System;
using System.Collections.Generic;

namespace BMW.Rheingold.Module.ISTA
{
    internal class ISTA_Kontext_DTC_Auswertung : ISTAServiceDialog
    {
        public ISTA_Kontext_DTC_Auswertung(ParameterContainer InParameter)
        {
            if (InParameter != null)
            {
                _globalModuleInParameter = InParameter;
            }
            __handleInParameter();
        }

        public virtual void Prepare()
        {
        }

        public virtual void Reset()
        {
        }

        public override void Invoke(string method, ParameterContainer inParam, ParameterContainer outParam, ParameterContainer inoutParam)
        {
            if ("DTC_Einzeln".Equals(method))
            {
                string f_ORT_NR_HEX = inParam.getParameter("F_ORT_NR_HEX", null) as string;
                bool DTC_Eingetragen = false;
                DTC_Einzeln(f_ORT_NR_HEX, ref DTC_Eingetragen);
                outParam.setParameter("DTC_Eingetragen", DTC_Eingetragen);
            }
            else if ("DTC_Liste".Equals(method))
            {
                List<string> f_ORT_NR_HEX_LISTE = inParam.getParameter("F_ORT_NR_HEX_LISTE", null) as List<string>;
                bool DTC_Eingetragen_Alle = false;
                int DTC_Eingetragen_Anzahl = 0;
                string DTC_Eingetragen_String = null;
                List<string> DTC_Eingetragen_Liste_HEX = null;
                List<int> DTC_Eingetragen_Liste_DEZ = null;
                DTC_Liste(f_ORT_NR_HEX_LISTE, ref DTC_Eingetragen_Alle, ref DTC_Eingetragen_Anzahl, ref DTC_Eingetragen_String, ref DTC_Eingetragen_Liste_HEX, ref DTC_Eingetragen_Liste_DEZ);
                outParam.setParameter("DTC_Eingetragen_Alle", DTC_Eingetragen_Alle);
                outParam.setParameter("DTC_Eingetragen_Anzahl", DTC_Eingetragen_Anzahl);
                outParam.setParameter("DTC_Eingetragen_String", DTC_Eingetragen_String);
                outParam.setParameter("DTC_Eingetragen_Liste_HEX", DTC_Eingetragen_Liste_HEX);
                outParam.setParameter("DTC_Eingetragen_Liste_DEZ", DTC_Eingetragen_Liste_DEZ);
            }
            else if ("DTC_Bereich".Equals(method))
            {
                string f_ORT_NR_HEX_MIN = inParam.getParameter("F_ORT_NR_HEX_MIN", "-1") as string;
                string f_ORT_NR_HEX_MAX = inParam.getParameter("F_ORT_NR_HEX_MAX", "-1") as string;
                bool DTC_Eingetragen_Alle2 = false;
                int DTC_Eingetragen_Anzahl2 = 0;
                string DTC_Eingetragen_String2 = null;
                List<string> DTC_Eingetragen_Liste_HEX2 = null;
                List<int> DTC_Eingetragen_Liste_DEZ2 = null;
                DTC_Bereich(f_ORT_NR_HEX_MIN, f_ORT_NR_HEX_MAX, ref DTC_Eingetragen_Alle2, ref DTC_Eingetragen_Anzahl2, ref DTC_Eingetragen_String2, ref DTC_Eingetragen_Liste_HEX2, ref DTC_Eingetragen_Liste_DEZ2);
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
                    throw new ServiceDialogMethodUnsupportedException(method);
                }
                string kode = inParam.getParameter("Kode", null) as string;
                bool Kode_Eingetragen = false;
                Sammelfehler_Einzeln(kode, ref Kode_Eingetragen);
                outParam.setParameter("Kode_Eingetragen", Kode_Eingetragen);
            }
        }

        public virtual void DTC_Einzeln(string F_ORT_NR_HEX, ref bool DTC_Eingetragen)
        {
            int num = 0;
            Logger.WriteInformation("DTC_Einzeln called");
            if (string.IsNullOrEmpty(F_ORT_NR_HEX))
            {
                DTC_Eingetragen = false;
            }
            else
            {
                DTC_Eingetragen = false;
                int num2 = Convert.ToInt32(F_ORT_NR_HEX, 16);
                foreach (Fault fault in Vehicle.FaultList)
                {
                    if (fault != null && fault.DTC.Relevance == true && !fault.DTC.IsVirtual && !fault.DTC.IsCombined && fault.DTC.F_ORT.HasValue && fault.DTC.F_ORT == num2)
                    {
                        DTC_Eingetragen = true;
                        break;
                    }
                }
            }
            Logger.WriteInformation("_ExitIndex is: {0}", num);
        }

        public virtual void DTC_Liste(List<string> F_ORT_NR_HEX_LISTE, ref bool DTC_Eingetragen_Alle, ref int DTC_Eingetragen_Anzahl, ref string DTC_Eingetragen_String, ref List<string> DTC_Eingetragen_Liste_HEX, ref List<int> DTC_Eingetragen_Liste_DEZ)
        {
            int num = 0;
            Logger.WriteInformation("DTC_Liste called");
            if (F_ORT_NR_HEX_LISTE == null)
            {
                DTC_Eingetragen_Alle = false;
                DTC_Eingetragen_Anzahl = -1;
                DTC_Eingetragen_String = "NV";
                DTC_Eingetragen_Liste_HEX = new List<string>();
                DTC_Eingetragen_Liste_HEX.Add("NV");
                DTC_Eingetragen_Liste_DEZ = new List<int>();
                DTC_Eingetragen_Liste_DEZ.Add(-1);
            }
            else
            {
                int num2 = 0;
                List<string> list = new List<string>();
                List<int> list2 = new List<int>();
                List<string> list3 = new List<string>();
                string text = "";
                list.Clear();
                list2.Clear();
                list3.Clear();
                List<int> list4 = new List<int>();
                foreach (string item in F_ORT_NR_HEX_LISTE)
                {
                    if (!string.IsNullOrEmpty(item))
                    {
                        list4.Add(Convert.ToInt32(item, 16));
                    }
                }
                List<string> list5 = new List<string>();
                List<int> list6 = new List<int>();
                List<string> list7 = new List<string>();
                foreach (Fault fault in Vehicle.FaultList)
                {
                    if (fault != null && fault.DTC.Relevance == true && !fault.DTC.IsVirtual && !fault.DTC.IsCombined && fault.DTC.F_ORT.HasValue && list4.Contains((int)fault.DTC.F_ORT.Value))
                    {
                        list5.Add(Convert.ToString((int)fault.DTC.F_ORT.Value, 16).ToUpper());
                        list6.Add((int)fault.DTC.F_ORT.Value);
                        list7.Add(Convert.ToString((int)fault.DTC.F_ORT.Value, 16).ToUpper() + " " + ((fault.XepFaultLabel != null && !string.IsNullOrEmpty(fault.XepFaultLabel.Title)) ? fault.XepFaultLabel.Title : FaultCodeConverters.LocalizedFaultLabel(fault.ECU, fault.DTC, Vehicle, FFMResolver)));
                    }
                }
                num2 = list6.Count;
                list2 = list6;
                list = list5;
                list3 = list7;
                if (num2 == 0)
                {
                    DTC_Eingetragen_Alle = false;
                    DTC_Eingetragen_Anzahl = num2;
                    DTC_Eingetragen_String = "NV";
                    DTC_Eingetragen_Liste_HEX = new List<string>();
                    DTC_Eingetragen_Liste_HEX.Add("NV");
                    DTC_Eingetragen_Liste_DEZ = new List<int>();
                    DTC_Eingetragen_Liste_DEZ.Add(-1);
                }
                else
                {
                    if (num2 < F_ORT_NR_HEX_LISTE.Count)
                    {
                        DTC_Eingetragen_Alle = false;
                    }
                    else
                    {
                        DTC_Eingetragen_Alle = true;
                    }
                    DTC_Eingetragen_Anzahl = num2;
                    text += "<spe:TEXTITEM  xmlns:spe='http://bmw.com/2014/Spe_Text_2.0'><spe:LIST>";
                    base._DoLoopHandling = true;
                    for (int i = 0; i < list.Count; i++)
                    {
                        text = text + "<spe:LISTENTRY>" + list3[i] + "</spe:LISTENTRY>";
                    }
                    base._DoLoopHandling = false;
                    text += "</spe:LIST></spe:TEXTITEM>";
                    DTC_Eingetragen_String = text;
                    DTC_Eingetragen_Liste_HEX = list;
                    DTC_Eingetragen_Liste_DEZ = list2;
                }
            }
            Logger.WriteInformation("_ExitIndex is: {0}", num);
        }

        public virtual void DTC_Bereich(string F_ORT_NR_HEX_MIN, string F_ORT_NR_HEX_MAX, ref bool DTC_Eingetragen_Alle, ref int DTC_Eingetragen_Anzahl, ref string DTC_Eingetragen_String, ref List<string> DTC_Eingetragen_Liste_HEX, ref List<int> DTC_Eingetragen_Liste_DEZ)
        {
            int num = 0;
            Logger.WriteInformation("DTC_Bereich called");
            if (F_ORT_NR_HEX_MIN == "-1" || F_ORT_NR_HEX_MAX == "-1")
            {
                DTC_Eingetragen_Alle = false;
                DTC_Eingetragen_Anzahl = -1;
                DTC_Eingetragen_String = "NV";
                DTC_Eingetragen_Liste_HEX = new List<string>();
                DTC_Eingetragen_Liste_HEX.Add("NV");
                DTC_Eingetragen_Liste_DEZ = new List<int>();
                DTC_Eingetragen_Liste_DEZ.Add(-1);
            }
            else
            {
                int num2 = ((!string.IsNullOrEmpty(F_ORT_NR_HEX_MIN)) ? Convert.ToInt32(F_ORT_NR_HEX_MIN, 16) : (-1));
                int num3 = ((!string.IsNullOrEmpty(F_ORT_NR_HEX_MAX)) ? Convert.ToInt32(F_ORT_NR_HEX_MAX, 16) : (-1));
                int num4 = 0;
                List<string> list = new List<string>();
                List<int> list2 = new List<int>();
                List<string> list3 = new List<string>();
                string text = "";
                list.Clear();
                list2.Clear();
                list3.Clear();
                List<string> list4 = new List<string>();
                List<int> list5 = new List<int>();
                List<string> list6 = new List<string>();
                foreach (Fault fault in Vehicle.FaultList)
                {
                    if (fault != null && fault.DTC.Relevance == true && !fault.DTC.IsVirtual && !fault.DTC.IsCombined && fault.DTC.F_ORT.HasValue && (int)fault.DTC.F_ORT.Value >= num2 && (int)fault.DTC.F_ORT.Value <= num3)
                    {
                        list4.Add(Convert.ToString((int)fault.DTC.F_ORT.Value, 16).ToUpper());
                        list5.Add((int)fault.DTC.F_ORT.Value);
                        list6.Add(Convert.ToString((int)fault.DTC.F_ORT.Value, 16).ToUpper() + " " + ((fault.XepFaultLabel != null && !string.IsNullOrEmpty(fault.XepFaultLabel.Title)) ? fault.XepFaultLabel.Title : FaultCodeConverters.LocalizedFaultLabel(fault.ECU, fault.DTC, Vehicle, FFMResolver)));
                    }
                }
                num4 = list5.Count;
                list2 = list5;
                list = list4;
                list3 = list6;
                if (num4 == 0)
                {
                    DTC_Eingetragen_Alle = false;
                    DTC_Eingetragen_Anzahl = num4;
                    DTC_Eingetragen_String = "NV";
                    DTC_Eingetragen_Liste_HEX = new List<string>();
                    DTC_Eingetragen_Liste_HEX.Add("NV");
                    DTC_Eingetragen_Liste_DEZ = new List<int>();
                    DTC_Eingetragen_Liste_DEZ.Add(-1);
                }
                else
                {
                    if (num4 < num3 - num2)
                    {
                        DTC_Eingetragen_Alle = false;
                    }
                    else
                    {
                        DTC_Eingetragen_Alle = true;
                    }
                    DTC_Eingetragen_Anzahl = num4;
                    text += "<spe:TEXTITEM  xmlns:spe='http://bmw.com/2014/Spe_Text_2.0'><spe:LIST>";
                    base._DoLoopHandling = true;
                    for (int i = 0; i < list.Count; i++)
                    {
                        text = text + "<spe:LISTENTRY>" + list3[i] + "</spe:LISTENTRY>";
                    }
                    base._DoLoopHandling = false;
                    text += "</spe:LIST></spe:TEXTITEM>";
                    DTC_Eingetragen_String = text;
                    DTC_Eingetragen_Liste_HEX = list;
                    DTC_Eingetragen_Liste_DEZ = list2;
                }
            }
            Logger.WriteInformation("_ExitIndex is: {0}", num);
        }

        public virtual void Sammelfehler_Einzeln(string Kode, ref bool Kode_Eingetragen)
        {
            int num = 0;
            Logger.WriteInformation("Sammelfehler_Einzeln called");
            if (Kode == null)
            {
                Kode_Eingetragen = false;
            }
            else
            {
                List<string> list = new List<string>();
                ParameterContainer parameterContainer = new ParameterContainer();
                ParameterContainer parameterContainer2 = new ParameterContainer();
                ParameterContainer parameterContainer3 = new ParameterContainer();
                base.Factory.CreateServiceDialog(this, "Sammelfehler_Einzeln", "52683531", _globalTabModuleISTA, 2722, parameterContainer, parameterContainer3).Invoke("Sammelfehlerkodes", parameterContainer, parameterContainer2, parameterContainer3);
                if (parameterContainer2.getParameter("fehlerkode_hex") != null)
                {
                    list = (List<string>)parameterContainer2.getParameter("fehlerkode_hex");
                }
                Kode_Eingetragen = list?.Contains(Kode.TrimStart('S', ' ', '0')) ?? false;
            }
            Logger.WriteInformation("_ExitIndex is: {0}", num);
        }
    }
}
