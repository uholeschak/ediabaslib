using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Psdz
{
    [ServiceKnownType(typeof(PsdzEcuContextInfo))]
    [ServiceKnownType(typeof(PsdzConnection))]
    [ServiceKnownType(typeof(PsdzSvt))]
    [ServiceKnownType(typeof(PsdzEcuIdentifier))]
    [ServiceKnownType(typeof(PsdzStandardSvt))]
    [ServiceKnownType(typeof(PsdzResponse))]
    [ServiceContract(SessionMode = SessionMode.Required)]
    public interface IEcuService
    {
        [OperationContract(Name = "RequestSvtFunctional")]
        [FaultContract(typeof(PsdzRuntimeException))]
        IPsdzStandardSvt RequestSvt(IPsdzConnection connection);

        [FaultContract(typeof(PsdzRuntimeException))]
        [OperationContract(Name = "RequestSvtFunctionalWithPhysicalRequest")]
        IPsdzStandardSvt RequestSvt(IPsdzConnection connection, IEnumerable<IPsdzEcuIdentifier> installedEcus);

        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        IEnumerable<IPsdzEcuContextInfo> RequestEcuContextInfos(IPsdzConnection connection, IEnumerable<IPsdzEcuIdentifier> installedEcus);

        [FaultContract(typeof(PsdzRuntimeException))]
        [OperationContract]
        IPsdzResponse UpdatePiaPortierungsmaster(IPsdzConnection connection, IPsdzSvt svt);
    }
}
