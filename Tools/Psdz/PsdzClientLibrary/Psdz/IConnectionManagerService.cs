using System;
using System.Collections.Generic;
using System.ServiceModel;
using BMW.Rheingold.Psdz.Client;
using BMW.Rheingold.Psdz.Model;
using BMW.Rheingold.Psdz.Model.Ecu;
using BMW.Rheingold.Psdz.Model.Exceptions;

namespace BMW.Rheingold.Psdz
{
    [ServiceContract(SessionMode = SessionMode.Required)]
    [ServiceKnownType(typeof(PsdzConnection))]
    [ServiceKnownType(typeof(PsdzConnectionVerboseResult))]
    public interface IConnectionManagerService
    {
        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        bool CheckConnection(IPsdzConnection connection);

        [OperationContract]
        [FaultContract(typeof(ArgumentException))]
        [FaultContract(typeof(PsdzRuntimeException))]
        IPsdzConnectionVerboseResult CheckConnectionVerbose(IPsdzConnection connection);

        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        void CloseConnection(IPsdzConnection connection);

        [OperationContract]
        [FaultContract(typeof(ArgumentException))]
        [FaultContract(typeof(PsdzRuntimeException))]
        IPsdzConnection ConnectOverBus(string project, string vehicleInfo, PsdzBus bus, InterfaceType interfaceType, string baureihe, string bauIstufe, bool tlsAllowed = false);

        [OperationContract]
        [FaultContract(typeof(ArgumentException))]
        [FaultContract(typeof(PsdzRuntimeException))]
        IPsdzConnection ConnectOverEthernet(string project, string vehicleInfo, string url, string baureihe, string bauIstufe, bool tlsAlloed = false);

        [OperationContract]
        [FaultContract(typeof(ArgumentException))]
        [FaultContract(typeof(PsdzRuntimeException))]
        IPsdzConnection ConnectOverIcom(string project, string vehicleInfo, string url, int additionalTransmissionTimeout, string baureihe, string bauIstufe, IcomConnectionType connectionType, bool shouldSetLinkPropertiesToDCan, bool tlsAlloed = false);

        [OperationContract]
        [FaultContract(typeof(ArgumentException))]
        [FaultContract(typeof(PsdzRuntimeException))]
        IPsdzConnection ConnectOverVin(string project, string vehicleInfo, string vin, string baureihe, string bauIstufe, bool tlsAlloed = false);

        [OperationContract]
        [FaultContract(typeof(ArgumentException))]
        [FaultContract(typeof(PsdzRuntimeException))]
        IPsdzConnection ConnectOverPtt(string project, string vehicleInfo, PsdzBus bus, string baureihe, string bauIstufe, bool tlsAlloed = false);

        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        void SetProdiasLogLevel(ProdiasLoglevel prodiasLoglevel);

        [OperationContract]
        void RequestShutdown();

        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        int GetHttpServerPort();
    }
}
