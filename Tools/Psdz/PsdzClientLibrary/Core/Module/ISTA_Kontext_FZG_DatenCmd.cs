using BMW.Rheingold.CoreFramework;
using BMW.Rheingold.Module.ISTA;
using PsdzClient.Core;
using PsdzClient.Core.Container;

namespace BMW.Rheingold.Module.ISTA
{
    internal class ISTA_Kontext_FZG_DatenCmd : ServiceDialogCmdBase
    {
        public ISTA_Kontext_FZG_DatenCmd(ISTAModule callingModule, string methodName, string path, IModuleExecutionParent globalTabModuleISTA, int elementNo)
            : base(callingModule, methodName, path, globalTabModuleISTA, elementNo)
        {
        }

        public override void CreateDialog(ParameterContainer inParam, ParameterContainer inoutParam)
        {
            Log.Info("ISTA_Kontext_FZG_DatenCmd.CreateDialog()", "ISTA_Kontext_DTC_Daten init started.");
            base.Display = false;
        }

        public override void DoInvoke(string method, ParameterContainer inParam, ParameterContainer outParam, ParameterContainer inoutParam)
        {
            if (base.CallingModule == null)
            {
                Log.Error("ISTA_Kontext_FZG_DatenCmd.Invoke()", "Failed to invoke method {0}, because calling module is null.", method);
                return;
            }
            ModuleParameter value = base.CallingModule.__RheinGoldCoreModuleParameters__.Clone();
            inParam.Parameter.Add("__RheinGoldCoreModuleParameters__", value);
            inParam.Parameter.Add("__RheinGoldTabModuleISTA__", base.CallingModule.GlobalTabModuleISTA);
            inParam.Parameter.Add("__RheinGoldSOCAccessor__", base.CallingModule.SOCAccessor);
            if ("Baustand".Equals(method))
            {
                string Baustand_String = string.Empty;
                int Baustand_Jahr = 0;
                int Baustand_Monat = 0;
                int Baustand_JJJJMM = 0;
                new ISTA_Kontext_FZG_Daten(inParam).Baustand(ref Baustand_String, ref Baustand_Jahr, ref Baustand_Monat, ref Baustand_JJJJMM);
                outParam.setParameter("Baustand_String", Baustand_String);
                outParam.setParameter("Baustand_Jahr", Baustand_Jahr);
                outParam.setParameter("Baustand_Monat", Baustand_Monat);
                outParam.setParameter("Baustand_JJJJMM", Baustand_JJJJMM);
            }
            else if ("Produktionsdatum".Equals(method))
            {
                string Produktionsdatum_String = null;
                int Produktionsdatum_Jahr = 0;
                int Produktionsdatum_Monat = 0;
                int Produktionsdatum_JJJJMM = 0;
                new ISTA_Kontext_FZG_Daten(inParam).Produktionsdatum(ref Produktionsdatum_String, ref Produktionsdatum_Jahr, ref Produktionsdatum_Monat, ref Produktionsdatum_JJJJMM);
                outParam.setParameter("Produktionsdatum_String", Produktionsdatum_String);
                outParam.setParameter("Produktionsdatum_Jahr", Produktionsdatum_Jahr);
                outParam.setParameter("Produktionsdatum_Monat", Produktionsdatum_Monat);
                outParam.setParameter("Produktionsdatum_JJJJMM", Produktionsdatum_JJJJMM);
            }
            else if ("IStufeHO".Equals(method))
            {
                string IStufeHO_String = null;
                int IStufeHO_Jahr = 0;
                int IStufeHO_Monat = 0;
                int IStufeHO_Nummer = 0;
                int IStufeHO_JJMMIII = 0;
                new ISTA_Kontext_FZG_Daten(inParam).IStufeHO(ref IStufeHO_String, ref IStufeHO_Jahr, ref IStufeHO_Monat, ref IStufeHO_Nummer, ref IStufeHO_JJMMIII);
                outParam.setParameter("IStufeHO_String", IStufeHO_String);
                outParam.setParameter("IStufeHO_Jahr", IStufeHO_Jahr);
                outParam.setParameter("IStufeHO_Monat", IStufeHO_Monat);
                outParam.setParameter("IStufeHO_Nummer", IStufeHO_Nummer);
                outParam.setParameter("IStufeHO_JJMMIII", IStufeHO_JJMMIII);
            }
            else if ("IStufeWerk".Equals(method))
            {
                string IStufeWerk_String = null;
                int IStufeWerk_Jahr = 0;
                int IStufeWerk_Monat = 0;
                int IStufeWerk_Nummer = 0;
                int IStufeWerk_JJMMIII = 0;
                new ISTA_Kontext_FZG_Daten(inParam).IStufeWerk(ref IStufeWerk_String, ref IStufeWerk_Jahr, ref IStufeWerk_Monat, ref IStufeWerk_Nummer, ref IStufeWerk_JJMMIII);
                outParam.setParameter("IStufeWerk_String", IStufeWerk_String);
                outParam.setParameter("IStufeWerk_Jahr", IStufeWerk_Jahr);
                outParam.setParameter("IStufeWerk_Monat", IStufeWerk_Monat);
                outParam.setParameter("IStufeWerk_Nummer", IStufeWerk_Nummer);
                outParam.setParameter("IStufeWerk_JJMMIII", IStufeWerk_JJMMIII);
            }
            else
            {
                Log.Error("ISTA_Kontext_FZG_DatenCmd.Invoke()", "Unsupported method {0} will be ignored.", method);
            }
        }
    }
}
