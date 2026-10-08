using PsdzClient;
using System.Collections.Generic;
using System.ServiceModel;
using RheingoldPsdzWebApi.Adapter.Contracts.Model;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Ecu;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Exceptions;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Services
{
    [PreserveSource(AttributesModified = true)]
    [ServiceContract(SessionMode = SessionMode.Required)]
    [ServiceKnownType(typeof(PsdzConnection))]
    [ServiceKnownType(typeof(PsdzEcuIdentifier))]
    [ServiceKnownType(typeof(PsdzStandardSvt))]
    [ServiceKnownType(typeof(PsdzEcuContextInfo))]
    [ServiceKnownType(typeof(PsdzResponse))]
    [ServiceKnownType(typeof(PsdzSvt))]
    public interface IEcuService
    {
        [PreserveSource(KeepAttribute = true)]
        [OperationContract(Name = "RequestSvtFunctional")]
        [FaultContract(typeof(PsdzRuntimeException))]
        IPsdzStandardSvt RequestSvt(IPsdzConnection connection);
        [PreserveSource(KeepAttribute = true)]
        [OperationContract(Name = "RequestSvtFunctionalWithPhysicalRequest")]
        [FaultContract(typeof(PsdzRuntimeException))]
        IPsdzStandardSvt RequestSvt(IPsdzConnection connection, IEnumerable<IPsdzEcuIdentifier> installedEcus);
        [PreserveSource(KeepAttribute = true)]
        [OperationContract(Name = "RequestSvtFunctionalWithSmacs")]
        [FaultContract(typeof(PsdzRuntimeException))]
        IPsdzSvt RequestSvtWithSmacs(IPsdzConnection connection, IEnumerable<IPsdzEcuIdentifier> installedEcus);
        IPsdzSvt RequestSVTReference(IPsdzConnection connection, IEnumerable<IPsdzEcuIdentifier> installedEcus);
        [PreserveSource(KeepAttribute = true)]
        [OperationContract(Name = "RequestSVTwithSmAcAndMirror")]
        [FaultContract(typeof(PsdzRuntimeException))]
        IPsdzSvt RequestSVTwithSmAcAndMirror(IPsdzConnection connection, IEnumerable<IPsdzEcuIdentifier> installedEcus);
        [PreserveSource(KeepAttribute = true)]
        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        IEnumerable<IPsdzEcuContextInfo> RequestEcuContextInfos(IPsdzConnection connection, IEnumerable<IPsdzEcuIdentifier> installedEcus);
        [PreserveSource(KeepAttribute = true)]
        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        IPsdzResponse UpdatePiaPortierungsmaster(IPsdzConnection connection, IPsdzSvt svt);
    }
}