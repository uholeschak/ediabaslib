using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient
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

	class ConnectionManagerServiceClient : PsdzClientBase<IConnectionManagerService>, IConnectionManagerService
	{
		public ConnectionManagerServiceClient(Binding binding, EndpointAddress remoteAddress) : base(binding, remoteAddress)
		{
		}

		public bool CheckConnection(IPsdzConnection connection)
		{
			return base.CallFunction<bool>((IConnectionManagerService m) => m.CheckConnection(connection));
		}

		public void CloseConnection(IPsdzConnection connection)
		{
			base.CallMethod(delegate (IConnectionManagerService m)
			{
				m.CloseConnection(connection);
			}, true);
		}

		public IPsdzConnection ConnectOverBus(string project, string vehicleInfo, PsdzBus bus, InterfaceType interfaceType, string baureihe, string bauIstufe)
		{
			return base.CallFunction<IPsdzConnection>((IConnectionManagerService m) => m.ConnectOverBus(project, vehicleInfo, bus, interfaceType, baureihe, bauIstufe));
		}

		public IPsdzConnection ConnectOverEthernet(string project, string vehicleInfo, string url, string baureihe, string bauIstufe)
		{
			return base.CallFunction<IPsdzConnection>((IConnectionManagerService m) => m.ConnectOverEthernet(project, vehicleInfo, url, baureihe, bauIstufe));
		}

		public IPsdzConnection ConnectOverIcom(string project, string vehicleInfo, string url, int additionalTransmissionTimeout, string baureihe, string bauIstufe, IcomConnectionType connectionType, bool shouldSetLinkPropertiesToDCan)
		{
			return base.CallFunction<IPsdzConnection>((IConnectionManagerService m) => m.ConnectOverIcom(project, vehicleInfo, url, additionalTransmissionTimeout, baureihe, bauIstufe, connectionType, shouldSetLinkPropertiesToDCan));
		}

		public IPsdzConnection ConnectOverVin(string project, string vehicleInfo, string vin, string baureihe, string bauIstufe)
		{
			return base.CallFunction<IPsdzConnection>((IConnectionManagerService m) => m.ConnectOverVin(project, vehicleInfo, vin, baureihe, bauIstufe));
		}

		public void SetProdiasLogLevel(ProdiasLoglevel prodiasLoglevel)
		{
			base.CallMethod(delegate (IConnectionManagerService service)
			{
				service.SetProdiasLogLevel(prodiasLoglevel);
			}, true);
		}

		public void RequestShutdown()
		{
			base.CallMethod(delegate (IConnectionManagerService service)
			{
				service.RequestShutdown();
			}, false);
		}

		public int GetHttpServerPort()
		{
			return base.CallFunction<int>((IConnectionManagerService service) => service.GetHttpServerPort());
		}

		public IPsdzConnection ConnectOverPtt(string project, string vehicleInfo, PsdzBus bus, string baureihe, string bauIstufe)
		{
			return base.CallFunction<IPsdzConnection>((IConnectionManagerService m) => m.ConnectOverPtt(project, vehicleInfo, bus, baureihe, bauIstufe));
		}
	}
}
