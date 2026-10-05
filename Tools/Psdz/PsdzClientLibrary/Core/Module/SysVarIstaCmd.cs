using BMW.Rheingold.CoreFramework;
using BMW.Rheingold.Module.ISTA;
using PsdzClient.Core;
using PsdzClient.Core.Container;

namespace BMW.Rheingold.Module.ISTA
{
    internal class SysVarIstaCmd : ServiceDialogCmdBase
    {
        public SysVarIstaCmd(ISTAModule callingModule, string methodName, string path, IModuleExecutionParent globalTabModuleISTA, int elementNo) : base(callingModule, methodName, path, globalTabModuleISTA, elementNo)
        {
        }

        public override void CreateDialog(ParameterContainer inParam, ParameterContainer inoutParam)
        {
            Log.Info("SysVarIstaCmd.CreateDialog()", "SYS_VAR_ISTA init started.");
            Display = false;
        }

        public override void DoInvoke(string method, ParameterContainer inParam, ParameterContainer outParam, ParameterContainer inoutParam)
        {
            if (CallingModule == null)
            {
                Log.Error("SysVarIstaCmd.DoInvoke()", "Failed to invoke method {0}, because calling module is null.", method);
                return;
            }

            ModuleParameter value = CallingModule.__RheinGoldCoreModuleParameters__.Clone();
            inParam.Parameter.Add("__RheinGoldCoreModuleParameters__", value);
            inParam.Parameter.Add("__RheinGoldTabModuleISTA__", CallingModule.GlobalTabModuleISTA);
            inParam.Parameter.Add("__RheinGoldSOCAccessor__", CallingModule.SOCAccessor);
            switch (method)
            {
                case "InitializeDialog":
                {
                    string[] ISTA_Systemvariable = new string[30];
                    if (inoutParam.getParameter("ISTA_Systemvariable") != null)
                    {
                        ISTA_Systemvariable = (string[])inoutParam.getParameter("Typmerkmale");
                    }

                    new SYS_VAR_ISTA(inParam).InitializeDialog(ref ISTA_Systemvariable);
                    inoutParam.Parameter.Add("ISTA_Systemvariable", ISTA_Systemvariable);
                    break;
                }

                case "DatumTester":
                {
                    string PDatumTester = string.Empty;
                    new SYS_VAR_ISTA(inParam).DatumTester(ref PDatumTester);
                    outParam.setParameter("PDatumTester", PDatumTester);
                    break;
                }

                case "LandTester":
                {
                    string PLandTester = string.Empty;
                    new SYS_VAR_ISTA(inParam).LandTester(ref PLandTester);
                    outParam.setParameter("PLandTester", PLandTester);
                    break;
                }

                case "UhrzeitTester":
                {
                    string PUhrzeitTester = string.Empty;
                    new SYS_VAR_ISTA(inParam).UhrzeitTester(ref PUhrzeitTester);
                    outParam.setParameter("PUhrzeitTester", PUhrzeitTester);
                    break;
                }

                default:
                    Log.Error("SysVarIstaCmd.DoInvoke()", "Unsupported method {0} will be ignored.", method);
                    break;
            }
        }
    }
}