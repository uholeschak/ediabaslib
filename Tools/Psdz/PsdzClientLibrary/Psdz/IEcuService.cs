using System.Collections.Generic;
using System.ServiceModel;
using BMW.Rheingold.Psdz.Model;
using BMW.Rheingold.Psdz.Model.Ecu;
using BMW.Rheingold.Psdz.Model.Exceptions;

namespace BMW.Rheingold.Psdz
{
    [ServiceContract(SessionMode = SessionMode.Required)]
    [ServiceKnownType(typeof(PsdzConnection))]
    [ServiceKnownType(typeof(PsdzEcuIdentifier))]
    [ServiceKnownType(typeof(PsdzStandardSvt))]
    [ServiceKnownType(typeof(PsdzEcuContextInfo))]
    [ServiceKnownType(typeof(PsdzResponse))]
    [ServiceKnownType(typeof(PsdzSvt))]
    public interface IEcuService
    {
        [OperationContract(Name = "RequestSvtFunctional")]
        [FaultContract(typeof(PsdzRuntimeException))]
        IPsdzStandardSvt RequestSvt(IPsdzConnection connection);

        [OperationContract(Name = "RequestSvtFunctionalWithPhysicalRequest")]
        [FaultContract(typeof(PsdzRuntimeException))]
        IPsdzStandardSvt RequestSvt(IPsdzConnection connection, IEnumerable<IPsdzEcuIdentifier> installedEcus);

        [OperationContract(Name = "RequestSvtFunctionalWithSmacs")]
        [FaultContract(typeof(PsdzRuntimeException))]
        IPsdzSvt RequestSvtWithSmacs(IPsdzConnection connection, IEnumerable<IPsdzEcuIdentifier> installedEcus);

        [OperationContract(Name = "RequestSVTwithSmAcAndMirror")]
        [FaultContract(typeof(PsdzRuntimeException))]
        IPsdzSvt RequestSVTwithSmAcAndMirror(IPsdzConnection connection, IEnumerable<IPsdzEcuIdentifier> installedEcus);

        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        IEnumerable<IPsdzEcuContextInfo> RequestEcuContextInfos(IPsdzConnection connection, IEnumerable<IPsdzEcuIdentifier> installedEcus);

        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        IPsdzResponse UpdatePiaPortierungsmaster(IPsdzConnection connection, IPsdzSvt svt);
    }
}
