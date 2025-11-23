using BMW.Rheingold.Psdz.Model;
using BMW.Rheingold.Psdz.Model.Ecu;
using BMW.Rheingold.Psdz.Model.Exceptions;
using BMW.Rheingold.Psdz.Model.SecureCoding;
using BMW.Rheingold.Psdz.Model.Tal;
using BMW.Rheingold.Psdz.Model.Tal.TalFilter;
using PsdzClient;
using System.ServiceModel;

namespace BMW.Rheingold.Psdz
{
    [PreserveSource(AttributesModified = true)]
    [ServiceContract(SessionMode = SessionMode.Required, CallbackContract = typeof(IPsdzProgressListener))]
    [ServiceKnownType(typeof(PsdzConnection))]
    [ServiceKnownType(typeof(PsdzTal))]
    [ServiceKnownType(typeof(PsdzTalFilter))]
    [ServiceKnownType(typeof(PsdzSvt))]
    [ServiceKnownType(typeof(PsdzFa))]
    [ServiceKnownType(typeof(PsdzVin))]
    [ServiceKnownType(typeof(TalExecutionSettings))]
    [ServiceKnownType(typeof(PsdzSecureCodingConfigCto))]
    [ServiceKnownType(typeof(PsdzDiagAddress))]
    public interface IIndividualDataRestoreService
    {
        [PreserveSource(KeepAttribute = true)]
        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        IPsdzTal GenerateBackupTal(IPsdzConnection connection, string backupDataPath, IPsdzTal standardTal, IPsdzTalFilter talFilter);

        [PreserveSource(KeepAttribute = true)]
        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        IPsdzTal GenerateRestorePrognosisTal(IPsdzConnection connection, string backupDataPath, IPsdzTal standardTal, IPsdzTal backupTal, IPsdzTalFilter talFilter);

        [PreserveSource(KeepAttribute = true)]
        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        IPsdzTal ExecuteAsyncBackupTal(IPsdzConnection connection, IPsdzTal tal, IPsdzSvt svtTarget, IPsdzFa faTarget, IPsdzVin vin, TalExecutionSettings talExecutionSettings, string backupDataPath);

        [PreserveSource(KeepAttribute = true)]
        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        IPsdzTal ExecuteBackupTal(IPsdzConnection connection, IPsdzTal tal, IPsdzSvt svtTarget, IPsdzFa faTarget, IPsdzVin vin, TalExecutionSettings talExecutionSettings, string backupDataPath);

        [PreserveSource(KeepAttribute = true)]
        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        IPsdzTal GenerateRestoreTal(IPsdzConnection connection, string backupDataFilePath, IPsdzTal standardTal, IPsdzTalFilter talFilter);

        [PreserveSource(KeepAttribute = true)]
        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        IPsdzTal ExecuteAsyncRestoreTal(IPsdzConnection connection, IPsdzTal tal, IPsdzSvt svtTarget, IPsdzFa faTarget, IPsdzVin vin, TalExecutionSettings talExecutionSettings);

        [PreserveSource(KeepAttribute = true)]
        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        IPsdzTal ExecuteRestoreTal(IPsdzConnection connection, IPsdzTal tal, IPsdzSvt svtTarget, IPsdzFa faTarget, IPsdzVin vin, TalExecutionSettings talExecutionSettings);
    }
}
