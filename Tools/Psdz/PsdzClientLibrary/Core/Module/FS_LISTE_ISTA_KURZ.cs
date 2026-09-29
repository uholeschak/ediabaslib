using BMW.Rheingold.CoreFramework;
using BMW.Rheingold.CoreFramework.DatabaseProvider;
using BMW.Rheingold.Module.ISTA;
using PsdzClient.Core;
using PsdzClient.Core.Container;
using System;
using System.Collections.Generic;
using BMW.Rheingold.CoreFramework.Contracts.Vehicle;

namespace BMW.Rheingold.Module.ISTA
{
    internal class FS_LISTE_ISTA_KURZ : ISTAModule
    {
        public string strDlgInfo;

        public int m_maxAnzahlFehlerkode;

        public bool bWriteLog;

        public bool p_WeiterButtonEnabledStack;

        public FS_LISTE_ISTA_KURZ(ParameterContainer InParameter)
        {
            if (InParameter != null)
            {
                _globalModuleInParameter = InParameter;
            }
            __handleInParameter();
            strDlgInfo = "V32-2020-10-22-FS_LISTE_ISTA_KURZ";
            m_maxAnzahlFehlerkode = 300;
            bWriteLog = true;
            p_WeiterButtonEnabledStack = false;
        }

        public virtual void Prepare()
        {
        }

        public virtual void Reset()
        {
        }

        public virtual void InitializeDialog(ref string[] Fehlerkode_Text, ref int Anzahl_Fehlerspeicher, ref int[] Fehlerkode_dez, ref string[] Fehlerkode_hex, ref string[] Fehlerkode_SGBD)
        {
            int num = 0;
            Logger.WriteInformation("InitializeDialogcalled");
            Logger.WriteInformation(strDlgInfo);
            Anzahl_Fehlerspeicher = 0;
            Fehlerkode_Text = new string[m_maxAnzahlFehlerkode];
            Fehlerkode_dez = new int[m_maxAnzahlFehlerkode];
            Fehlerkode_hex = new string[m_maxAnzahlFehlerkode];
            Fehlerkode_SGBD = new string[m_maxAnzahlFehlerkode];
            base._DoLoopHandling = true;
            for (int i = 0; i < m_maxAnzahlFehlerkode; i++)
            {
                Fehlerkode_Text[i] = "";
                Fehlerkode_dez[i] = 0;
                Fehlerkode_hex[i] = "";
                Fehlerkode_SGBD[i] = "";
            }
            base._DoLoopHandling = false;
            List<int> list = new List<int>();
            List<string> list2 = new List<string>();
            List<string> list3 = new List<string>();
            List<string> list4 = new List<string>();
            foreach (Fault fault in Vehicle.FaultList)
            {
                if (fault != null && fault.DTC.Relevance == true && !fault.DTC.IsVirtual && !fault.DTC.IsCombined && fault.DTC.F_ORT.HasValue && fault.ECU != null)
                {
                    list.Add(Convert.ToInt32(fault.DTC.F_ORT));
                    list2.Add($"{Convert.ToInt32(fault.DTC.F_ORT):X}");
                    list3.Add((fault.XepFaultLabel != null && !string.IsNullOrEmpty(fault.XepFaultLabel.Title)) ? fault.XepFaultLabel.Title : FaultCodeConverters.LocalizedFaultLabel(fault.ECU, fault.DTC, Vehicle, FFMResolver));
                    if (!string.IsNullOrEmpty(fault.ECU.ECU_SGBD))
                    {
                        list4.Add(fault.ECU.ECU_SGBD);
                        continue;
                    }
                    if (!string.IsNullOrEmpty(fault.ECU.VARIANTE))
                    {
                        list4.Add(fault.ECU.VARIANTE);
                        continue;
                    }
                    Log.Info(Log.CurrentMethod(), "adding fallback value '#NV' because SGBD and Variante are null or empty");
                    list4.Add("#NV");
                }
            }
            Anzahl_Fehlerspeicher = list.Count;
            Fehlerkode_dez = list.ToArray();
            Fehlerkode_hex = list2.ToArray();
            Fehlerkode_Text = list3.ToArray();
            Fehlerkode_SGBD = list4.ToArray();
            Logger.WriteInformation("_ExitIndex is: {0}", num);
        }

