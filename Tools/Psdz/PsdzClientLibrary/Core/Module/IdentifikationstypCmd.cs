using BMW.Rheingold.CoreFramework;
using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using PsdzClient.Core;
using PsdzClient.Core.Container;

namespace BMW.Rheingold.Module.ISTA
{
    internal class IdentifikationstypCmd : ServiceDialogCmdBase
    {
        public IdentifikationstypCmd(ISTAModule callingModule, string methodName, string path, IModuleExecutionParent globalTabModuleISTA, int elementNo)
            : base(callingModule, methodName, path, globalTabModuleISTA, elementNo)
        {
        }

        public override void CreateDialog(ParameterContainer inParam, ParameterContainer inoutParam)
        {
            Log.Info("IdentifikationstypCmd.CreateDialog()", "Identifikationstyp init started.");
            base.Display = false;
        }

        public override void DoInvoke(string method, ParameterContainer inParam, ParameterContainer outParam, ParameterContainer inoutParam)
        {
            if (base.CallingModule == null)
            {
                Log.Error("IdentifikationstypCmd.DoInvoke()", "Failed to invoke method {0}, because calling module is null.", method);
                return;
            }
            Log.Info("IdentifikationstypCmd.DoInvoke()", "Identifikationstyp");
            ModuleParameter value = base.CallingModule.__RheinGoldCoreModuleParameters__.Clone();
            inParam.Parameter.Add("__RheinGoldCoreModuleParameters__", value);
            inParam.Parameter.Add("__RheinGoldTabModuleISTA__", base.CallingModule.GlobalTabModuleISTA);
            inParam.Parameter.Add("__RheinGoldSOCAccessor__", base.CallingModule.SOCAccessor);
            if ("Get_Ident_Typ".Equals(method))
            {
                int num = 0;
                switch (base.CallingModule.Vehicle.VehicleIdentLevel)
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
