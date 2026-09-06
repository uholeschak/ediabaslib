using BMW.Authoring.Vehicle;
using BMW.Rheingold.Module.ISTA;
using PsdzClient.Core;
using PsdzClient.Core.Container;
using System;
using BMW.Rheingold.CoreFramework;
using BMW.Rheingold.CoreFramework.DatabaseProvider;

namespace BMW.Rheingold.Module.ISTA
{
    public class ISTA_Kontext_DTC_Daten : ISTAModule
    {
        public int Ersatzwert_Zahl;

        public string Ersatzwert_String;

        public bool Ersatzwert_Bool;

        public ITextLocator Ersatzwert_Text;

        public ISTA_Kontext_DTC_Daten(ParameterContainer InParameter)
        {
            if (InParameter != null)
            {
                _globalModuleInParameter = InParameter;
            }
            __handleInParameter();
            Ersatzwert_Zahl = -1;
            Ersatzwert_String = "NV";
            Ersatzwert_Bool = false;
            Ersatzwert_Text = __Text("");
        }

        public virtual void Prepare()
        {
        }

        public virtual void Reset()
        {
        }

        public virtual void DTC_Details_kurz(string F_ORT_NR_HEX, ref int F_finden_ORT_NR_DEZ, ref string F_finden_ORT_TEXT, ref int F_finden_VORHANDEN_NR, ref string F_finden_VORHANDEN_TEXT, ref int F_finden_HFK)
        {
            int num = 0;
            Logger.WriteInformation("DTC_Details_kurzcalled");
            F_finden_ORT_NR_DEZ = Ersatzwert_Zahl;
            F_finden_ORT_TEXT = Ersatzwert_String;
            F_finden_VORHANDEN_NR = Ersatzwert_Zahl;
            F_finden_VORHANDEN_TEXT = Ersatzwert_String;
            F_finden_HFK = Ersatzwert_Zahl;
            int num2 = ((!string.IsNullOrEmpty(F_ORT_NR_HEX)) ? Convert.ToInt32(F_ORT_NR_HEX, 16) : (-1));
            if (num2 != -1)
            {
                foreach (Fault fault in Vehicle.FaultList)
                {
                    if (fault != null && fault.DTC.Relevance == true && !fault.DTC.IsVirtual && !fault.DTC.IsCombined && fault.DTC.F_ORT.HasValue && fault.DTC.F_ORT == num2)
                    {
                        F_finden_ORT_NR_DEZ = (int)(fault.DTC.F_ORT.HasValue ? fault.DTC.F_ORT.Value : Ersatzwert_Zahl);
                        F_finden_ORT_TEXT = ((fault.XepFaultLabel != null && !string.IsNullOrEmpty(fault.XepFaultLabel.Title)) ? fault.XepFaultLabel.Title : FaultCodeConverters.LocalizedFaultLabel(fault.ECU, fault.DTC, Vehicle, FFMResolver));
                        F_finden_VORHANDEN_NR = (fault.DTC.F_VORHANDEN_NR.HasValue ? fault.DTC.F_VORHANDEN_NR.Value : Ersatzwert_Zahl);
                        F_finden_VORHANDEN_TEXT = ((!string.IsNullOrEmpty(fault.ExistingLabel)) ? fault.ExistingLabel : FaultCodeConverters.LocalizedExistingLabel(fault.ECU, fault.DTC, Vehicle, FFMResolver));
                        F_finden_HFK = (int)(fault.DTC.F_HFK.HasValue ? fault.DTC.F_HFK.Value : Ersatzwert_Zahl);
                        break;
                    }
                }
            }
            Logger.WriteInformation("_ExitIndex is: {0}", num);
        }