        public virtual void LeseAnzahlFehlerspeicher(ref int Anzahl_Fehlerspeicher)
        {
            int num = 0;
            Logger.WriteInformation("LeseAnzahlFehlerspeichercalled");
            Logger.WriteInformation($"{strDlgInfo}, Methode(LeseAnzahlFehlerspeicher) Start");
            int num2 = 0;
            int num3 = 0;
            int num4 = 0;
            foreach (Fault fault in Vehicle.FaultList)
            {
                if (!fault.DTC.IsVirtual && !fault.DTC.IsCombined)
                {
                    num2++;
                }
                else if (fault.DTC.IsVirtual && !fault.DTC.IsCombined)
                {
                    num3++;
                }
                else if (fault.DTC.IsVirtual && fault.DTC.IsCombined)
                {
                    num4++;
                }
            }
            Anzahl_Fehlerspeicher = num2;
            Logger.WriteInformation($"{strDlgInfo}, Methode(LeseAnzahlFehlerspeicher) Ende(Anzahl_Fehlerspeicher={Anzahl_Fehlerspeicher})");
            Logger.WriteInformation("_ExitIndex is: {0}", num);
        }

        public virtual void FSPvorhanden_pruefen(bool FSPvorhanden, ref string SGgruppe, ref string SGvariante, ref string Fehlerart, ref int Fehlercode)
        {
            int num = 0;
            Logger.WriteInformation("FSPvorhanden_pruefencalled");
            if (bWriteLog)
            {
                Logger.WriteInformation($"{strDlgInfo}, Methode(FSPvorhanden_pruefen) Start");
            }
            FSPvorhanden = false;
            if (VehicleContext != null && Vehicle.ECU != null)
            {
                IEcu eCUbyECU_SGBD = Vehicle.getECUbyECU_SGBD(SGvariante);
                if (eCUbyECU_SGBD != null && eCUbyECU_SGBD.FEHLER != null)
                {
                    foreach (IDtc item in eCUbyECU_SGBD.FEHLER)
                    {
                        if (item.Relevance == true && item.F_ORT.HasValue && !string.IsNullOrEmpty(eCUbyECU_SGBD.VARIANTE) && item.F_ORT == Fehlercode)
                        {
                            FSPvorhanden = true;
                        }
                    }
                }
            }
            if (bWriteLog)
            {
                Logger.WriteInformation($"{strDlgInfo}, Methode(FSPvorhanden_pruefen) Ende(FSPvorhanden={FSPvorhanden})");
            }
            Logger.WriteInformation("_ExitIndex is: {0}", num);
        }

