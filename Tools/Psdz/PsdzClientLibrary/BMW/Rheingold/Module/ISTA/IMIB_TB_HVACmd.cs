using BMW.Rheingold.CoreFramework;
using System.Collections.Generic;

namespace BMW.Rheingold.Module.ISTA
{
    internal class IMIB_TB_HVACmd : ServiceDialogCmdBase
    {
        public IMIB_TB_HVACmd(ISTAModule callingModule, string methodName, string path, IModuleExecutionParent globalTabModuleISTA, int elementNo) : base(callingModule, methodName, path, globalTabModuleISTA, elementNo)
        {
        }

        public override void CreateDialog(ParameterContainer inParam, ParameterContainer inoutParam)
        {
            Log.Info("IMIB_TB_HVACmd.CreateDialog()", "IMIB_TB_HVA init started.");
            Display = false;
        }

        public override void DoInvoke(string method, ParameterContainer inParam, ParameterContainer outParam, ParameterContainer inoutParam)
        {
            if (CallingModule == null)
            {
                Log.Error("IMIB_TB_HVACmd.DoInvoke()", "Failed to invoke method {0}, because calling module is null.", method);
                return;
            }

            ModuleParameter value = CallingModule.__RheinGoldCoreModuleParameters__.Clone();
            inParam.Parameter.Add("__RheinGoldCoreModuleParameters__", value);
            inParam.Parameter.Add("__RheinGoldTabModuleISTA__", CallingModule.GlobalTabModuleISTA);
            inParam.Parameter.Add("__RheinGoldSOCAccessor__", CallingModule.SOCAccessor);
            if ("a_Laden".Equals(method))
            {
                int Ergebnis = 0;
                new IMIB_TB_HVA(inParam).a_Laden(ref Ergebnis);
                outParam.setParameter("Ergebnis", Ergebnis);
            }
            else if ("b_Start".Equals(method))
            {
                new IMIB_TB_HVA(inParam).b_Start();
            }
            else if ("c_Lesen".Equals(method))
            {
                List<string> ErgebnisListe = null;
                double Status = 0.0;
                double Wert = 0.0;
                double Wert2 = 0.0;
                double Wert3 = 0.0;
                double Wert4 = 0.0;
                double Wert5 = 0.0;
                double Wert6 = 0.0;
                double Drehzahl = 0.0;
                new IMIB_TB_HVA(inParam).c_Lesen(ref ErgebnisListe, ref Status, ref Wert, ref Wert2, ref Wert3, ref Wert4, ref Wert5, ref Wert6, ref Drehzahl);
                outParam.setParameter("ErgebnisListe", ErgebnisListe);
                outParam.setParameter("Status", Status);
                outParam.setParameter("Wert1", Wert);
                outParam.setParameter("Wert2", Wert2);
                outParam.setParameter("Wert3", Wert3);
                outParam.setParameter("Wert4", Wert4);
                outParam.setParameter("Wert5", Wert5);
                outParam.setParameter("Wert6", Wert6);
                outParam.setParameter("Drehzahl", Drehzahl);
            }
            else if ("d_Stop".Equals(method))
            {
                new IMIB_TB_HVA(inParam).d_Stop();
            }
            else
            {
                if (!"e_Entladen".Equals(method))
                {
                    throw new ServiceDialogMethodUnsupportedException();
                }

                new IMIB_TB_HVA(inParam).e_Entladen();
            }
        }
    }
}
