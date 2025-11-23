using BMW.Rheingold.Psdz.Client;
using BMW.Rheingold.Psdz.Model;
using BMW.Rheingold.Psdz.Model.Ecu;
using BMW.Rheingold.Psdz.Model.Exceptions;
using PsdzClient;
using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace BMW.Rheingold.Psdz
{
    [PreserveSource(AttributesModified = true)]
    [ServiceContract(SessionMode = SessionMode.Required)]
    [ServiceKnownType(typeof(PsdzConnection))]
    [ServiceKnownType(typeof(PsdzConnectionVerboseResult))]
    public interface IConnectionManagerService
    {
        [PreserveSource(KeepAttribute = true)]
        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        bool CheckConnection(IPsdzConnection connection);

        [PreserveSource(KeepAttribute = true)]
        [OperationContract]
        [FaultContract(typeof(ArgumentException))]
        [FaultContract(typeof(PsdzRuntimeException))]
        IPsdzConnectionVerboseResult CheckConnectionVerbose(IPsdzConnection connection);

        [PreserveSource(KeepAttribute = true)]
        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        void CloseConnection(IPsdzConnection connection);

        [PreserveSource(KeepAttribute = true)]
        [OperationContract]
        [FaultContract(typeof(ArgumentException))]
        [FaultContract(typeof(PsdzRuntimeException))]
        IPsdzConnection ConnectOverBus(string project, string vehicleInfo, PsdzBus bus, InterfaceType interfaceType, string baureihe, string bauIstufe, bool tlsAllowed = false);

        [PreserveSource(KeepAttribute = true)]
        [OperationContract]
        [FaultContract(typeof(ArgumentException))]
        [FaultContract(typeof(PsdzRuntimeException))]
        IPsdzConnection ConnectOverEthernet(string project, string vehicleInfo, string url, string baureihe, string bauIstufe, bool tlsAlloed = false);

        [PreserveSource(KeepAttribute = true)]
        [OperationContract]
        [FaultContract(typeof(ArgumentException))]
        [FaultContract(typeof(PsdzRuntimeException))]
        IPsdzConnection ConnectOverIcom(string project, string vehicleInfo, string url, int additionalTransmissionTimeout, string baureihe, string bauIstufe, IcomConnectionType connectionType, bool shouldSetLinkPropertiesToDCan, bool tlsAlloed = false);

        [PreserveSource(KeepAttribute = true)]
        [OperationContract]
        [FaultContract(typeof(ArgumentException))]
        [FaultContract(typeof(PsdzRuntimeException))]
        IPsdzConnection ConnectOverVin(string project, string vehicleInfo, string vin, string baureihe, string bauIstufe, bool tlsAlloed = false);

        [PreserveSource(KeepAttribute = true)]
        [OperationContract]
        [FaultContract(typeof(ArgumentException))]
        [FaultContract(typeof(PsdzRuntimeException))]
        IPsdzConnection ConnectOverPtt(string project, string vehicleInfo, PsdzBus bus, string baureihe, string bauIstufe, bool tlsAlloed = false);

        [PreserveSource(KeepAttribute = true)]
        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        void SetProdiasLogLevel(ProdiasLoglevel prodiasLoglevel);

        [PreserveSource(KeepAttribute = true)]
        [OperationContract]
        void RequestShutdown();

        [PreserveSource(KeepAttribute = true)]
        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        int GetHttpServerPort();
    }
}
