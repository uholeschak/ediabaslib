using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient
{
	class IndividualDataRestoreServiceClient : PsdzDuplexClientBase<IIndividualDataRestoreService, IPsdzProgressListener>, IIndividualDataRestoreService
	{
		public IndividualDataRestoreServiceClient(IPsdzProgressListener progressListener, Binding binding, EndpointAddress remoteAddress) : base(progressListener, binding, remoteAddress)
		{
		}

		public IPsdzTal GenerateBackupTal(IPsdzConnection connection, string backupDataPath, IPsdzTal standardTal, IPsdzTalFilter talFilter)
		{
			return base.CallFunction<IPsdzTal>((IIndividualDataRestoreService m) => m.GenerateBackupTal(connection, backupDataPath, standardTal, talFilter));
		}

		public IPsdzTal ExecuteBackupTal(IPsdzConnection connection, IPsdzTal tal, IPsdzSvt svtTarget, IPsdzFa faTarget, IPsdzVin vin, TalExecutionSettings talExecutionSettings, string backupDataPath)
		{
			return base.CallFunction<IPsdzTal>((IIndividualDataRestoreService service) => service.ExecuteBackupTal(connection, tal, svtTarget, faTarget, vin, talExecutionSettings, backupDataPath));
		}

		public IPsdzTal ExecuteAsyncBackupTal(IPsdzConnection connection, IPsdzTal tal, IPsdzSvt svtTarget, IPsdzFa faTarget, IPsdzVin vin, TalExecutionSettings talExecutionSettings, string backupDataPath)
		{
			return base.CallFunction<IPsdzTal>((IIndividualDataRestoreService service) => service.ExecuteAsyncBackupTal(connection, tal, svtTarget, faTarget, vin, talExecutionSettings, backupDataPath));
		}

		public IPsdzTal GenerateRestoreTal(IPsdzConnection connection, string backupDataFilePath, IPsdzTal standardTal, IPsdzTalFilter talFilter)
		{
			return base.CallFunction<IPsdzTal>((IIndividualDataRestoreService service) => service.GenerateRestoreTal(connection, backupDataFilePath, standardTal, talFilter));
		}

		public IPsdzTal ExecuteAsyncRestoreTal(IPsdzConnection connection, IPsdzTal tal, IPsdzSvt svtTarget, IPsdzFa faTarget, IPsdzVin vin, TalExecutionSettings talExecutionSettings)
		{
			return base.CallFunction<IPsdzTal>((IIndividualDataRestoreService service) => service.ExecuteAsyncRestoreTal(connection, tal, svtTarget, faTarget, vin, talExecutionSettings));
		}

		public IPsdzTal ExecuteRestoreTal(IPsdzConnection connection, IPsdzTal tal, IPsdzSvt svtTarget, IPsdzFa faTarget, IPsdzVin vin, TalExecutionSettings talExecutionSettings)
		{
			return base.CallFunction<IPsdzTal>((IIndividualDataRestoreService service) => service.ExecuteRestoreTal(connection, tal, svtTarget, faTarget, vin, talExecutionSettings));
		}

		public IPsdzTal GenerateRestorePrognosisTal(IPsdzConnection connection, string backupDataPath, IPsdzTal standardTal, IPsdzTal backupTal, IPsdzTalFilter talFilter)
		{
			return base.CallFunction<IPsdzTal>((IIndividualDataRestoreService service) => service.GenerateRestorePrognosisTal(connection, backupDataPath, standardTal, backupTal, talFilter));
		}
	}
}
