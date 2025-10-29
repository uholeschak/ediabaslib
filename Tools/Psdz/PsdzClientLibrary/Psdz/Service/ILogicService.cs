using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.Psdz.Model;
using BMW.Rheingold.Psdz.Model.Ecu;
using BMW.Rheingold.Psdz.Model.Exceptions;
using BMW.Rheingold.Psdz.Model.Obd;
using BMW.Rheingold.Psdz.Model.Sfa;
using BMW.Rheingold.Psdz.Model.Svb;
using BMW.Rheingold.Psdz.Model.Swt;
using BMW.Rheingold.Psdz.Model.Tal;
using BMW.Rheingold.Psdz.Model.Tal.TalFilter;

namespace BMW.Rheingold.Psdz
{
    [ServiceContract(SessionMode = SessionMode.Required)]
    [ServiceKnownType(typeof(PsdzFa))]
    [ServiceKnownType(typeof(PsdzFp))]
    [ServiceKnownType(typeof(PsdzIstufe))]
    [ServiceKnownType(typeof(PsdzTal))]
    [ServiceKnownType(typeof(PsdzTalFilter))]
    [ServiceKnownType(typeof(PsdzSwtApplication))]
    [ServiceKnownType(typeof(PsdzSwtApplicationId))]
    [ServiceKnownType(typeof(PsdzSwtAction))]
    [ServiceKnownType(typeof(PsdzSvt))]
    [ServiceKnownType(typeof(PsdzStandardSvt))]
    [ServiceKnownType(typeof(PsdzSollverbauung))]
    [ServiceKnownType(typeof(PsdzConnection))]
    [ServiceKnownType(typeof(PsdzSgbmId))]
    [ServiceKnownType(typeof(PsdzDiagAddress))]
    [ServiceKnownType(typeof(PsdzEcuIdentifier))]
    [ServiceKnownType(typeof(PsdzObdData))]
    [ServiceKnownType(typeof(PsdzSollSfaCto))]
    [ServiceKnownType(typeof(PsdzSecureTokenEto))]
    [ServiceKnownType(typeof(PsdzFeatureLongStatusCto))]
    [ServiceKnownType(typeof(PsdzEcuFeatureTokenRelationCto))]
    [ServiceKnownType(typeof(PsdzFeatureIdCto))]
    [ServiceKnownType(typeof(PsdzProgrammingProtectionDataCto))]
    [ServiceKnownType(typeof(TalGenerationSettings))]
    public interface ILogicService
    {
        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        IPsdzTal ChangeSwtActionType(IPsdzTal tal, IEnumerable<IPsdzSwtApplicationId> swtApplicationIds, IEnumerable<PsdzSwtActionType> swtActionTypes);

        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        IPsdzTal DeleteSwtTransactions(IPsdzTal tal, IEnumerable<IPsdzSwtApplicationId> swtApplicationIdsWhitelist, IEnumerable<IPsdzSwtApplicationId> swtApplicationIdsBlacklist);

        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        IPsdzStandardSvt FillBntnNamesForMainSeries(string baureihenverbund, IPsdzStandardSvt svt);

        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        IPsdzTal FillFsc(IPsdzTal tal, IPsdzSwtAction swtAction);

        [OperationContract(Name = "FillFscSingle")]
        [FaultContract(typeof(PsdzRuntimeException))]
        IPsdzTal FillFsc(IPsdzTal tal, IEnumerable<IPsdzSwtApplication> swtApplications);

        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        IPsdzTal FilterTal(IPsdzTal tal, IPsdzTalFilter talFilter);

        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        IPsdzTal GenerateFcfnTal(IEnumerable<IPsdzSwtApplication> swtApplications, IPsdzSvt svtActual, IPsdzTalFilter talFilter);

        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        IPsdzFp GenerateFp(IPsdzFa fa);

        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        IPsdzFp GenerateFpInterpretation(string baureihe, IPsdzFp fp);

        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        IPsdzSollverbauung GenerateSollverbauungEinzelFlash(IPsdzConnection connection, IPsdzIstufe iStufeTarget, IPsdzIstufe iStufeShipment, IPsdzSvt svtActual, IPsdzFa faTarget, IEnumerable<IPsdzDiagAddress> ecusToBeFlashed);

        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        IPsdzSollverbauung GenerateSollverbauungGesamtFlash(IPsdzConnection connection, IPsdzIstufe iStufeTarget, IPsdzIstufe iStufeShipment, IPsdzSvt svtActual, IPsdzFa faTarget, IPsdzTalFilter talFilter);

        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        IPsdzSollverbauung GenerateSollverbauungGesamtflashWithMatcher(IPsdzIstufe iStufeTarget, IPsdzIstufe iStufeShipment, IPsdzSvt svtActual, IPsdzFa faTarget, IPsdzTalFilter talFilter);

        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        IPsdzSvt GenerateSvtSoll(IPsdzConnection connection, IPsdzFa faActual, IPsdzFa faTarget, IPsdzSvt svtActual, IPsdzIstufe iStufeShipment, IPsdzIstufe iStufeActual, IPsdzIstufe iStufeTarget);

        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        IPsdzTal GenerateTal(IPsdzConnection connection, IPsdzSvt svtActual, IPsdzSollverbauung sollverbauung, IPsdzSwtAction swtAction, IPsdzTalFilter talFilter, string backupDataPath, string vinFromFA = "");

