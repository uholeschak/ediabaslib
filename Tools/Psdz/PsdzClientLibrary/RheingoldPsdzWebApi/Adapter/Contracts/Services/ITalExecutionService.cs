using PsdzClient;
using System;
using System.IO;
using System.ServiceModel;
using System.Threading;
using RheingoldPsdzWebApi.Adapter.Contracts.Model;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Exceptions;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.SecureCoding;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Sfa;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Tal;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Services
{
    [PreserveSource(AttributesModified = true)]
    [ServiceContract(SessionMode = SessionMode.Required, CallbackContract = typeof(IPsdzProgressListener))]
    [ServiceKnownType(typeof(PsdzConnection))]
    [ServiceKnownType(typeof(PsdzFa))]
    [ServiceKnownType(typeof(PsdzSvt))]
    [ServiceKnownType(typeof(PsdzVin))]
    [ServiceKnownType(typeof(PsdzTal))]
    [ServiceKnownType(typeof(PsdzSecureCodingConfigCto))]
    [ServiceKnownType(typeof(PsdzProgrammingProtectionDataCto))]
    public interface ITalExecutionService : ILifeCycleDependencyProvider
    {
        [PreserveSource(KeepAttribute = true)]
        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        IPsdzTal ExecuteTal(IPsdzConnection connection, IPsdzTal tal, IPsdzSvt svtTarget, IPsdzVin vin, IPsdzFa faTarget, TalExecutionSettings talExecutionConfig, string backupDataPath, CancellationToken ct);

        [PreserveSource(KeepAttribute = true)]
        [OperationContract]
        [FaultContract(typeof(ArgumentException))]
        [FaultContract(typeof(FileNotFoundException))]
        [FaultContract(typeof(PsdzRuntimeException))]
        IPsdzTal ExecuteTalFile(IPsdzConnection connection, string pathToTal, string vin, string pathToFa, TalExecutionSettings talExecutionSettings, CancellationToken ct);

        [PreserveSource(KeepAttribute = true)]
        [OperationContract]
        [FaultContract(typeof(ArgumentException))]
        [FaultContract(typeof(PsdzRuntimeException))]
        IPsdzTal ExecuteHDDUpdate(IPsdzConnection connection, IPsdzTal tal, IPsdzFa fa, IPsdzVin vin, TalExecutionSettings settings);
    }
}
