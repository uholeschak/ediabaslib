using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Channels;
using BMW.Rheingold.Psdz.Model;
using BMW.Rheingold.Psdz.Model.Ecu;
using BMW.Rheingold.Psdz.Model.Obd;
using BMW.Rheingold.Psdz.Model.Sfa;
using BMW.Rheingold.Psdz.Model.Svb;
using BMW.Rheingold.Psdz.Model.Swt;
using BMW.Rheingold.Psdz.Model.Tal;
using BMW.Rheingold.Psdz.Model.Tal.TalFilter;

namespace BMW.Rheingold.Psdz.Client
{
    internal sealed class LogicServiceClient : PsdzClientBase<ILogicService>, ILogicService
    {
        internal LogicServiceClient(Binding binding, EndpointAddress remoteAddress)
            : base(binding, remoteAddress)
        {
        }

        public IPsdzTal GenerateTalForSfa(IPsdzTal tal, IEnumerable<IPsdzFeatureLongStatusCto> istSfa, IEnumerable<IPsdzEcuFeatureTokenRelationCto> sollSfa, PsdzCalculationStrategyEtoEnum calculationStrategy, IEnumerable<IPsdzSecureTokenEto> featureActivationTokens, IEnumerable<IPsdzDiagAddress> diagAddressCtos, IEnumerable<IPsdzFeatureIdCto> featureIdWhiteList, IEnumerable<IPsdzFeatureIdCto> featureIdBlackList, bool suppressCreationOfSfaWriteTA)
        {
            return CallFunction((ILogicService m) => m.GenerateTalForSfa(tal, istSfa, sollSfa, calculationStrategy, featureActivationTokens, diagAddressCtos, featureIdWhiteList, featureIdBlackList, suppressCreationOfSfaWriteTA));
        }

        public IPsdzTal GenerateTalForSfa(IPsdzSvt svt, IEnumerable<IPsdzFeatureLongStatusCto> istSfa, IEnumerable<IPsdzEcuFeatureTokenRelationCto> sollSfa, PsdzCalculationStrategyEtoEnum calculationStrategy, IEnumerable<IPsdzSecureTokenEto> featureActivationTokens, IEnumerable<IPsdzDiagAddress> diagAddressCtos, IEnumerable<IPsdzFeatureIdCto> featureIdWhiteList, IEnumerable<IPsdzFeatureIdCto> featureIdBlackList, bool suppressCreationOfSfaWriteTA)
        {
            return CallFunction((ILogicService m) => m.GenerateTalForSfa(svt, istSfa, sollSfa, calculationStrategy, featureActivationTokens, diagAddressCtos, featureIdWhiteList, featureIdBlackList, suppressCreationOfSfaWriteTA));
        }

        public IPsdzSollSfaCto GenerateSfaSollStand(IPsdzSvt svt, IEnumerable<IPsdzSecureTokenEto> tokenPack)
        {
            return CallFunction((ILogicService m) => m.GenerateSfaSollStand(svt, tokenPack));
        }

        public IPsdzTal ChangeSwtActionType(IPsdzTal tal, IEnumerable<IPsdzSwtApplicationId> swtApplicationIds, IEnumerable<PsdzSwtActionType> swtActionTypes)
        {
            return CallFunction((ILogicService m) => m.ChangeSwtActionType(tal, swtApplicationIds, swtActionTypes));
        }

        public IPsdzTal DeleteSwtTransactions(IPsdzTal tal, IEnumerable<IPsdzSwtApplicationId> swtApplicationIdsWhitelist, IEnumerable<IPsdzSwtApplicationId> swtApplicationIdsBlacklist)
        {
            return CallFunction((ILogicService m) => m.DeleteSwtTransactions(tal, swtApplicationIdsWhitelist, swtApplicationIdsBlacklist));
        }

        public IPsdzStandardSvt FillBntnNamesForMainSeries(string baureihenverbund, IPsdzStandardSvt svt)
        {
            return CallFunction((ILogicService m) => m.FillBntnNamesForMainSeries(baureihenverbund, svt));
        }

        public IPsdzTal FillFsc(IPsdzTal tal, IPsdzSwtAction swtAction)
        {
            return CallFunction((ILogicService m) => m.FillFsc(tal, swtAction));
        }

        public IPsdzTal FillFsc(IPsdzTal tal, IEnumerable<IPsdzSwtApplication> swtApplications)
        {
            return CallFunction((ILogicService m) => m.FillFsc(tal, swtApplications));
        }

        public IPsdzTal FilterTal(IPsdzTal tal, IPsdzTalFilter talFilter)
        {
            return CallFunction((ILogicService m) => m.FilterTal(tal, talFilter));
        }

        public IPsdzTal GenerateFcfnTal(IEnumerable<IPsdzSwtApplication> swtApplications, IPsdzSvt svtActual, IPsdzTalFilter talFilter)
        {
            return CallFunction((ILogicService m) => m.GenerateFcfnTal(swtApplications, svtActual, talFilter));
        }