        public virtual void FSPvorhanden(ref string SGGruppe, ref string SGVariante, ref string FType, ref int FehlerCode, ref bool isVorhanden)
        {
            int num = 0;
            Logger.WriteInformation("FSPvorhandencalled");
            if (bWriteLog)
            {
                Logger.WriteInformation($"{strDlgInfo}, Methode(FSPvorhanden_pruefen) Start");
            }
            bool flag = false;
            if (VehicleContext != null && Vehicle.ECU != null)
            {
                foreach (ECU item in Vehicle.ECU)
                {
                    if (item == null || item.FEHLER == null || !string.Equals(item.ECU_GRUPPE, SGGruppe, StringComparison.OrdinalIgnoreCase) || !string.Equals(item.VARIANTE, SGVariante, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                    foreach (DTC item2 in item.FEHLER)
                    {
                        if (item2.Relevance == true && !item2.IsVirtual && !item2.IsCombined && item2.F_ORT == FehlerCode)
                        {
                            flag = true;
                        }
                    }
                }
            }
            isVorhanden = flag;
            if (bWriteLog)
            {
                Logger.WriteInformation($"{strDlgInfo}, Methode(FSPvorhanden_pruefen) Ende(FSPvorhanden={isVorhanden})");
            }
            Logger.WriteInformation("_ExitIndex is: {0}", num);
        }

        public virtual void AnzahlFehlerspeicher(ref int anzahlFehlerspeicherGesamt, ref int anzahlFehlerkodes, ref int anzahlVirtuelleFehlerkodes, ref int anzahlSammelfehlerkodes)
        {
            int num = 0;
            Logger.WriteInformation("AnzahlFehlerspeichercalled");
            Logger.WriteInformation($"{strDlgInfo}, Methode(AnzahlFehlerspeicher) Start");
            int num2 = 0;
            int num3 = 0;
            int num4 = 0;
            foreach (Fault fault in Vehicle.FaultList)
            {
                if (!fault.DTC.IsVirtual && !fault.DTC.IsCombined)
                {
                    num2++;
                }
                else if (fault.DTC.IsVirtual && !fault.DTC.IsCombined)
                {
                    num3++;
                }
                else if (fault.DTC.IsVirtual && fault.DTC.IsCombined)
                {
                    num4++;
                }
            }
            anzahlFehlerkodes = num2;
            anzahlVirtuelleFehlerkodes = num3;
            anzahlSammelfehlerkodes = num4;
            anzahlFehlerspeicherGesamt = num2 + num3 + num4;
            Logger.WriteInformation($"{strDlgInfo}, Methode(AnzahlFehlerspeicher) Ende(anzahlFehlerspeicherGesamt={anzahlFehlerspeicherGesamt})");
            Logger.WriteInformation("_ExitIndex is: {0}", num);
        }

        public virtual void Fehlerkodes(ref int anzahl_Fehlerspeicher, ref List<string> fehlerkode_NodeID, ref List<string> fehlerkode_Text, ref List<int> fehlerkode_dez, ref List<string> fehlerkode_hex, ref List<string> fehlerkode_SGBD)
        {
            int num = 0;
            Logger.WriteInformation("Fehlerkodescalled");
            Logger.WriteInformation($"{strDlgInfo}, Methode(Fehlerkodes) Start");
            anzahl_Fehlerspeicher = 0;
            fehlerkode_NodeID = new List<string>();
            fehlerkode_Text = new List<string>();
            fehlerkode_dez = new List<int>();
            fehlerkode_hex = new List<string>();
            fehlerkode_SGBD = new List<string>();
            List<string> list = new List<string>();
            List<int> list2 = new List<int>();
            List<string> list3 = new List<string>();
            List<string> list4 = new List<string>();
            List<string> list5 = new List<string>();
            try
            {
                foreach (Fault fault in Vehicle.FaultList)
                {
                    if (fault != null && fault.DTC.Relevance == true && !fault.DTC.IsVirtual && !fault.DTC.IsCombined && fault.DTC.Id.HasValue && fault.DTC.F_ORT.HasValue && fault.ECU != null)
                    {
                        list.Add(fault.DTC.Id.ToString());
                        list2.Add(Convert.ToInt32(fault.DTC.F_ORT));
                        list3.Add($"{Convert.ToInt32(fault.DTC.F_ORT):X}");
                        list4.Add((fault.XepFaultLabel != null && !string.IsNullOrEmpty(fault.XepFaultLabel.Title)) ? fault.XepFaultLabel.Title : FaultCodeConverters.LocalizedFaultLabel(fault.ECU, fault.DTC, Vehicle, FFMResolver));
                        if (!string.IsNullOrEmpty(fault.ECU.ECU_SGBD))
                        {
                            list5.Add(fault.ECU.ECU_SGBD);
                            continue;
                        }
                        if (!string.IsNullOrEmpty(fault.ECU.VARIANTE))
                        {
                            list5.Add(fault.ECU.VARIANTE);
                            continue;
                        }
                        Log.Info(Log.CurrentMethod(), "adding fallback value '#NV' because SGBD and Variante are null or empty");
                        list5.Add("#NV");
                    }
                    else
                    {
                        Log.Info(Log.CurrentMethod(), "Fault skipped. Fault details: relevance = {0}, isVirtual = {1}, isCombined = {2}, dtcID = {3}, dtcFort = {4}, ECUSGBD = {5}, Variant = {6}", fault.DTC.Relevance, fault.DTC.IsVirtual, fault.DTC.IsCombined, fault.DTC.Id, fault.DTC.F_ORT, (fault.ECU == null) ? "" : fault.ECU.ECU_SGBD, (fault.ECU == null) ? "" : fault.ECU.VARIANTE);
                    }
                }
            }
            catch (Exception exception)
            {
                Log.WarningException(Log.CurrentMethod(), exception);
            }
            anzahl_Fehlerspeicher = list2.Count;
            fehlerkode_NodeID = list;
            fehlerkode_dez = list2;
            fehlerkode_hex = list3;
            fehlerkode_Text = list4;
            fehlerkode_SGBD = list5;
            Logger.WriteInformation($"{strDlgInfo}, Methode(Fehlerkodes) Ende(anzahl_Fehlerspeicher={anzahl_Fehlerspeicher})");
            Logger.WriteInformation("_ExitIndex is: {0}", num);
        }

        public virtual void VirtuelleFehlerkodes(ref int anzahl_Fehlerspeicher, ref List<string> fehlerkode_NodeID, ref List<string> fehlerkode_Text, ref List<int> fehlerkode_dez, ref List<string> fehlerkode_hex, ref List<string> fehlerkode_SGBD)
        {
            int num = 0;
            Logger.WriteInformation("VirtuelleFehlerkodescalled");
            Logger.WriteInformation($"{strDlgInfo}, Methode(VirtuelleFehlerkodes) Start");
            anzahl_Fehlerspeicher = 0;
            fehlerkode_NodeID = new List<string>();
            fehlerkode_Text = new List<string>();
            fehlerkode_dez = new List<int>();
            fehlerkode_hex = new List<string>();
            fehlerkode_SGBD = new List<string>();
            List<string> list = new List<string>();
            List<int> list2 = new List<int>();
            List<string> list3 = new List<string>();
            List<string> list4 = new List<string>();
            List<string> list5 = new List<string>();
            foreach (Fault fault in Vehicle.FaultList)
            {
                if (fault != null && fault.DTC.Relevance == true && fault.DTC.IsVirtual && !fault.DTC.IsCombined && fault.DTC.Id.HasValue && fault.DTC.F_ORT.HasValue && fault.ECU != null)
                {
                    list.Add(fault.DTC.Id.ToString());
                    list2.Add(Convert.ToInt32(fault.DTC.F_ORT));
                    list3.Add($"{Convert.ToInt32(fault.DTC.F_ORT):X}");
                    list4.Add((fault.XepFaultLabel != null && !string.IsNullOrEmpty(fault.XepFaultLabel.Title)) ? fault.XepFaultLabel.Title : FaultCodeConverters.LocalizedFaultLabel(fault.ECU, fault.DTC, Vehicle, FFMResolver));
                    if (!string.IsNullOrEmpty(fault.ECU.ECU_SGBD))
                    {
                        list5.Add(fault.ECU.ECU_SGBD);
                        continue;
                    }
                    if (!string.IsNullOrEmpty(fault.ECU.VARIANTE))
                    {
                        list5.Add(fault.ECU.VARIANTE);
                        continue;
                    }
                    Log.Info(Log.CurrentMethod(), "adding fallback value '#NV' because SGBD and Variante are null or empty");
                    list5.Add("#NV");
                }
            }
            anzahl_Fehlerspeicher = list2.Count;
            fehlerkode_NodeID = list;
            fehlerkode_dez = list2;
            fehlerkode_hex = list3;
            fehlerkode_Text = list4;
            fehlerkode_SGBD = list5;
            Logger.WriteInformation($"{strDlgInfo}, Methode(VirtuelleFehlerkodes) Ende(anzahl_Fehlerspeicher={anzahl_Fehlerspeicher})");
            Logger.WriteInformation("_ExitIndex is: {0}", num);
        }

        public virtual void Sammelfehlerkodes(ref int anzahl_Fehlerspeicher, ref List<string> fehlerkode_NodeID, ref List<string> fehlerkode_Text, ref List<int> fehlerkode_dez, ref List<string> fehlerkode_hex, ref List<string> fehlerkode_SGBD)
        {
            int num = 0;
            Logger.WriteInformation("Sammelfehlerkodescalled");
            Logger.WriteInformation($"{strDlgInfo}, Methode(Sammelfehlerkodes) Start");
            anzahl_Fehlerspeicher = 0;
            fehlerkode_NodeID = new List<string>();
            fehlerkode_Text = new List<string>();
            fehlerkode_dez = new List<int>();
            fehlerkode_hex = new List<string>();
            fehlerkode_SGBD = new List<string>();
            List<string> list = new List<string>();
            List<int> list2 = new List<int>();
            List<string> list3 = new List<string>();
            List<string> list4 = new List<string>();
            List<string> list5 = new List<string>();
            foreach (Fault fault in Vehicle.FaultList)
            {
                if (fault != null && fault.DTC.Relevance == true && fault.DTC.IsVirtual && fault.DTC.IsCombined && fault.DTC.Id.HasValue && fault.DTC.F_ORT.HasValue)
                {
                    list.Add(fault.DTC.Id.ToString());
                    list2.Add(Convert.ToInt32(fault.DTC.F_ORT));
                    list3.Add($"{Convert.ToInt32(fault.DTC.F_ORT):X}");
                    list4.Add((fault.XepFaultLabel != null && !string.IsNullOrEmpty(fault.XepFaultLabel.Title)) ? fault.XepFaultLabel.Title : FaultCodeConverters.LocalizedFaultLabel(fault.ECU, fault.DTC, Vehicle, FFMResolver));
                    list5.Add("NV");
                }
            }
            anzahl_Fehlerspeicher = list2.Count;
            fehlerkode_NodeID = list;
            fehlerkode_dez = list2;
            fehlerkode_hex = list3;
            fehlerkode_Text = list4;
            fehlerkode_SGBD = list5;
            Logger.WriteInformation($"{strDlgInfo}, Methode(Sammelfehlerkodes) Ende(anzahl_Fehlerspeicher={anzahl_Fehlerspeicher})");
            Logger.WriteInformation("_ExitIndex is: {0}", num);
        }
    }
}