        public virtual void DTC_Details_lang(string F_ORT_NR_HEX, ref int F_finden_ORT_NR_DEZ, ref string F_finden_ORT_TEXT, ref int F_finden_VORHANDEN_NR, ref string F_finden_VORHANDEN_TEXT, ref int F_finden_HFK, ref int F_finden_HLZ, ref int F_finden_EREIGNIS_DTC, ref string F_finden_SGBD, ref double F_finden_UW_KM_L, ref double F_finden_UW_ZEIT_L)
        {
            int num = 0;
            Logger.WriteInformation("DTC_Details_langcalled");
            F_finden_ORT_NR_DEZ = Ersatzwert_Zahl;
            F_finden_ORT_TEXT = Ersatzwert_String;
            F_finden_VORHANDEN_NR = Ersatzwert_Zahl;
            F_finden_VORHANDEN_TEXT = Ersatzwert_String;
            F_finden_HFK = Ersatzwert_Zahl;
            F_finden_HLZ = Ersatzwert_Zahl;
            F_finden_EREIGNIS_DTC = Ersatzwert_Zahl;
            F_finden_SGBD = Ersatzwert_String;
            F_finden_UW_KM_L = Ersatzwert_Zahl;
            F_finden_UW_ZEIT_L = Ersatzwert_Zahl;
            int num2 = ((!string.IsNullOrEmpty(F_ORT_NR_HEX)) ? Convert.ToInt32(F_ORT_NR_HEX, 16) : (-1));
            if (num2 != -1)
            {
                foreach (Fault fault in Vehicle.FaultList)
                {
                    if (fault != null && fault.DTC.Relevance == true && !fault.DTC.IsVirtual && !fault.DTC.IsCombined && fault.DTC.F_ORT.HasValue && fault.DTC.F_ORT == num2)
                    {
                        F_finden_ORT_NR_DEZ = (int)(fault.DTC.F_ORT.HasValue ? fault.DTC.F_ORT.Value : Ersatzwert_Zahl);
                        F_finden_ORT_TEXT = ((fault.XepFaultLabel != null && !string.IsNullOrEmpty(fault.XepFaultLabel.Title)) ? fault.XepFaultLabel.Title : FaultCodeConverters.LocalizedFaultLabel(fault.ECU, fault.DTC, Vehicle, FFMResolver));
                        F_finden_VORHANDEN_NR = (fault.DTC.F_VORHANDEN_NR.HasValue ? fault.DTC.F_VORHANDEN_NR.Value : Ersatzwert_Zahl);
                        F_finden_VORHANDEN_TEXT = ((!string.IsNullOrEmpty(fault.ExistingLabel)) ? fault.ExistingLabel : FaultCodeConverters.LocalizedExistingLabel(fault.ECU, fault.DTC, Vehicle, FFMResolver));
                        F_finden_HFK = (int)(fault.DTC.F_HFK.HasValue ? fault.DTC.F_HFK.Value : Ersatzwert_Zahl);
                        F_finden_HLZ = (int)(fault.DTC.F_HLZ.HasValue ? fault.DTC.F_HLZ.Value : Ersatzwert_Zahl);
                        F_finden_EREIGNIS_DTC = (fault.DTC.F_EREIGNIS_DTC.HasValue ? fault.DTC.F_EREIGNIS_DTC.Value : Ersatzwert_Zahl);
                        if (!string.IsNullOrEmpty(fault.ECU.ECU_SGBD))
                        {
                            F_finden_SGBD = fault.ECU.ECU_SGBD;
                        }
                        else if (!string.IsNullOrEmpty(fault.ECU.VARIANTE))
                        {
                            F_finden_SGBD = fault.ECU.VARIANTE;
                        }
                        F_finden_UW_KM_L = (fault.DTC.F_UW_KM.HasValue ? ((double)fault.DTC.F_UW_KM.Value) : ((double)Ersatzwert_Zahl));
                        _ = fault.DTC.F_UW_ZEIT;
                        F_finden_UW_ZEIT_L = fault.DTC.F_UW_ZEIT;
                        break;
                    }
                }
            }
            Logger.WriteInformation("_ExitIndex is: {0}", num);
        }
    }
}
