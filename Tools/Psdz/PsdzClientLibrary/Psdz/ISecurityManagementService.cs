using System.Collections.Generic;
using System.ServiceModel;
using BMW.Rheingold.Psdz.Model;
using BMW.Rheingold.Psdz.Model.Ecu;
using BMW.Rheingold.Psdz.Model.Exceptions;
using BMW.Rheingold.Psdz.Model.SecurityManagement;
using BMW.Rheingold.Psdz.Model.Sfa;

namespace BMW.Rheingold.Psdz
{
    [ServiceContract(SessionMode = SessionMode.Required, CallbackContract = typeof(IPsdzProgressListener))]
    [ServiceKnownType(typeof(PsdzReadEcuUidResultCto))]
    [ServiceKnownType(typeof(PsdzConnection))]
    [ServiceKnownType(typeof(PsdzEcuIdentifier))]
    [ServiceKnownType(typeof(PsdzSvt))]
    [ServiceKnownType(typeof(PsdzTargetBitmask))]
    [ServiceKnownType(typeof(PsdzIPsecEcuBitmaskResultCto))]
    [ServiceKnownType(typeof(PsdzEcuFailureResponseCto))]
    public interface ISecurityManagementService
    {
        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        IPsdzReadEcuUidResultCto readEcuUid(IPsdzConnection connection, IEnumerable<IPsdzEcuIdentifier> ecus, IPsdzSvt svt);

        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        IPsdzTargetBitmask GenerateIPSecTargetBitmask(IPsdzConnection connection, IPsdzSvt svt);

        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        IEnumerable<IPsdzEcuIdentifier> GenerateECUlistWithIPsecBitmasksDiffering(IPsdzConnection connection, byte[] targetBm, IDictionary<IPsdzEcuIdentifier, byte[]> ecuBms);

        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        IEnumerable<IPsdzEcuFailureResponseCto> WriteIPsecBitmasks(IPsdzConnection connection, IEnumerable<IPsdzEcuIdentifier> ecus, byte[] targetBm, IPsdzSvt svt);

        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        IPsdzIPsecEcuBitmaskResultCto ReadIPsecBitmasks(IPsdzConnection connection, IEnumerable<IPsdzEcuIdentifier> ecus, IPsdzSvt svt);

        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        IEnumerable<IPsdzEcuIdentifier> GetIPsecEnabledECUs(IPsdzSvt svt);
    }
}
