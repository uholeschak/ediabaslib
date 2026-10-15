using PsdzClient;
using System.Collections.Generic;
using System.ServiceModel;
using RheingoldPsdzWebApi.Adapter.Contracts.Model;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Ecu;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Exceptions;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.SecurityManagement;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Sfa;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Services
{
    [PreserveSource(AttributesModified = true)]
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
        [PreserveSource(KeepAttribute = true)]
        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        IPsdzReadEcuUidResultCto readEcuUid(IPsdzConnection connection, IEnumerable<IPsdzEcuIdentifier> ecus, IPsdzSvt svt);

        [PreserveSource(KeepAttribute = true)]
        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        IPsdzTargetBitmask GenerateIPSecTargetBitmask(IPsdzConnection connection, IPsdzSvt svt);

        [PreserveSource(KeepAttribute = true)]
        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        IEnumerable<IPsdzEcuIdentifier> GenerateECUlistWithIPsecBitmasksDiffering(IPsdzConnection connection, byte[] targetBm, IDictionary<IPsdzEcuIdentifier, byte[]> ecuBms);

        [PreserveSource(KeepAttribute = true)]
        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        IEnumerable<IPsdzEcuFailureResponseCto> WriteIPsecBitmasks(IPsdzConnection connection, IEnumerable<IPsdzEcuIdentifier> ecus, byte[] targetBm, IPsdzSvt svt);

        [PreserveSource(KeepAttribute = true)]
        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        IPsdzIPsecEcuBitmaskResultCto ReadIPsecBitmasks(IPsdzConnection connection, IEnumerable<IPsdzEcuIdentifier> ecus, IPsdzSvt svt);

        [PreserveSource(KeepAttribute = true)]
        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        IEnumerable<IPsdzEcuIdentifier> GetIPsecEnabledECUs(IPsdzSvt svt);
    }
}
