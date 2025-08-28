using System;
using System.IO;
using System.ServiceModel;
using System.Threading;
using BMW.Rheingold.Psdz.Model;
using BMW.Rheingold.Psdz.Model.Exceptions;
using BMW.Rheingold.Psdz.Model.SecureCoding;
using BMW.Rheingold.Psdz.Model.Tal;

namespace BMW.Rheingold.Psdz
{
    [ServiceContract(SessionMode = SessionMode.Required, CallbackContract = typeof(IPsdzProgressListener))]
    [ServiceKnownType(typeof(PsdzConnection))]
    [ServiceKnownType(typeof(PsdzFa))]
    [ServiceKnownType(typeof(PsdzSvt))]
    [ServiceKnownType(typeof(PsdzVin))]
    [ServiceKnownType(typeof(PsdzTal))]
    [ServiceKnownType(typeof(PsdzSecureCodingConfigCto))]
    [ServiceKnownType(typeof(PsdzProgrammingProtectionDataCto))]
    public interface ITalExecutionService
    {
        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        IPsdzTal ExecuteTal(IPsdzConnection connection, IPsdzTal tal, IPsdzSvt svtTarget, IPsdzVin vin, IPsdzFa faTarget, TalExecutionSettings talExecutionConfig, string backupDataPath, CancellationToken ct);

        [OperationContract]
        [FaultContract(typeof(ArgumentException))]
        [FaultContract(typeof(FileNotFoundException))]
        [FaultContract(typeof(PsdzRuntimeException))]
        IPsdzTal ExecuteTalFile(IPsdzConnection connection, string pathToTal, string vin, string pathToFa, TalExecutionSettings talExecutionSettings, CancellationToken ct);

        [OperationContract]
        [FaultContract(typeof(ArgumentException))]
        [FaultContract(typeof(PsdzRuntimeException))]
        IPsdzTal ExecuteHDDUpdate(IPsdzConnection connection, IPsdzTal tal, IPsdzFa fa, IPsdzVin vin, TalExecutionSettings settings);
    }
}
