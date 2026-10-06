using BMW.Rheingold.CoreFramework;
using BMW.Rheingold.CoreFramework.Contracts.Vehicle;

namespace BMW.Rheingold.Module.ISTA
{
    internal class IdentifikationstypCmd : ServiceDialogCmdBase
    {
        public IdentifikationstypCmd(ISTAModule callingModule, string methodName, string path, IModuleExecutionParent globalTabModuleISTA, int elementNo) : base(callingModule, methodName, path, globalTabModuleISTA, elementNo)
        {
        }

        public override void CreateDialog(ParameterContainer inParam, ParameterContainer inoutParam)
        {
            Log.Info("IdentifikationstypCmd.CreateDialog()", "Identifikationstyp init started.");
            Display = false;
        }

        public override void DoInvoke(string method, ParameterContainer inParam, ParameterContainer outParam, ParameterContainer inoutParam)
        {
            if (CallingModule == null)
            {
                Log.Error("IdentifikationstypCmd.DoInvoke()", "Failed to invoke method {0}, because calling module is null.", method);
                return;
            }

            Log.Info("IdentifikationstypCmd.DoInvoke()", "Identifikationstyp");
            ModuleParameter value = CallingModule.__RheinGoldCoreModuleParameters__.Clone();
            inParam.Parameter.Add("__RheinGoldCoreModuleParameters__", value);
            inParam.Parameter.Add("__RheinGoldTabModuleISTA__", CallingModule.GlobalTabModuleISTA);
            inParam.Parameter.Add("__RheinGoldSOCAccessor__", CallingModule.SOCAccessor);
            if ("Get_Ident_Typ".Equals(method))
            {
                int num = 0;
                switch (CallingModule.Vehicle.VehicleIdentLevel)
                {
                    case IdentificationLevel.None:
                    case IdentificationLevel.VehicleTypeNotLicensed:
                        num = 3;
                        break;
                    case IdentificationLevel.BasicFeatures:
                    case IdentificationLevel.VINOnly:
                    case IdentificationLevel.VINBasedFeatures:
                    case IdentificationLevel.VINBasedOnlineUpdated:
                        num = 1;
                        break;
                    case IdentificationLevel.VINVehicleReadout:
                    case IdentificationLevel.VINVehicleReadoutOnlineUpdated:
                        num = 2;
                        break;
                    default:
                        num = 0;
                        break;
                }

                outParam.setParameter("Typ", num);
                return;
            }

            throw new ServiceDialogMethodUnsupportedException();
        }
    }
}
