using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using PsdzClient.Core;
using PsdzClient.Core.Container;
using System;
using System.Linq;

namespace BMW.Rheingold.Psdz
{
    public abstract class ActiveGatewayUtils
    {
        public static bool WriteRoutingTable(IEcuKom ecuKom, IVehicle vehicle, long? ecuDiagAddr)
        {
            if (ecuKom == null)
            {
                throw new ArgumentNullException("ecuKom");
            }
            if (vehicle == null)
            {
                throw new ArgumentNullException("vehicle");
            }
            if (ecuDiagAddr.HasValue)
            {
                Log.Info("ActiveGatewayUtils.WriteRoutingTable()", "Write ECU-specific (DiagAddr: 0x{0:X2}) routing table to adapter...", ecuDiagAddr);
            }
            else
            {
                Log.Info("ActiveGatewayUtils.WriteRoutingTable()", "Write default routing table to adapter...");
            }
            int num = 0;
            int num2 = vehicle.ECU.Count() + 2;
            string param = string.Format("0xEF;00;00;00;{0}", ecuDiagAddr.HasValue ? "0x00" : "0x01");
            num += (SteuernAddDiagRouting(ecuKom, param) ? 1 : 0);
            param = string.Format("0xDF;00;00;00;{0}", ecuDiagAddr.HasValue ? "0x00" : "0x02");
            num += (SteuernAddDiagRouting(ecuKom, param) ? 1 : 0);
            if (vehicle.ECU != null)
            {
                foreach (IEcu item in vehicle.ECU)
                {
                    param = item.ID_SG_ADR + ";00;00;00;";
                    param = ((ecuDiagAddr.HasValue && ecuDiagAddr != item.ID_SG_ADR) ? (param + "0x00") : (param + ((item.DiagProtocoll == typeDiagProtocoll.UDS) ? "0x02" : "0x01")));
                    num += (SteuernAddDiagRouting(ecuKom, param) ? 1 : 0);
                }
            }
            if (num == num2)
            {
                Log.Info("ActiveGatewayUtils.WriteRoutingTable()", "Routing table successfully written!");
                return true;
            }
            Log.Error("ActiveGatewayUtils.WriteRoutingTable()", "Errors occurred while writing routing table!");
            return false;
        }

        public static bool WriteRoutingTableBn2020(IEcuKom ecuKom, IVehicle vehicle)
        {
            if (ecuKom == null)
            {
                throw new ArgumentNullException("ecuKom");
            }
            if (vehicle == null)
            {
                throw new ArgumentNullException("vehicle");
            }
            Log.Info("ActiveGatewayUtils.WriteRoutingTableBn2020()", "Write BN2020-specific routing table to adapter");
            int num = 0;
            int num2 = vehicle.ECU.Count() + 2;
            num += (SteuernAddDiagRouting(ecuKom, "0xEF;00;00;00;0x00") ? 1 : 0);
            num += (SteuernAddDiagRouting(ecuKom, "0xDF;00;00;0x01;0x02") ? 1 : 0);
            if (vehicle.ECU != null)
            {
                foreach (IEcu item in vehicle.ECU)
                {
                    string param = string.Format("{0};00;00;00;{1}", item.ID_SG_ADR, (item.DiagProtocoll == typeDiagProtocoll.UDS) ? "0x02" : "0x00");
                    num += (SteuernAddDiagRouting(ecuKom, param) ? 1 : 0);
                }
            }
            if (num == num2)
            {
                Log.Info("ActiveGatewayUtils.WriteRoutingTableBn2020()", "Routing table successfully written!");
                return true;
            }
            Log.Error("ActiveGatewayUtils.WriteRoutingTableBn2020()", "Errors occurred while writing routing table!");
            return false;
        }

        private static bool SteuernAddDiagRouting(IEcuKom ecuKom, string param)
        {
            IEcuJob ecuJob = ecuKom.ApiJob("g_zgw", "_steuern_add_diag_routing", param, string.Empty, 2);
            Log.Debug("ActiveGatewayUtils.SteuernAddDiagRouting()", "Setting up routing with param: {0} - JOB_STATUS: {1}", param, ecuJob.getStringResult("JOB_STATUS") ?? "<None>");
            return ecuJob.IsOkay();
        }
    }
}