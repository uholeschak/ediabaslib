using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.Psdz.Model;

namespace BMW.Rheingold.Psdz.Client
{
    public enum InterfaceType
    {
        Vector,
        Omitec
    }

    public enum IcomConnectionType
    {
        Ip,
        DCan
    }

    public enum PsdzBus
    {
        ACan,
        BodyCan,
        DCan,
        FaCan,
        KCan,
        Unknown,
        Unbekannt,
        Most,
        FlexRay,
        B2Can,
        Ethernet,
        SCan,
        LeCan,
        LpCan,
        IkCan,
        SfCan,
        ZsgCan,
        ZgwCan,
        Le2Can,
        InfraCan,
        B3Can,
        SystemBusEthernet,
        AeCanFd,
        ApCanFd,
        FasCanFd,
        UssCanFd,
        SrrCanFd
    }

    public enum ProdiasLoglevel
    {
        OFF,
        INFO,
        DEBUG,
        TRACE,
        ERROR
    }

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

        public IPsdzConnection ConnectOverBus(string project, string vehicleInfo, PsdzBus bus, InterfaceType interfaceType, string baureihe, string bauIstufe)
        {
            return CallFunction((IConnectionManagerService m) => m.ConnectOverBus(project, vehicleInfo, bus, interfaceType, baureihe, bauIstufe));
        }

        public IPsdzConnection ConnectOverEthernet(string project, string vehicleInfo, string url, string baureihe, string bauIstufe)
        {
            return CallFunction((IConnectionManagerService m) => m.ConnectOverEthernet(project, vehicleInfo, url, baureihe, bauIstufe));
        }

        public IPsdzConnection ConnectOverIcom(string project, string vehicleInfo, string url, int additionalTransmissionTimeout, string baureihe, string bauIstufe, IcomConnectionType connectionType, bool shouldSetLinkPropertiesToDCan)
        {
            return CallFunction((IConnectionManagerService m) => m.ConnectOverIcom(project, vehicleInfo, url, additionalTransmissionTimeout, baureihe, bauIstufe, connectionType, shouldSetLinkPropertiesToDCan));
        }

        public IPsdzConnection ConnectOverVin(string project, string vehicleInfo, string vin, string baureihe, string bauIstufe)
        {
            return CallFunction((IConnectionManagerService m) => m.ConnectOverVin(project, vehicleInfo, vin, baureihe, bauIstufe));
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

        public IPsdzConnection ConnectOverPtt(string project, string vehicleInfo, PsdzBus bus, string baureihe, string bauIstufe)
        {
            return CallFunction((IConnectionManagerService m) => m.ConnectOverPtt(project, vehicleInfo, bus, baureihe, bauIstufe));
        }
    }
}
