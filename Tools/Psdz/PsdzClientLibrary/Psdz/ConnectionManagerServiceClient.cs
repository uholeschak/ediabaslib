using BMW.Rheingold.Psdz.Model;
using BMW.Rheingold.Psdz.Model.Ecu;
using PsdzClient.Programming;
using PsdzClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;

namespace BMW.Rheingold.Psdz.Client
{
    [PreserveSource(Removed = true)]
    internal sealed class ConnectionManagerServiceClient : PsdzClientBase<IConnectionManagerService>, IConnectionManagerService
    {
        public ConnectionManagerServiceClient(Binding binding, EndpointAddress remoteAddress)
            : base(binding, remoteAddress)
        {
        }

        public bool CheckConnection(IPsdzConnection connection)
        {
            return CallFunction((IConnectionManagerService m) => m.CheckConnection(connection));
        }

        public IPsdzConnectionVerboseResult CheckConnectionVerbose(IPsdzConnection connection)
        {
            return CallFunction((IConnectionManagerService m) => m.CheckConnectionVerbose(connection));
        }

        public void CloseConnection(IPsdzConnection connection)
        {
            CallMethod(delegate (IConnectionManagerService m)
            {
                m.CloseConnection(connection);
            });
        }

        public IPsdzConnection ConnectOverBus(string project, string vehicleInfo, PsdzBus bus, InterfaceType interfaceType, string baureihe, string bauIstufe, bool tlsAllowed)
        {
            return CallFunction((IConnectionManagerService m) => m.ConnectOverBus(project, vehicleInfo, bus, interfaceType, baureihe, bauIstufe, tlsAllowed));
        }

        public IPsdzConnection ConnectOverEthernet(string project, string vehicleInfo, string url, string baureihe, string bauIstufe, bool tlsAllowed)
        {
            return CallFunction((IConnectionManagerService m) => m.ConnectOverEthernet(project, vehicleInfo, url, baureihe, bauIstufe, tlsAllowed));
        }

        public IPsdzConnection ConnectOverIcom(string project, string vehicleInfo, string url, int additionalTransmissionTimeout, string baureihe, string bauIstufe, IcomConnectionType connectionType, bool shouldSetLinkPropertiesToDCan, bool tlsAllowed)
        {
            return CallFunction((IConnectionManagerService m) => m.ConnectOverIcom(project, vehicleInfo, url, additionalTransmissionTimeout, baureihe, bauIstufe, connectionType, shouldSetLinkPropertiesToDCan, tlsAllowed));
        }

        public IPsdzConnection ConnectOverVin(string project, string vehicleInfo, string vin, string baureihe, string bauIstufe, bool tlsAllowed)
        {
            return CallFunction((IConnectionManagerService m) => m.ConnectOverVin(project, vehicleInfo, vin, baureihe, bauIstufe, tlsAllowed));
        }

        public void SetProdiasLogLevel(ProdiasLoglevel prodiasLoglevel)
        {
            CallMethod(delegate (IConnectionManagerService service)
            {
                service.SetProdiasLogLevel(prodiasLoglevel);
            });
        }

        public void RequestShutdown()
        {
            CallMethod(delegate (IConnectionManagerService service)
            {
                service.RequestShutdown();
            }, cacheChannel: false);
        }

        public int GetHttpServerPort()
        {
            return CallFunction((IConnectionManagerService service) => service.GetHttpServerPort());
        }

        public IPsdzConnection ConnectOverPtt(string project, string vehicleInfo, PsdzBus bus, string baureihe, string bauIstufe, bool tlsAllowed)
        {
            return CallFunction((IConnectionManagerService m) => m.ConnectOverPtt(project, vehicleInfo, bus, baureihe, bauIstufe, tlsAllowed));
        }
    }
}
