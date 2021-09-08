using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Psdz
{
    public enum PsdzCalculationStrategyEtoEnum
    {
        AFTER_CERTIFICATES,
        BEFORE_CERTIFICATES,
        END_OF_LINE,
        UPDATE,
        UPDATE_WITHOUT_DELETE
    }

	[ServiceKnownType(typeof(PsdzObdData))]
	[ServiceKnownType(typeof(PsdzTalFilter))]
	[ServiceKnownType(typeof(PsdzSwtApplicationId))]
	[ServiceKnownType(typeof(PsdzSwtAction))]
	[ServiceKnownType(typeof(PsdzSvt))]
	[ServiceKnownType(typeof(PsdzStandardSvt))]
	[ServiceKnownType(typeof(PsdzSollverbauung))]
	[ServiceKnownType(typeof(PsdzConnection))]
	[ServiceKnownType(typeof(PsdzSgbmId))]
	[ServiceKnownType(typeof(PsdzDiagAddress))]
	[ServiceKnownType(typeof(PsdzTal))]
	[ServiceContract(SessionMode = SessionMode.Required)]
	[ServiceKnownType(typeof(PsdzSollSfaCto))]
	[ServiceKnownType(typeof(PsdzSecureTokenEto))]
	[ServiceKnownType(typeof(PsdzFeatureLongStatusCto))]
	[ServiceKnownType(typeof(PsdzEcuFeatureTokenRelationCto))]
	[ServiceKnownType(typeof(PsdzFeatureIdCto))]
	[ServiceKnownType(typeof(PsdzEcuIdentifier))]
	[ServiceKnownType(typeof(PsdzSwtApplication))]
	[ServiceKnownType(typeof(PsdzFa))]
	[ServiceKnownType(typeof(PsdzFp))]
	[ServiceKnownType(typeof(PsdzIstufe))]
	public interface ILogicService
	{
		[FaultContract(typeof(PsdzRuntimeException))]
		[OperationContract]
		IPsdzTal ChangeSwtActionType(IPsdzTal tal, IEnumerable<IPsdzSwtApplicationId> swtApplicationIds, IEnumerable<PsdzSwtActionType> swtActionTypes);

		[FaultContract(typeof(PsdzRuntimeException))]
		[OperationContract]
		IPsdzTal DeleteSwtTransactions(IPsdzTal tal, IEnumerable<IPsdzSwtApplicationId> swtApplicationIdsWhitelist, IEnumerable<IPsdzSwtApplicationId> swtApplicationIdsBlacklist);

		[FaultContract(typeof(PsdzRuntimeException))]
		[OperationContract]
		IPsdzStandardSvt FillBntnNamesForMainSeries(string baureihenverbund, IPsdzStandardSvt svt);

		[FaultContract(typeof(PsdzRuntimeException))]
		[OperationContract]
		IPsdzTal FillFsc(IPsdzTal tal, IPsdzSwtAction swtAction);

		[FaultContract(typeof(PsdzRuntimeException))]
		[OperationContract(Name = "FillFscSingle")]
		IPsdzTal FillFsc(IPsdzTal tal, IEnumerable<IPsdzSwtApplication> swtApplications);

		[OperationContract]
		[FaultContract(typeof(PsdzRuntimeException))]
		IPsdzTal FilterTal(IPsdzTal tal, IPsdzTalFilter talFilter);

		[OperationContract]
		[FaultContract(typeof(PsdzRuntimeException))]
		IPsdzTal GenerateFcfnTal(IEnumerable<IPsdzSwtApplication> swtApplications, IPsdzSvt svtActual, IPsdzTalFilter talFilter);

		[FaultContract(typeof(PsdzRuntimeException))]
		[OperationContract]
		IPsdzFp GenerateFp(IPsdzFa fa);

		[FaultContract(typeof(PsdzRuntimeException))]
		[OperationContract]
		IPsdzFp GenerateFpInterpretation(string baureihe, IPsdzFp fp);

		[FaultContract(typeof(PsdzRuntimeException))]
		[OperationContract]
		IPsdzSollverbauung GenerateSollverbauungEinzelFlash(IPsdzConnection connection, IPsdzIstufe iStufeTarget, IPsdzIstufe iStufeShipment, IPsdzSvt svtActual, IPsdzFa faTarget, IEnumerable<IPsdzDiagAddress> ecusToBeFlashed);

		[FaultContract(typeof(PsdzRuntimeException))]
		[OperationContract]
		IPsdzSollverbauung GenerateSollverbauungGesamtFlash(IPsdzConnection connection, IPsdzIstufe iStufeTarget, IPsdzIstufe iStufeShipment, IPsdzSvt svtActual, IPsdzFa faTarget, IPsdzTalFilter talFilter);

		[FaultContract(typeof(PsdzRuntimeException))]
		[OperationContract]
		IPsdzSvt GenerateSvtSoll(IPsdzConnection connection, IPsdzFa faActual, IPsdzFa faTarget, IPsdzSvt svtActual, IPsdzIstufe iStufeShipment, IPsdzIstufe iStufeActual, IPsdzIstufe iStufeTarget);

		[OperationContract]
		[FaultContract(typeof(PsdzRuntimeException))]
		IPsdzTal GenerateTal(IPsdzConnection connection, IPsdzSvt svtActual, IPsdzSollverbauung sollverbauung, IPsdzSwtAction swtAction, IPsdzTalFilter talFilter, string backupDataPath, string vinFromFA = "");

		[FaultContract(typeof(PsdzRuntimeException))]
		[OperationContract(Name = "GenerateTalWithoutBackup")]
		IPsdzTal GenerateTal(IPsdzConnection connection, IPsdzSvt svtActual, IPsdzSollverbauung sollverbauung, IPsdzSwtAction swtAction, IPsdzTalFilter talFilter, string vinFromFA = "");

		[OperationContract]
		[FaultContract(typeof(PsdzRuntimeException))]
		IEnumerable<IPsdzIstufe> GetPossibleIntegrationLevel(IPsdzFa fa);

		[FaultContract(typeof(PsdzRuntimeException))]
		[OperationContract]
		IEnumerable<IPsdzSgbmId> RequestSweList(IPsdzTal tal, bool ignoreSwDelete);

		[OperationContract]
		[FaultContract(typeof(PsdzRuntimeException))]
		IDictionary<IPsdzEcuIdentifier, IPsdzObdData> RequestRelevantObdData(IPsdzConnection connection, IPsdzSvt svtActual);

		[OperationContract]
		[FaultContract(typeof(PsdzRuntimeException))]
		IPsdzTal RequestHDDUpdateTal(IPsdzConnection connection, IPsdzSvt svtActual, IPsdzSvt svtSoll, IPsdzSwtAction swtAction, string backupDataPath);

		[OperationContract]
		[FaultContract(typeof(PsdzRuntimeException))]
		IPsdzSollSfaCto GenerateSfaSollStand(IEnumerable<IPsdzSecureTokenEto> tokenPack);

		[FaultContract(typeof(PsdzRuntimeException))]
		[OperationContract(Name = "GenerateTalForSfaUsingTal")]
		IPsdzTal GenerateTalForSfa(IPsdzTal tal, IEnumerable<IPsdzFeatureLongStatusCto> istSfa, IEnumerable<IPsdzEcuFeatureTokenRelationCto> sollSfa, PsdzCalculationStrategyEtoEnum calculationStrategy, IEnumerable<IPsdzSecureTokenEto> featureActivationTokens, IEnumerable<IPsdzDiagAddress> diagAddressCtos, IEnumerable<IPsdzFeatureIdCto> featureIdWhiteList, IEnumerable<IPsdzFeatureIdCto> featureIdBlackList);

		[OperationContract(Name = "GenerateTalForSfaUsingSvt")]
		[FaultContract(typeof(PsdzRuntimeException))]
		IPsdzTal GenerateTalForSfa(IPsdzSvt svt, IEnumerable<IPsdzFeatureLongStatusCto> istSfa, IEnumerable<IPsdzEcuFeatureTokenRelationCto> sollSfa, PsdzCalculationStrategyEtoEnum calculationStrategy, IEnumerable<IPsdzSecureTokenEto> featureActivationTokens, IEnumerable<IPsdzDiagAddress> diagAddressCtos, IEnumerable<IPsdzFeatureIdCto> featureIdWhiteList, IEnumerable<IPsdzFeatureIdCto> featureIdBlackList);
	}
}