        public IPsdzFp GenerateFp(IPsdzFa fa)
        {
            return CallFunction((ILogicService m) => m.GenerateFp(fa));
        }

        public IPsdzFp GenerateFpInterpretation(string baureihe, IPsdzFp fp)
        {
            return CallFunction((ILogicService m) => m.GenerateFpInterpretation(baureihe, fp));
        }

        public IPsdzSollverbauung GenerateSollverbauungEinzelFlash(IPsdzConnection connection, IPsdzIstufe iStufeTarget, IPsdzIstufe iStufeShipment, IPsdzSvt svtActual, IPsdzFa faTarget, IEnumerable<IPsdzDiagAddress> ecusToBeFlashed)
        {
            return CallFunction((ILogicService m) => m.GenerateSollverbauungEinzelFlash(connection, iStufeTarget, iStufeShipment, svtActual, faTarget, ecusToBeFlashed));
        }

        public IPsdzSollverbauung GenerateSollverbauungGesamtFlash(IPsdzConnection connection, IPsdzIstufe iStufeTarget, IPsdzIstufe iStufeShipment, IPsdzSvt svtActual, IPsdzFa faTarget, IPsdzTalFilter talFilter)
        {
            return CallFunction((ILogicService m) => m.GenerateSollverbauungGesamtFlash(connection, iStufeTarget, iStufeShipment, svtActual, faTarget, talFilter));
        }

        public IPsdzSollverbauung GenerateSollverbauungGesamtflashWithMatcher(IPsdzIstufe iStufeTarget, IPsdzIstufe iStufeShipment, IPsdzSvt svtActual, IPsdzFa faTarget, IPsdzTalFilter talFilter)
        {
            return CallFunction((ILogicService m) => m.GenerateSollverbauungGesamtflashWithMatcher(iStufeTarget, iStufeShipment, svtActual, faTarget, talFilter));
        }

        public IPsdzSvt GenerateSvtSoll(IPsdzConnection connection, IPsdzFa faActual, IPsdzFa faTarget, IPsdzSvt svtActual, IPsdzIstufe iStufeShipment, IPsdzIstufe iStufeActual, IPsdzIstufe iStufeTarget)
        {
            return CallFunction((ILogicService m) => m.GenerateSvtSoll(connection, faActual, faTarget, svtActual, iStufeShipment, iStufeActual, iStufeTarget));
        }

        public IPsdzTal GenerateTal(IPsdzConnection connection, IPsdzSvt svtActual, IPsdzSollverbauung sollverbauung, IPsdzSwtAction swtAction, IPsdzTalFilter talFilter, string backupDataPath, string vinFromFA = "")
        {
            return CallFunction((ILogicService m) => m.GenerateTal(connection, svtActual, sollverbauung, swtAction, talFilter, backupDataPath, vinFromFA));
        }

        public IPsdzTal GenerateTal(IPsdzConnection connection, IPsdzSvt svtActual, IPsdzSollverbauung sollverbauung, IPsdzSwtAction swtAction, IPsdzTalFilter talFilter, string vinFromFA = "")
        {
            return CallFunction((ILogicService m) => m.GenerateTal(connection, svtActual, sollverbauung, swtAction, talFilter, vinFromFA));
        }

        public IPsdzTal GenerateTal(IPsdzConnection connection, IPsdzSvt svtActual, IPsdzSollverbauung sollverbauung, IPsdzSwtAction swtAction, IPsdzTalFilter talFilter, TalGenerationSettings config, string vinFromFA = "")
        {
            return CallFunction((ILogicService m) => m.GenerateTal(connection, svtActual, sollverbauung, swtAction, talFilter, config, vinFromFA));
        }

        public IEnumerable<IPsdzIstufe> GetPossibleIntegrationLevel(IPsdzFa fa)
        {
            return CallFunction((ILogicService m) => m.GetPossibleIntegrationLevel(fa));
        }

        public IPsdzTal RequestHDDUpdateTal(IPsdzConnection connection, IPsdzSvt svtActual, IPsdzSvt svtSoll, IPsdzSwtAction swtAction, string backupDataPath)
        {
            return CallFunction((ILogicService m) => m.RequestHDDUpdateTal(connection, svtActual, svtSoll, swtAction, backupDataPath));
        }

        public IEnumerable<IPsdzSgbmId> RequestSweList(IPsdzTal tal, bool ignoreSwDelete)
        {
            return CallFunction((ILogicService m) => m.RequestSweList(tal, ignoreSwDelete));
        }

        public IDictionary<IPsdzEcuIdentifier, IPsdzObdData> RequestRelevantObdData(IPsdzConnection connection, IPsdzSvt svtActual)
        {
            return CallFunction((ILogicService m) => m.RequestRelevantObdData(connection, svtActual));
        }

        public IPsdzProgrammingProtectionDataCto GenerateSweListForProgrammingProtection(IPsdzConnection connection, IPsdzTal tal)
        {
            return CallFunction((ILogicService m) => m.GenerateSweListForProgrammingProtection(connection, tal));
        }
    }
}
