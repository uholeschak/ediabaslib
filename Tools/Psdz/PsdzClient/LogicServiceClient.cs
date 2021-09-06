using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient
{
	class LogicServiceClient : PsdzClientBase<ILogicService>, ILogicService
	{
		internal LogicServiceClient(Binding binding, EndpointAddress remoteAddress) : base(binding, remoteAddress)
		{
		}

		public IPsdzTal GenerateTalForSfa(IPsdzTal tal, IEnumerable<IPsdzFeatureLongStatusCto> istSfa, IEnumerable<IPsdzEcuFeatureTokenRelationCto> sollSfa, PsdzCalculationStrategyEtoEnum calculationStrategy, IEnumerable<IPsdzSecureTokenEto> featureActivationTokens, IEnumerable<IPsdzDiagAddress> diagAddressCtos, IEnumerable<IPsdzFeatureIdCto> featureIdWhiteList, IEnumerable<IPsdzFeatureIdCto> featureIdBlackList)
		{
			return base.CallFunction<IPsdzTal>((ILogicService m) => m.GenerateTalForSfa(tal, istSfa, sollSfa, calculationStrategy, featureActivationTokens, diagAddressCtos, featureIdWhiteList, featureIdBlackList));
		}

		public IPsdzTal GenerateTalForSfa(IPsdzSvt svt, IEnumerable<IPsdzFeatureLongStatusCto> istSfa, IEnumerable<IPsdzEcuFeatureTokenRelationCto> sollSfa, PsdzCalculationStrategyEtoEnum calculationStrategy, IEnumerable<IPsdzSecureTokenEto> featureActivationTokens, IEnumerable<IPsdzDiagAddress> diagAddressCtos, IEnumerable<IPsdzFeatureIdCto> featureIdWhiteList, IEnumerable<IPsdzFeatureIdCto> featureIdBlackList)
		{
			return base.CallFunction<IPsdzTal>((ILogicService m) => m.GenerateTalForSfa(svt, istSfa, sollSfa, calculationStrategy, featureActivationTokens, diagAddressCtos, featureIdWhiteList, featureIdBlackList));
		}

		public IPsdzSollSfaCto GenerateSfaSollStand(IEnumerable<IPsdzSecureTokenEto> tokenPack)
		{
			return base.CallFunction<IPsdzSollSfaCto>((ILogicService m) => m.GenerateSfaSollStand(tokenPack));
		}

		public IPsdzTal ChangeSwtActionType(IPsdzTal tal, IEnumerable<IPsdzSwtApplicationId> swtApplicationIds, IEnumerable<PsdzSwtActionType> swtActionTypes)
		{
			return base.CallFunction<IPsdzTal>((ILogicService m) => m.ChangeSwtActionType(tal, swtApplicationIds, swtActionTypes));
		}

		public IPsdzTal DeleteSwtTransactions(IPsdzTal tal, IEnumerable<IPsdzSwtApplicationId> swtApplicationIdsWhitelist, IEnumerable<IPsdzSwtApplicationId> swtApplicationIdsBlacklist)
		{
			return base.CallFunction<IPsdzTal>((ILogicService m) => m.DeleteSwtTransactions(tal, swtApplicationIdsWhitelist, swtApplicationIdsBlacklist));
		}

		public IPsdzStandardSvt FillBntnNamesForMainSeries(string baureihenverbund, IPsdzStandardSvt svt)
		{
			return base.CallFunction<IPsdzStandardSvt>((ILogicService m) => m.FillBntnNamesForMainSeries(baureihenverbund, svt));
		}

		public IPsdzTal FillFsc(IPsdzTal tal, IPsdzSwtAction swtAction)
		{
			return base.CallFunction<IPsdzTal>((ILogicService m) => m.FillFsc(tal, swtAction));
		}

		public IPsdzTal FillFsc(IPsdzTal tal, IEnumerable<IPsdzSwtApplication> swtApplications)
		{
			return base.CallFunction<IPsdzTal>((ILogicService m) => m.FillFsc(tal, swtApplications));
		}

		public IPsdzTal FilterTal(IPsdzTal tal, IPsdzTalFilter talFilter)
		{
			return base.CallFunction<IPsdzTal>((ILogicService m) => m.FilterTal(tal, talFilter));
		}

		public IPsdzTal GenerateFcfnTal(IEnumerable<IPsdzSwtApplication> swtApplications, IPsdzSvt svtActual, IPsdzTalFilter talFilter)
		{
			return base.CallFunction<IPsdzTal>((ILogicService m) => m.GenerateFcfnTal(swtApplications, svtActual, talFilter));
		}

		public IPsdzFp GenerateFp(IPsdzFa fa)
		{
			return base.CallFunction<IPsdzFp>((ILogicService m) => m.GenerateFp(fa));
		}

		public IPsdzFp GenerateFpInterpretation(string baureihe, IPsdzFp fp)
		{
			return base.CallFunction<IPsdzFp>((ILogicService m) => m.GenerateFpInterpretation(baureihe, fp));
		}

		public IPsdzSollverbauung GenerateSollverbauungEinzelFlash(IPsdzConnection connection, IPsdzIstufe iStufeTarget, IPsdzIstufe iStufeShipment, IPsdzSvt svtActual, IPsdzFa faTarget, IEnumerable<IPsdzDiagAddress> ecusToBeFlashed)
		{
			return base.CallFunction<IPsdzSollverbauung>((ILogicService m) => m.GenerateSollverbauungEinzelFlash(connection, iStufeTarget, iStufeShipment, svtActual, faTarget, ecusToBeFlashed));
		}

		public IPsdzSollverbauung GenerateSollverbauungGesamtFlash(IPsdzConnection connection, IPsdzIstufe iStufeTarget, IPsdzIstufe iStufeShipment, IPsdzSvt svtActual, IPsdzFa faTarget, IPsdzTalFilter talFilter)
		{
			return base.CallFunction<IPsdzSollverbauung>((ILogicService m) => m.GenerateSollverbauungGesamtFlash(connection, iStufeTarget, iStufeShipment, svtActual, faTarget, talFilter));
		}

		public IPsdzSvt GenerateSvtSoll(IPsdzConnection connection, IPsdzFa faActual, IPsdzFa faTarget, IPsdzSvt svtActual, IPsdzIstufe iStufeShipment, IPsdzIstufe iStufeActual, IPsdzIstufe iStufeTarget)
		{
			return base.CallFunction<IPsdzSvt>((ILogicService m) => m.GenerateSvtSoll(connection, faActual, faTarget, svtActual, iStufeShipment, iStufeActual, iStufeTarget));
		}

		public IPsdzTal GenerateTal(IPsdzConnection connection, IPsdzSvt svtActual, IPsdzSollverbauung sollverbauung, IPsdzSwtAction swtAction, IPsdzTalFilter talFilter, string backupDataPath, string vinFromFA = "")
		{
			return base.CallFunction<IPsdzTal>((ILogicService m) => m.GenerateTal(connection, svtActual, sollverbauung, swtAction, talFilter, backupDataPath, vinFromFA));
		}

		public IPsdzTal GenerateTal(IPsdzConnection connection, IPsdzSvt svtActual, IPsdzSollverbauung sollverbauung, IPsdzSwtAction swtAction, IPsdzTalFilter talFilter, string vinFromFA = "")
		{
			return base.CallFunction<IPsdzTal>((ILogicService m) => m.GenerateTal(connection, svtActual, sollverbauung, swtAction, talFilter, vinFromFA));
		}

		public IEnumerable<IPsdzIstufe> GetPossibleIntegrationLevel(IPsdzFa fa)
		{
			return base.CallFunction<IEnumerable<IPsdzIstufe>>((ILogicService m) => m.GetPossibleIntegrationLevel(fa));
		}

		public IPsdzTal RequestHDDUpdateTal(IPsdzConnection connection, IPsdzSvt svtActual, IPsdzSvt svtSoll, IPsdzSwtAction swtAction, string backupDataPath)
		{
			return base.CallFunction<IPsdzTal>((ILogicService m) => m.RequestHDDUpdateTal(connection, svtActual, svtSoll, swtAction, backupDataPath));
		}

		public IEnumerable<IPsdzSgbmId> RequestSweList(IPsdzTal tal, bool ignoreSwDelete)
		{
			return base.CallFunction<IEnumerable<IPsdzSgbmId>>((ILogicService m) => m.RequestSweList(tal, ignoreSwDelete));
		}

		public IDictionary<IPsdzEcuIdentifier, IPsdzObdData> RequestRelevantObdData(IPsdzConnection connection, IPsdzSvt svtActual)
		{
			return base.CallFunction<IDictionary<IPsdzEcuIdentifier, IPsdzObdData>>((ILogicService m) => m.RequestRelevantObdData(connection, svtActual));
		}
	}
}
