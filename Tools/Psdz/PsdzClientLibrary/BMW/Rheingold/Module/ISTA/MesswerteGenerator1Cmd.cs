using BMW.Rheingold.CoreFramework;

namespace BMW.Rheingold.Module.ISTA
{
    internal class MesswerteGenerator1Cmd : ServiceDialogCmdBase
    {
        public MesswerteGenerator1Cmd(ISTAModule callingModule, string methodName, string path, IModuleExecutionParent globalTabModuleISTA, int elementNo)
            : base(callingModule, methodName, path, globalTabModuleISTA, elementNo)
        {
        }

        public override void CreateDialog(ParameterContainer inParam, ParameterContainer inoutParam)
        {
            Log.Info("MesswerteGenerator1Cmd.CreateDialog()", "Messwerte_Generator1 init started.");
            base.Display = false;
        }

        public override void DoInvoke(string method, ParameterContainer inParam, ParameterContainer outParam, ParameterContainer inoutParam)
        {
            if (base.CallingModule == null)
            {
                Log.Error("MesswerteGenerator1Cmd.Invoke()", "Failed to invoke method {0}, because calling module is null.", method);
                return;
            }
            ModuleParameter value = base.CallingModule.__RheinGoldCoreModuleParameters__.Clone();
            inParam.Parameter.Add("__RheinGoldCoreModuleParameters__", value);
            inParam.Parameter.Add("__RheinGoldTabModuleISTA__", base.CallingModule.GlobalTabModuleISTA);
            inParam.Parameter.Add("__RheinGoldSOCAccessor__", base.CallingModule.SOCAccessor);
            if ("Zufallszahl".Equals(method))
            {
                int von = (int)inParam.getParameter("Von", 0);
                int bis = (int)inParam.getParameter("Bis", 0);
                int nachkomma = (int)inParam.getParameter("Nachkomma", 0);
                double Wert = 0.0;
                new Messwerte_Generator1(inParam).Zufallszahl(von, bis, nachkomma, ref Wert);
                outParam.setParameter("Wert", Wert);
                return;
            }
            if ("Zahlenbereich_hin_her".Equals(method))
            {
                int von2 = (int)inParam.getParameter("Von", 0);
                int bis2 = (int)inParam.getParameter("Bis", 0);
                int nachkomma2 = (int)inParam.getParameter("Nachkomma", 0);
                double schrittweite = (double)inParam.getParameter("Schrittweite", 0.0);
                double Wert2 = 0.0;
                new Messwerte_Generator1(inParam).Zahlenbereich_hin_her(von2, bis2, nachkomma2, schrittweite, ref Wert2);
                outParam.setParameter("Wert", Wert2);
                return;
            }
            throw new ServiceDialogMethodUnsupportedException();
        }
    }
}
