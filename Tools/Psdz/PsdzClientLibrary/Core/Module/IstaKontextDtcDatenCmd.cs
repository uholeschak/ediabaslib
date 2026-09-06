using BMW.Rheingold.CoreFramework;
using BMW.Rheingold.Module.ISTA;
using PsdzClient.Core;
using PsdzClient.Core.Container;

namespace BMW.Rheingold.Module.ISTA
{
    internal class IstaKontextDtcDatenCmd : ServiceDialogCmdBase
    {
        public IstaKontextDtcDatenCmd(ISTAModule callingModule, string methodName, string path, IModuleExecutionParent globalTabModuleISTA, int elementNo)
            : base(callingModule, methodName, path, globalTabModuleISTA, elementNo)
        {
        }

        public override void CreateDialog(ParameterContainer inParam, ParameterContainer inoutParam)
        {
            Log.Info("IstaKontextDtcDatenCmd.CreateDialog()", "ISTA_Kontext_DTC_Daten init started.");
            base.Display = false;
        }

        public override void DoInvoke(string method, ParameterContainer inParam, ParameterContainer outParam, ParameterContainer inoutParam)
        {
            if (base.CallingModule == null)
            {
                Log.Error("IstaKontextDtcDatenCmd.DoInvoke()", "Failed to invoke method {0}, because calling module is null.", method);
                return;
            }
            ModuleParameter value = base.CallingModule.__RheinGoldCoreModuleParameters__.Clone();
            inParam.Parameter.Add("__RheinGoldCoreModuleParameters__", value);
            inParam.Parameter.Add("__RheinGoldTabModuleISTA__", base.CallingModule.GlobalTabModuleISTA);
            inParam.Parameter.Add("__RheinGoldSOCAccessor__", base.CallingModule.SOCAccessor);
            if ("DTC_Details_kurz".Equals(method))
            {
                string f_ORT_NR_HEX = inParam.getParameter("F_ORT_NR_HEX", string.Empty) as string;
                int F_finden_ORT_NR_DEZ = 0;
                string F_finden_ORT_TEXT = string.Empty;
                int F_finden_VORHANDEN_NR = 0;
                string F_finden_VORHANDEN_TEXT = string.Empty;
                int F_finden_HFK = 0;
                new ISTA_Kontext_DTC_Daten(inParam).DTC_Details_kurz(f_ORT_NR_HEX, ref F_finden_ORT_NR_DEZ, ref F_finden_ORT_TEXT, ref F_finden_VORHANDEN_NR, ref F_finden_VORHANDEN_TEXT, ref F_finden_HFK);
                outParam.setParameter("F_finden_ORT_NR_DEZ", F_finden_ORT_NR_DEZ);
                outParam.setParameter("F_finden_ORT_TEXT", F_finden_ORT_TEXT);
                outParam.setParameter("F_finden_VORHANDEN_NR", F_finden_VORHANDEN_NR);
                outParam.setParameter("F_finden_VORHANDEN_TEXT", F_finden_VORHANDEN_TEXT);
                outParam.setParameter("F_finden_HFK", F_finden_HFK);
                return;
            }
            if ("DTC_Details_lang".Equals(method))
            {
                string f_ORT_NR_HEX2 = inParam.getParameter("F_ORT_NR_HEX", string.Empty) as string;
                int F_finden_ORT_NR_DEZ2 = 0;
                string F_finden_ORT_TEXT2 = string.Empty;
                int F_finden_VORHANDEN_NR2 = 0;
                string F_finden_VORHANDEN_TEXT2 = string.Empty;
                int F_finden_HFK2 = 0;
                int F_finden_HLZ = 0;
                int F_finden_EREIGNIS_DTC = 0;
                string F_finden_SGBD = string.Empty;
                double F_finden_UW_KM_L = 0.0;
                double F_finden_UW_ZEIT_L = 0.0;
                new ISTA_Kontext_DTC_Daten(inParam).DTC_Details_lang(f_ORT_NR_HEX2, ref F_finden_ORT_NR_DEZ2, ref F_finden_ORT_TEXT2, ref F_finden_VORHANDEN_NR2, ref F_finden_VORHANDEN_TEXT2, ref F_finden_HFK2, ref F_finden_HLZ, ref F_finden_EREIGNIS_DTC, ref F_finden_SGBD, ref F_finden_UW_KM_L, ref F_finden_UW_ZEIT_L);
                outParam.setParameter("F_finden_ORT_NR_DEZ", F_finden_ORT_NR_DEZ2);
                outParam.setParameter("F_finden_ORT_TEXT", F_finden_ORT_TEXT2);
                outParam.setParameter("F_finden_VORHANDEN_NR", F_finden_VORHANDEN_NR2);
                outParam.setParameter("F_finden_VORHANDEN_TEXT", F_finden_VORHANDEN_TEXT2);
                outParam.setParameter("F_finden_HFK", F_finden_HFK2);
                outParam.setParameter("F_finden_HLZ", F_finden_HLZ);
                outParam.setParameter("F_finden_EREIGNIS_DTC", F_finden_EREIGNIS_DTC);
                outParam.setParameter("F_finden_SGBD", F_finden_SGBD);
                outParam.setParameter("F_finden_UW_KM_L", F_finden_UW_KM_L);
                outParam.setParameter("F_finden_UW_ZEIT_L", F_finden_UW_ZEIT_L);
                return;
            }
            throw new ServiceDialogMethodUnsupportedException();
        }
    }
}
