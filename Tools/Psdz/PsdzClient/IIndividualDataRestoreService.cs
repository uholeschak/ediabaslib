using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient
{
	[ServiceContract(SessionMode = SessionMode.Required, CallbackContract = typeof(IPsdzProgressListener))]
	[ServiceKnownType(typeof(PsdzSvt))]
	[ServiceKnownType(typeof(PsdzFa))]
	[ServiceKnownType(typeof(PsdzTalFilter))]
	[ServiceKnownType(typeof(TalExecutionSettings))]
	[ServiceKnownType(typeof(PsdzSecureCodingConfigCto))]
	[ServiceKnownType(typeof(PsdzVin))]
	[ServiceKnownType(typeof(PsdzConnection))]
	[ServiceKnownType(typeof(PsdzTal))]
	public interface IIndividualDataRestoreService
	{
		[FaultContract(typeof(PsdzRuntimeException))]
		[OperationContract]
		IPsdzTal GenerateBackupTal(IPsdzConnection connection, string backupDataPath, IPsdzTal standardTal, IPsdzTalFilter talFilter);

		[FaultContract(typeof(PsdzRuntimeException))]
		[OperationContract]
		IPsdzTal GenerateRestorePrognosisTal(IPsdzConnection connection, string backupDataPath, IPsdzTal standardTal, IPsdzTal backupTal, IPsdzTalFilter talFilter);

		[OperationContract]
		[FaultContract(typeof(PsdzRuntimeException))]
		IPsdzTal ExecuteAsyncBackupTal(IPsdzConnection connection, IPsdzTal tal, IPsdzSvt svtTarget, IPsdzFa faTarget, IPsdzVin vin, TalExecutionSettings talExecutionSettings, string backupDataPath);

		[FaultContract(typeof(PsdzRuntimeException))]
		[OperationContract]
		IPsdzTal ExecuteBackupTal(IPsdzConnection connection, IPsdzTal tal, IPsdzSvt svtTarget, IPsdzFa faTarget, IPsdzVin vin, TalExecutionSettings talExecutionSettings, string backupDataPath);

		[FaultContract(typeof(PsdzRuntimeException))]
		[OperationContract]
		IPsdzTal GenerateRestoreTal(IPsdzConnection connection, string backupDataFilePath, IPsdzTal standardTal, IPsdzTalFilter talFilter);

		[FaultContract(typeof(PsdzRuntimeException))]
		[OperationContract]
		IPsdzTal ExecuteAsyncRestoreTal(IPsdzConnection connection, IPsdzTal tal, IPsdzSvt svtTarget, IPsdzFa faTarget, IPsdzVin vin, TalExecutionSettings talExecutionSettings);

		[FaultContract(typeof(PsdzRuntimeException))]
		[OperationContract]
		IPsdzTal ExecuteRestoreTal(IPsdzConnection connection, IPsdzTal tal, IPsdzSvt svtTarget, IPsdzFa faTarget, IPsdzVin vin, TalExecutionSettings talExecutionSettings);
	}
}
