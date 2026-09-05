using BMW.Rheingold.CoreFramework;
using BMW.Rheingold.Module.ISTA;
using PsdzClient.Core;
using PsdzClient.Core.Container;

namespace BMW.Rheingold.Module.ISTA
{
    internal class IstaZeitCmd : ServiceDialogCmdBase
    {
        public IstaZeitCmd(ISTAModule callingModule, string methodName, string path, IModuleExecutionParent globalTabModuleISTA, int elementNo)
            : base(callingModule, methodName, path, globalTabModuleISTA, elementNo)
        {
        }

        public override void CreateDialog(ParameterContainer inParam, ParameterContainer inoutParam)
        {
            Log.Info("IstaZeitCmd.CreateDialog()", "ISTA_Zeit init started.");
            base.Display = false;
        }

        public override void DoInvoke(string method, ParameterContainer inParam, ParameterContainer outParam, ParameterContainer inoutParam)
        {
            if (base.CallingModule == null)
            {
                Log.Error("IstaZeitCmd.Invoke()", "Failed to invoke method {0}, because calling module is null.", method);
                return;
            }
            ModuleParameter value = base.CallingModule.__RheinGoldCoreModuleParameters__.Clone();
            inParam.Parameter.Add("__RheinGoldCoreModuleParameters__", value);
            inParam.Parameter.Add("__RheinGoldTabModuleISTA__", base.CallingModule.GlobalTabModuleISTA);
            inParam.Parameter.Add("__RheinGoldSOCAccessor__", base.CallingModule.SOCAccessor);
            if ("Datum".Equals(method))
            {
                string Datum_String = string.Empty;
                int Datum_Jahr = 0;
                int Datum_Monat = 0;
                int Datum_Tag = 0;
                int Datum_JJJJMMTT = 0;
                new ISTA_Zeit(inParam).Datum(ref Datum_String, ref Datum_Jahr, ref Datum_Monat, ref Datum_Tag, ref Datum_JJJJMMTT);
                outParam.setParameter("Datum_String", Datum_String);
                outParam.setParameter("Datum_Jahr", Datum_Jahr);
                outParam.setParameter("Datum_Monat", Datum_Monat);
                outParam.setParameter("Datum_Tag", Datum_Tag);
                outParam.setParameter("Datum_JJJJMMTT", Datum_JJJJMMTT);
                return;
            }
            throw new ServiceDialogMethodUnsupportedException();
        }
    }
}
