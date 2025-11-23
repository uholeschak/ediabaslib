using BMW.Rheingold.Psdz.Model;
using BMW.Rheingold.Psdz.Model.Ecu;
using BMW.Rheingold.Psdz.Model.Exceptions;
using PsdzClient;
using System.Collections.Generic;
using System.ServiceModel;

namespace BMW.Rheingold.Psdz
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
        [PreserveSource(AttributesModified = true)]
        [OperationContract(Name = "RequestSvtFunctional")]
        [FaultContract(typeof(PsdzRuntimeException))]
        IPsdzStandardSvt RequestSvt(IPsdzConnection connection);

        [PreserveSource(AttributesModified = true)]
        [OperationContract(Name = "RequestSvtFunctionalWithPhysicalRequest")]
        [FaultContract(typeof(PsdzRuntimeException))]
        IPsdzStandardSvt RequestSvt(IPsdzConnection connection, IEnumerable<IPsdzEcuIdentifier> installedEcus);

        [PreserveSource(AttributesModified = true)]
        [OperationContract(Name = "RequestSvtFunctionalWithSmacs")]
        [FaultContract(typeof(PsdzRuntimeException))]
        IPsdzSvt RequestSvtWithSmacs(IPsdzConnection connection, IEnumerable<IPsdzEcuIdentifier> installedEcus);

        [PreserveSource(AttributesModified = true)]
        [OperationContract(Name = "RequestSVTwithSmAcAndMirror")]
        [FaultContract(typeof(PsdzRuntimeException))]
        IPsdzSvt RequestSVTwithSmAcAndMirror(IPsdzConnection connection, IEnumerable<IPsdzEcuIdentifier> installedEcus);

        [PreserveSource(AttributesModified = true)]
        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        IEnumerable<IPsdzEcuContextInfo> RequestEcuContextInfos(IPsdzConnection connection, IEnumerable<IPsdzEcuIdentifier> installedEcus);

        [PreserveSource(AttributesModified = true)]
        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        IPsdzResponse UpdatePiaPortierungsmaster(IPsdzConnection connection, IPsdzSvt svt);
    }
}