        [OperationContract(Name = "GenerateTalWithoutBackup")]
        [FaultContract(typeof(PsdzRuntimeException))]
        IPsdzTal GenerateTal(IPsdzConnection connection, IPsdzSvt svtActual, IPsdzSollverbauung sollverbauung, IPsdzSwtAction swtAction, IPsdzTalFilter talFilter, string vinFromFA = "");

        [OperationContract(Name = "GenerateTalWithTalSettings")]
        [FaultContract(typeof(PsdzRuntimeException))]
        IPsdzTal GenerateTal(IPsdzConnection connection, IPsdzSvt svtActual, IPsdzSollverbauung sollverbauung, IPsdzSwtAction swtAction, IPsdzTalFilter talFilter, TalGenerationSettings config, string vinFromFA = "");

        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        IEnumerable<IPsdzIstufe> GetPossibleIntegrationLevel(IPsdzFa fa);

        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        IEnumerable<IPsdzSgbmId> RequestSweList(IPsdzTal tal, bool ignoreSwDelete);

        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        IDictionary<IPsdzEcuIdentifier, IPsdzObdData> RequestRelevantObdData(IPsdzConnection connection, IPsdzSvt svtActual);

        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        IPsdzTal RequestHDDUpdateTal(IPsdzConnection connection, IPsdzSvt svtActual, IPsdzSvt svtSoll, IPsdzSwtAction swtAction, string backupDataPath);

        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        IPsdzSollSfaCto GenerateSfaSollStand(IPsdzSvt svt, IEnumerable<IPsdzSecureTokenEto> tokenPack);

        [OperationContract(Name = "GenerateTalForSfaUsingTal")]
        [FaultContract(typeof(PsdzRuntimeException))]
        IPsdzTal GenerateTalForSfa(IPsdzTal tal, IEnumerable<IPsdzFeatureLongStatusCto> istSfa, IEnumerable<IPsdzEcuFeatureTokenRelationCto> sollSfa, PsdzCalculationStrategyEtoEnum calculationStrategy, IEnumerable<IPsdzSecureTokenEto> featureActivationTokens, IEnumerable<IPsdzDiagAddress> diagAddressCtos, IEnumerable<IPsdzFeatureIdCto> featureIdWhiteList, IEnumerable<IPsdzFeatureIdCto> featureIdBlackList, bool suppressCreationOfSfaWriteTA);

        [OperationContract(Name = "GenerateTalForSfaUsingSvt")]
        [FaultContract(typeof(PsdzRuntimeException))]
        IPsdzTal GenerateTalForSfa(IPsdzSvt svt, IEnumerable<IPsdzFeatureLongStatusCto> istSfa, IEnumerable<IPsdzEcuFeatureTokenRelationCto> sollSfa, PsdzCalculationStrategyEtoEnum calculationStrategy, IEnumerable<IPsdzSecureTokenEto> featureActivationTokens, IEnumerable<IPsdzDiagAddress> diagAddressCtos, IEnumerable<IPsdzFeatureIdCto> featureIdWhiteList, IEnumerable<IPsdzFeatureIdCto> featureIdBlackList, bool suppressCreationOfSfaWriteTA);

        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        IPsdzProgrammingProtectionDataCto GenerateSweListForProgrammingProtection(IPsdzConnection connection, IPsdzTal tal);
    }
}
