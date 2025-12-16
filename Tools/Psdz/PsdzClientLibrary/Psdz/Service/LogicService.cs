using BMW.Rheingold.Psdz;
using BMW.Rheingold.Psdz.Model;
using BMW.Rheingold.Psdz.Model.Ecu;
using BMW.Rheingold.Psdz.Model.Obd;
using BMW.Rheingold.Psdz.Model.Sfa;
using BMW.Rheingold.Psdz.Model.Svb;
using BMW.Rheingold.Psdz.Model.Swt;
using BMW.Rheingold.Psdz.Model.Tal;
using BMW.Rheingold.Psdz.Model.Tal.TalFilter;
using PsdzClient.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using RestSharp;

namespace BMW.Rheingold.Psdz
{
    internal class LogicService : ILogicService
    {
        private readonly IWebCallHandler _webCallHandler;
        private readonly string endpointService = "logic";
        public LogicService(IWebCallHandler webCallHandler)
        {
            _webCallHandler = webCallHandler;
        }

        public IPsdzTal DeleteSwtTransactions(IPsdzTal tal, IEnumerable<IPsdzSwtApplicationId> swtApplicationIdsWhitelist, IEnumerable<IPsdzSwtApplicationId> swtApplicationIdsBlacklist)
        {
            try
            {
                DeleteSwtTransactionsRequestModel requestBodyObject = new DeleteSwtTransactionsRequestModel
                {
                    Tal = TalMapper.Map(tal),
                    SwtApplicationIdWhiteList = swtApplicationIdsWhitelist?.Select(SwtApplicationIdMapper.Map).ToList(),
                    SwtApplicationIdBlackList = swtApplicationIdsBlacklist?.Select(SwtApplicationIdMapper.Map).ToList()
                };
                return TalMapper.Map(_webCallHandler.ExecuteRequest<TalModel>(endpointService, "deleteswttransactions", HttpMethod.Post, requestBodyObject).Data);
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }

        public IPsdzStandardSvt FillBntnNamesForMainSeries(string baureihenverbund, IPsdzStandardSvt svt)
        {
            try
            {
                FillBntnNamesForMainSeriesRequestModel requestBodyObject = new FillBntnNamesForMainSeriesRequestModel
                {
                    Baureihenverbund = baureihenverbund,
                    Svt = StandardSvtMapper.Map(svt)
                };
                return StandardSvtMapper.Map(_webCallHandler.ExecuteRequest<StandardSvtModel>(endpointService, "fillbntnnamesformainseries", HttpMethod.Post, requestBodyObject).Data);
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }

        public IPsdzTal FillFsc(IPsdzTal tal, IEnumerable<IPsdzSwtApplication> swtApplications)
        {
            try
            {
                FillFscRequestModel requestBodyObject = new FillFscRequestModel
                {
                    Tal = TalMapper.Map(tal),
                    SwtApplications = swtApplications?.Select(SwtApplicationMapper.Map).ToList()
                };
                return TalMapper.Map(_webCallHandler.ExecuteRequest<TalModel>(endpointService, "fillfsc", HttpMethod.Post, requestBodyObject).Data);
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }

        public IPsdzTal FilterTal(IPsdzTal tal, IPsdzTalFilter talFilter)
        {
            try
            {
                FilterTalRequestModel requestBodyObject = new FilterTalRequestModel
                {
                    Tal = TalMapper.Map(tal),
                    TalFilter = TalFilterMapper.Map(talFilter)
                };
                return TalMapper.Map(_webCallHandler.ExecuteRequest<TalModel>(endpointService, "filtertal", HttpMethod.Post, requestBodyObject).Data);
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }

        public IPsdzTal GenerateFcfnTal(IEnumerable<IPsdzSwtApplication> swtApplications, IPsdzSvt svtActual, IPsdzTalFilter talFilter)
        {
            try
            {
                GenerateFcfnTalRequestModel requestBodyObject = new GenerateFcfnTalRequestModel
                {
                    SwtApplications = swtApplications?.Select(SwtApplicationMapper.Map).ToList(),
                    Svt = SvtMapper.Map(svtActual),
                    TalFilter = TalFilterMapper.Map(talFilter)
                };
                return TalMapper.Map(_webCallHandler.ExecuteRequest<TalModel>(endpointService, "generatefcfntal", HttpMethod.Post, requestBodyObject).Data);
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }

        public IPsdzFp GenerateFp(IPsdzFa fa)
        {
            try
            {
                GenerateFpRequestModel requestBodyObject = new GenerateFpRequestModel
                {
                    Fa = FaMapper.Map(fa)
                };
                return FpMapper.Map(_webCallHandler.ExecuteRequest<FpModel>(endpointService, "generatefp", HttpMethod.Post, requestBodyObject).Data);
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }

        public IPsdzFp GenerateFpInterpretation(string baureihe, IPsdzFp fp)
        {
            try
            {
                GenerateFpInterpretationRequestModel requestBodyObject = new GenerateFpInterpretationRequestModel
                {
                    Baureihe = baureihe,
                    Fp = FpMapper.Map(fp)
                };
                return FpMapper.Map(_webCallHandler.ExecuteRequest<FpModel>(endpointService, "generatefpinterpretation", HttpMethod.Post, requestBodyObject).Data);
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }

        public IPsdzSollSfaCto GenerateSfaSollStand(IPsdzSvt svt, IEnumerable<IPsdzSecureTokenEto> tokenPack)
        {
            try
            {
                GenerateSfaTargetStatusRequestModel requestBodyObject = new GenerateSfaTargetStatusRequestModel
                {
                    SvtCurrent = null,
                    PsdzTokenPack = tokenPack?.Select(SecureTokenEtoMapper.Map).ToList()
                };
                return SollSfaCtoMapper.Map(_webCallHandler.ExecuteRequest<SollSfaCtoModel>(endpointService, "generatesfasollstand", HttpMethod.Post, requestBodyObject).Data);
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }

        public IPsdzSollverbauung GenerateSollverbauungGesamtFlash(IPsdzConnection connection, IPsdzIstufe iStufeTarget, IPsdzIstufe iStufeShipment, IPsdzSvt svtActual, IPsdzFa faTarget, IPsdzTalFilter talFilter)
        {
            try
            {
                GenerateSollverbauungGesamtFlashRequestModel requestBodyObject = new GenerateSollverbauungGesamtFlashRequestModel
                {
                    ILevelTarget = ILevelMapper.Map(iStufeTarget),
                    ILevelShipment = ILevelMapper.Map(iStufeShipment),
                    SvtActual = SvtMapper.Map(svtActual),
                    FaTarget = FaMapper.Map(faTarget),
                    TalFilter = TalFilterMapper.Map(talFilter),
                    FaultTolerant = false
                };
                return SollverbauungMapper.Map(_webCallHandler.ExecuteRequest<SollverbauungModel>(endpointService, $"generatesollverbauunggesamtflash/{connection.Id}", HttpMethod.Post, requestBodyObject).Data);
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }

        public IPsdzSollverbauung GenerateSollverbauungGesamtflashWithMatcher(IPsdzIstufe iStufeTarget, IPsdzIstufe iStufeShipment, IPsdzSvt svtActual, IPsdzFa faTarget, IPsdzTalFilter talFilter)
        {
            try
            {
                GenerateSollverbauungGesamtFlashWithMatcherRequestModel requestBodyObject = new GenerateSollverbauungGesamtFlashWithMatcherRequestModel
                {
                    ILevelTarget = ILevelMapper.Map(iStufeTarget),
                    ILevelShipment = ILevelMapper.Map(iStufeShipment),
                    SvtActual = SvtMapper.Map(svtActual),
                    FaTarget = FaMapper.Map(faTarget),
                    TalFilter = TalFilterMapper.Map(talFilter),
                    FaultTolerant = true
                };
                return SollverbauungMapper.Map(_webCallHandler.ExecuteRequest<SollverbauungModel>(endpointService, "generatesollverbauunggesamtflashwithmatcher", HttpMethod.Post, requestBodyObject).Data);
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }

        public IPsdzTal GenerateTal(IPsdzConnection connection, IPsdzSvt svtActual, IPsdzSollverbauung sollverbauung, IPsdzSwtAction swtAction, IPsdzTalFilter talFilter, string backupDataPath, string vinFromFA = "")
        {
            return GenerateTal(connection, svtActual, sollverbauung, swtAction, talFilter, vinFromFA);
        }

        public IPsdzTal GenerateTal(IPsdzConnection connection, IPsdzSvt svtActual, IPsdzSollverbauung sollverbauung, IPsdzSwtAction swtAction, IPsdzTalFilter talFilter, string vinFromFA = "")
        {
            try
            {
                GenerateTalRequestModel requestBodyObject = new GenerateTalRequestModel
                {
                    SvtActual = SvtMapper.Map(svtActual),
                    Sollverbauung = SollverbauungMapper.Map(sollverbauung),
                    SwtAction = SwtActionMapper.Map(swtAction),
                    TalFilter = TalFilterMapper.Map(talFilter),
                    VinFromFa = ((vinFromFA == "") ? null : vinFromFA)
                };
                return TalMapper.Map(_webCallHandler.ExecuteRequest<TalModel>(endpointService, $"generatetal/{connection.Id}", HttpMethod.Post, requestBodyObject).Data);
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }

        public IPsdzTal GenerateTal(IPsdzConnection connection, IPsdzSvt svtActual, IPsdzSollverbauung sollverbauung, IPsdzSwtAction swtAction, IPsdzTalFilter talFilter, TalGenerationSettings config, string vinFromFA = "")
        {
            try
            {
                GenerateTalRequestModel requestBodyObject = new GenerateTalRequestModel
                {
                    SvtActual = SvtMapper.Map(svtActual),
                    Sollverbauung = SollverbauungMapper.Map(sollverbauung),
                    SwtAction = SwtActionMapper.Map(swtAction),
                    TalFilter = TalFilterMapper.Map(talFilter),
                    VinFromFa = ((vinFromFA == "") ? null : vinFromFA),
                    TalGenerationSettings = TalGenerationSettingsMapper.Map(config)
                };
                return TalMapper.Map(_webCallHandler.ExecuteRequest<TalModel>(endpointService, $"generateTalWithSettings/{connection.Id}", HttpMethod.Post, requestBodyObject).Data);
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }

        public IPsdzTal GenerateTalForSfa(IPsdzTal tal, IEnumerable<IPsdzFeatureLongStatusCto> istSfa, IEnumerable<IPsdzEcuFeatureTokenRelationCto> sollSfa, PsdzCalculationStrategyEtoEnum calculationStrategy, IEnumerable<IPsdzSecureTokenEto> featureActivationTokens, IEnumerable<IPsdzDiagAddress> diagAddressCtos, IEnumerable<IPsdzFeatureIdCto> featureIdWhiteList, IEnumerable<IPsdzFeatureIdCto> featureIdBlackList, bool suppressCreationOfSfaWriteTA)
        {
            try
            {
                GenerateTalForSfaRequestModel requestBodyObject = new GenerateTalForSfaRequestModel
                {
                    Tal = TalMapper.Map(tal),
                    SfaCurrent = istSfa?.Select(FeatureLongStatusCtoMapper.Map).ToList(),
                    SfaTarget = sollSfa?.Select(EcuFeatureTokenRelationCtoMapper.Map).ToList(),
                    CalculationStrategy = new CalculationStrategyEtoMapper().GetValue(calculationStrategy),
                    FeatureActivationTokens = featureActivationTokens?.Select(SecureTokenEtoMapper.Map).ToList(),
                    DiagAdresses = diagAddressCtos?.Select(DiagAddressMapper.Map).ToList(),
                    FeatureIdWhiteList = featureIdWhiteList?.Select(FeatureIdCtoMapper.Map).ToList(),
                    FeatureIdBlackList = featureIdBlackList?.Select(FeatureIdCtoMapper.Map).ToList(),
                    SupressCreationOfSfaWriteTA = suppressCreationOfSfaWriteTA
                };
                return TalMapper.Map(_webCallHandler.ExecuteRequest<TalModel>(endpointService, "generatetalforsfa", HttpMethod.Post, requestBodyObject).Data);
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }

        public IPsdzTal GenerateTalForSfa(IPsdzSvt svt, IEnumerable<IPsdzFeatureLongStatusCto> istSfa, IEnumerable<IPsdzEcuFeatureTokenRelationCto> sollSfa, PsdzCalculationStrategyEtoEnum calculationStrategy, IEnumerable<IPsdzSecureTokenEto> featureActivationTokens, IEnumerable<IPsdzDiagAddress> diagAddressCtos, IEnumerable<IPsdzFeatureIdCto> featureIdWhiteList, IEnumerable<IPsdzFeatureIdCto> featureIdBlackList, bool suppressCreationOfSfaWriteTA)
        {
            try
            {
                GenerateTalForSfaOnlyRequestModel requestBodyObject = new GenerateTalForSfaOnlyRequestModel
                {
                    Svt = SvtMapper.Map(svt),
                    SfaCurrent = istSfa?.Select(FeatureLongStatusCtoMapper.Map).ToList(),
                    SfaTarget = sollSfa?.Select(EcuFeatureTokenRelationCtoMapper.Map).ToList(),
                    CalculationStrategy = new CalculationStrategyEtoMapper().GetValue(calculationStrategy),
                    FeatureActivationTokens = featureActivationTokens?.Select(SecureTokenEtoMapper.Map).ToList(),
                    DiagAdresses = diagAddressCtos?.Select(DiagAddressMapper.Map).ToList(),
                    FeatureIdWhiteList = featureIdWhiteList?.Select(FeatureIdCtoMapper.Map).ToList(),
                    FeatureIdBlackList = featureIdBlackList?.Select(FeatureIdCtoMapper.Map).ToList(),
                    SuppressCreationOfSfaWriteTA = suppressCreationOfSfaWriteTA
                };
                return TalMapper.Map(_webCallHandler.ExecuteRequest<TalModel>(endpointService, "generatetalforsfaonly", HttpMethod.Post, requestBodyObject).Data);
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }

        public IPsdzProgrammingProtectionDataCto GenerateSweListForProgrammingProtection(IPsdzConnection connection, IPsdzTal tal)
        {
            try
            {
                ProgrammingProtectionDataCtoRequestModel requestBodyObject = new ProgrammingProtectionDataCtoRequestModel
                {
                    Tal = TalMapper.Map(tal)
                };
                return ProgrammingProtectionDataCtoMapper.Map(_webCallHandler.ExecuteRequest<ProgrammingProtectionDataCtoModel>(endpointService, $"generateswelistforprogrammingprotection/{connection.Id}", HttpMethod.Post, requestBodyObject).Data);
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }

        public IEnumerable<IPsdzIstufe> GetPossibleIntegrationLevel(IPsdzFa fa)
        {
            try
            {
                GetPossibleIntegrationLevelRequestModel requestBodyObject = new GetPossibleIntegrationLevelRequestModel
                {
                    Fa = FaMapper.Map(fa)
                };
                IEnumerable<IPsdzIstufe> listOfIStufe = _webCallHandler.ExecuteRequest<List<ILevelModel>>(endpointService, "getpossibleintegrationlevel", HttpMethod.Post, requestBodyObject).Data?.Select(ILevelMapper.Map);
                return OrderIntegrationLevelsAscending(listOfIStufe);
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }

        public IPsdzTal RequestHDDUpdateTal(IPsdzConnection connection, IPsdzSvt svtActual, IPsdzSvt svtSoll, IPsdzSwtAction swtAction, string backupDataPath)
        {
            try
            {
                RequestHDDUpdateTalRequestModel requestBodyObject = new RequestHDDUpdateTalRequestModel
                {
                    SvtActual = SvtMapper.Map(svtActual),
                    SvtTarget = SvtMapper.Map(svtSoll),
                    SwtAction = SwtActionMapper.Map(swtAction),
                    BackupPath = backupDataPath
                };
                return TalMapper.Map(_webCallHandler.ExecuteRequest<TalModel>(endpointService, $"requesthddupdatetal/{connection.Id}", HttpMethod.Post, requestBodyObject).Data);
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }

        public IDictionary<IPsdzEcuIdentifier, IPsdzObdData> RequestRelevantObdData(IPsdzConnection connection, IPsdzSvt svtActual)
        {
            try
            {
                RequestRelevantObdDataRequestModel requestBodyObject = new RequestRelevantObdDataRequestModel
                {
                    SvtActual = SvtMapper.Map(svtActual)
                };
                return _webCallHandler.ExecuteRequest<RequestRelevantObdDataResponseModel>(endpointService, $"requestrelevantobddata/{connection.Id}", HttpMethod.Post, requestBodyObject).Data?.EcuToObdMap?.ToDictionary((KeyValuePairModel<EcuIdentifierModel, ObdDataModel> map) => EcuIdentifierMapper.Map(map.Key), (KeyValuePairModel<EcuIdentifierModel, ObdDataModel> map) => ObdDataMapper.Map(map.Value));
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }

        public IEnumerable<IPsdzSgbmId> RequestSweList(IPsdzTal tal, bool ignoreSwDelete)
        {
            try
            {
                RequestSweListRequestModel requestBodyObject = new RequestSweListRequestModel
                {
                    Tal = TalMapper.Map(tal),
                    IgnoreSwDelete = ignoreSwDelete
                };
                return _webCallHandler.ExecuteRequest<IList<SgbmIdModel>>(endpointService, "requestswelist", HttpMethod.Post, requestBodyObject).Data?.Select(SgbmIdMapper.Map);
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }

        private IList<IPsdzIstufe> OrderIntegrationLevelsAscending(IEnumerable<IPsdzIstufe> listOfIStufe)
        {
            return listOfIStufe.OrderBy((IPsdzIstufe i) => i.Value.Substring(i.Value.IndexOf('-'))).ToList();
        }

        public IPsdzTal FillFsc(IPsdzTal tal, IPsdzSwtAction swtAction)
        {
            throw new NotImplementedException("Not implemented in the PSdZ Web API.");
        }

        public IPsdzSollverbauung GenerateSollverbauungEinzelFlash(IPsdzConnection connection, IPsdzIstufe iStufeTarget, IPsdzIstufe iStufeShipment, IPsdzSvt svtActual, IPsdzFa faTarget, IEnumerable<IPsdzDiagAddress> ecusToBeFlashed)
        {
            throw new NotImplementedException("Not implemented in the PSdZ Web API.");
        }

        public IPsdzSvt GenerateSvtSoll(IPsdzConnection connection, IPsdzFa faActual, IPsdzFa faTarget, IPsdzSvt svtActual, IPsdzIstufe iStufeShipment, IPsdzIstufe iStufeActual, IPsdzIstufe iStufeTarget)
        {
            throw new NotImplementedException("Not implemented in the PSdZ Web API.");
        }

        public IPsdzTal ChangeSwtActionType(IPsdzTal tal, IEnumerable<IPsdzSwtApplicationId> swtApplicationIds, IEnumerable<PsdzSwtActionType> swtActionTypes)
        {
            throw new NotImplementedException("Not implemented in the PSdZ Web API.");
        }
    }
}