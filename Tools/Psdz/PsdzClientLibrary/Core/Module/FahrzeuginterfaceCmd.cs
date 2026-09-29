using BMW.Rheingold.CoreFramework;
using BMW.Rheingold.CoreFramework.Contracts.ConnectionManagement;
using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using BMW.Rheingold.Module.ISTA;
using PsdzClient.Core;
using PsdzClient.Core.Container;
using System.Collections.Generic;
using System.Linq;
using BMW.Rheingold.CoreFramework.DatabaseProvider;

namespace BMW.Rheingold.Module.ISTA
{
    internal class FahrzeuginterfaceCmd : ServiceDialogCmdBase
    {
        public FahrzeuginterfaceCmd(ISTAModule callingModule, string methodName, string path, IModuleExecutionParent globalTabModuleISTA, int elementNo)
            : base(callingModule, methodName, path, globalTabModuleISTA, elementNo)
        {
        }

        public override void CreateDialog(ParameterContainer inParam, ParameterContainer inoutParam)
        {
            Log.Info("FahrzeuginterfaceCmd.CreateDialog()", "Fahrzeuginterface init started.");
            base.Display = false;
        }

        private bool GetConnectionStatus()
        {
            ILogic logic = base.CallingModule.__RheinGoldCoreModuleParameters__.getParameter(ModuleParameter.ParameterName.Logic) as ILogic;
            bool result;
            switch (base.CallingModule.Vehicle.VCI.VCIType)
            {
                case VCIDeviceType.ICOM:
                    result = logic.VecInfo.VCI.IsConnected;
                    Log.Info(Log.CurrentMethod(), $"Connection state ICOM: {logic.VecInfo.VCI.IsConnected}; isDead: {logic.VecInfo.VCI.IsDead}");
                    Log.Info(Log.CurrentMethod(), $"Connection state CallingModule ICOM: {0}; isDead: {1}", base.CallingModule.Vehicle.VCI.IsConnected, base.CallingModule.Vehicle.VCI.IsDead);
                    break;
                case VCIDeviceType.PTT:
                    result = logic.VecInfo.VCI.IsConnected;
                    Log.Info(Log.CurrentMethod(), $"Connection state PTT: {logic.VecInfo.VCI.IsConnected}; isDead: {logic.VecInfo.VCI.IsDead}");
                    Log.Info(Log.CurrentMethod(), $"Connection state CallingModule PTT: {base.CallingModule.Vehicle.VCI.IsConnected}; isDead: {base.CallingModule.Vehicle.VCI.IsDead}");
                    break;
                case VCIDeviceType.SIM:
                case VCIDeviceType.INFOSESSION:
                case VCIDeviceType.UNKNOWN:
                    result = false;
                    break;
                default:
                    result = true;
                    break;
            }
            return result;
        }

        public override void DoInvoke(string method, ParameterContainer inParam, ParameterContainer outParam, ParameterContainer inoutParam)
        {
            if (base.CallingModule == null)
            {
                Log.Warning("ServiceDialog.DoInvoke()", "Fahrzeuginterface: callingModule was null.");
            }
            else if ("Verbindungsstatus".Equals(method))
            {
                bool connectionStatus = GetConnectionStatus();
                outParam.setParameter("Verbunden", connectionStatus);
                outParam.setParameter("Verbindungsstatus", connectionStatus ? 1 : 0);
            }
            else if ("Verbinden".Equals(method))
            {
                ILogic logic = base.CallingModule.__RheinGoldCoreModuleParameters__.getParameter(ModuleParameter.ParameterName.Logic) as ILogic;
                Vehicle vehicle = logic?.VecInfo;
                if (vehicle == null)
                {
                    outParam.setParameter("erfolgreich", false);
                    return;
                }
                CallConnectionManager(logic, vehicle, ConnectionTargetTypes.VCI);
                outParam.setParameter("erfolgreich", GetConnectionStatus());
            }
            else if ("Trennen".Equals(method))
            {
                if (base.CallingModule.__RheinGoldCoreModuleParameters__.getParameter(ModuleParameter.ParameterName.Logic) is ILogic logic2)
                {
                    if (!logic2.ActivateKL15())
                    {
                        IList<string> lang = new string[1] { ConfigSettings.CurrentUICulture }.ToList();
                        IList<LocalizedText> titleList = new FormatedData("#Info").Localize(lang);
                        IList<LocalizedText> msgList = new FormatedData("#VCILoss.WarningToBattery").Localize(lang);
                        logic2.Services.InteractionService.RegisterMessage(titleList, msgList);
                    }
                    logic2.SwitchToInfoSession();
                }
                outParam.setParameter("erfolgreich", !GetConnectionStatus());
            }
            else if ("Verbindungsdetails".Equals(method))
            {
                bool connectionStatus2 = GetConnectionStatus();
                outParam.setParameter("Verbindungszustand", connectionStatus2 ? 1 : (-1));
            }
            else if ("IMIB_Trennen".Equals(method))
            {
                if (base.MeasurmentService.IsConnectedToImib)
                {
                    base.MeasurmentService.DisconnectImib(force: true);
                }
                outParam.setParameter("erfolgreich", !base.MeasurmentService.IsConnectedToImib);
            }
            else
            {
                Log.Error("FahrzeuginterfaceCmd.DoInvoke()", "Unsupported method {0} will be ignored.", method);
            }
        }
    }
}
