using BMW.Rheingold.Psdz;
using BMW.Rheingold.Psdz.Model;
using BMW.Rheingold.Psdz.Model.Ecu;
using BMW.Rheingold.Psdz.Model.Sfa;
using PsdzClient.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using RestSharp;

namespace BMW.Rheingold.Psdz
{
    internal class SecureFeatureActivationService : ISecureFeatureActivationService
    {
        private readonly string endpointService = "semsfa";
        private readonly IWebCallHandler _webCallHandler;
        public SecureFeatureActivationService(IWebCallHandler webCallHandler)
        {
            _webCallHandler = webCallHandler;
        }

        public IEnumerable<IPsdzEcuFailureResponseCto> DeleteSecureToken(IPsdzConnection connection, IPsdzEcuIdentifier psdzEcuIdentifier, IPsdzFeatureIdCto psdzFeatureId)
        {
            try
            {
                DeleteSecureTokenRequestModel requestBodyObject = new DeleteSecureTokenRequestModel
                {
                    EcuIdentifier = EcuIdentifierMapper.Map(psdzEcuIdentifier),
                    FeatureId = FeatureIdCtoMapper.Map(psdzFeatureId)
                };
                return _webCallHandler.ExecuteRequest<IList<EcuFailureResponseCtoModel>>(endpointService, $"deletesecuretoken/{connection.Id}", HttpMethod.Post, requestBodyObject).Data?.Select(EcuFailureResponseCtoMapper.MapCto).ToList();
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                return null;
            }
        }

        public IPsdzDiscoverFeatureStatusResultCto DiscoverAllFeatures(IPsdzConnection connection, IPsdzSvt svt)
        {
            try
            {
                DiscoverAllFeatureStatusRequestModel requestBodyObject = new DiscoverAllFeatureStatusRequestModel
                {
                    Svt = SvtMapper.Map(svt)
                };
                return DiscoverFeatureStatusResultCtoMapper.Map(_webCallHandler.ExecuteRequest<DiscoverFeatureStatusResultCtoModel>(endpointService, $"discoverallfeaturestatus/{connection.Id}", HttpMethod.Post, requestBodyObject).Data);
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }

        public IPsdzFetchCalculationSecureTokensResultCto FetchCalculationOfSecureTokensOffline(string secureTokenFilePath, IPsdzSvt svtIst)
        {
            try
            {
                FetchCalculationOfSecureTokensOfflineRequestModel requestBodyObject = new FetchCalculationOfSecureTokensOfflineRequestModel
                {
                    SecureTokenFilePath = secureTokenFilePath,
                    Svt = SvtMapper.Map(svtIst)
                };
                return FetchCalculationSecureTokensResultCtoMapper.Map(_webCallHandler.ExecuteRequest<FetchCalculationSecureTokensResultCtoModel>(endpointService, "fetchcalculationofsecuretokensoffline", HttpMethod.Post, requestBodyObject).Data);
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }

        public IPsdzFetchCalculationSecureTokensResultCto FetchResultOfSecureTokenCalculation(IPsdzSecurityBackendRequestIdEto securityBackendRequestIdEto)
        {
            try
            {
                FetchResultOfSecureTokenCalculationRequestModel requestBodyObject = new FetchResultOfSecureTokenCalculationRequestModel
                {
                    SecurityBackendRequestId = SecurityBackendRequestIdEtoMapper.Map(securityBackendRequestIdEto)
                };
                return FetchCalculationSecureTokensResultCtoMapper.Map(_webCallHandler.ExecuteRequest<FetchCalculationSecureTokensResultCtoModel>(endpointService, "fetchresultofsecuretokencalculation", HttpMethod.Post, requestBodyObject).Data);
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }

        public IPsdzReadLcsResultCto ReadLcs(IPsdzConnection connection, IPsdzSvt svt, IEnumerable<IPsdzEcuIdentifier> whitelistedECUs, IEnumerable<IPsdzEcuIdentifier> blacklistedECUs)
        {
            try
            {
                ReadLcsRequestModel requestBodyObject = new ReadLcsRequestModel
                {
                    Svt = SvtMapper.Map(svt),
                    WhitelistedEcus = whitelistedECUs?.Select(EcuIdentifierMapper.Map).ToList()
                };
                return ReadLcsResultCtoMapper.Map(_webCallHandler.ExecuteRequest<ReadLcsResultCtoModel>(endpointService, $"readlcs/{connection.Id}", HttpMethod.Post, requestBodyObject).Data);
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                return null;
            }
        }

        public IPsdzReadSecureEcuModeResultCto ReadSecureEcuMode(IPsdzConnection connection, IEnumerable<IPsdzEcuIdentifier> ecus, IPsdzSvt svt)
        {
            try
            {
                ReadSecureEcuModeRequestModel requestBodyObject = new ReadSecureEcuModeRequestModel
                {
                    Ecus = ecus?.Select(EcuIdentifierMapper.Map).ToList(),
                    Svt = SvtMapper.Map(svt)
                };
                return ReadSecureEcuModeResultCtoMapper.Map(_webCallHandler.ExecuteRequest<ReadSecureEcuModeResultCtoModel>(endpointService, $"readsecureecumode/{connection.Id}", HttpMethod.Post, requestBodyObject).Data);
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                return null;
            }
        }

        public IPsdzReadStatusResultCto ReadStatus(PsdzStatusRequestFeatureTypeEtoEnum statusRequestFeatureType, IPsdzConnection connection, IPsdzSvt svt, IEnumerable<IPsdzEcuIdentifier> ecus, bool extendedStatus, int retries, int timeBetweenRetries)
        {
            try
            {
                ReadStatusRequestModel requestBodyObject = new ReadStatusRequestModel
                {
                    StatusRequestFeatureType = new StatusRequestFeatureTypeEtoMapper().GetValue(statusRequestFeatureType),
                    Svt = SvtMapper.Map(svt),
                    Ecus = ecus?.Select(EcuIdentifierMapper.Map).ToList(),
                    ExtendedStatus = extendedStatus,
                    Retries = retries,
                    TimeBetweenRetries = timeBetweenRetries
                };
                return ReadStatusResultCtoMapper.Map(_webCallHandler.ExecuteRequest<ReadStatusResultCtoModel>(endpointService, $"readstatus/{connection.Id}", HttpMethod.Post, requestBodyObject).Data);
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }

        public IEnumerable<IPsdzSecurityBackendRequestFailureCto> RequestDirectSecureTokensPackageOffline(string filePath, string client, string system, IPsdzVin vin, IPsdzSvt svt, IPsdzSecureTokenRequestCto secureTokenRequest)
        {
            try
            {
                RequestDirectSecureTokensPackageOfflineRequestModel requestBodyObject = new RequestDirectSecureTokensPackageOfflineRequestModel
                {
                    Svt = SvtMapper.Map(svt),
                    Filepath = filePath,
                    Client = client,
                    System = system,
                    Vin = VinMapper.Map(vin),
                    SecureTokenRequest = SecureTokenRequestCtoMapper.Map(secureTokenRequest)
                };
                return _webCallHandler.ExecuteRequest<IList<SecurityBackendRequestFailureCtoModel>>(endpointService, "requestdirectsecuretokenspackageoffline", HttpMethod.Post, requestBodyObject).Data?.Select(SecurityBackendRequestFailureCtoMapper.Map).ToList();
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }

        public IPsdzSecurityBackendRequestIdEto RequestNewestSecureTokenPackageForVehicle(IEnumerable<string> backendUrls, string client, string system, int retries, int timeout, IPsdzVin vin, IPsdzSvt svtIst, bool rebuildTokens)
        {
            try
            {
                RequestNewestSecureTokenPackageForVehicleRequestModel requestBodyObject = new RequestNewestSecureTokenPackageForVehicleRequestModel
                {
                    BackendUrls = backendUrls?.ToList(),
                    Client = client,
                    System = system,
                    Retries = retries,
                    Timeout = timeout,
                    Vin = VinMapper.Map(vin),
                    SvtIst = SvtMapper.Map(svtIst),
                    RebuildTokens = rebuildTokens
                };
                return SecurityBackendRequestIdEtoMapper.Map(_webCallHandler.ExecuteRequest<SecurityBackendRequestIdEtoModel>(endpointService, "requestnewestsecuretokenpackageforvehicle", HttpMethod.Post, requestBodyObject).Data);
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }

        public IEnumerable<IPsdzSecurityBackendRequestFailureCto> RequestNewestSecureTokenPackageForVehicleOffline(string requestFilePath, string client, string system, IPsdzVin vin, bool rebuildTokens)
        {
            try
            {
                RequestNewestSecureTokenPackageForVehicleOfflineRequestModel requestBodyObject = new RequestNewestSecureTokenPackageForVehicleOfflineRequestModel
                {
                    RequestFilePath = requestFilePath,
                    Client = client,
                    System = system,
                    Vin = VinMapper.Map(vin),
                    RebuildTokens = rebuildTokens
                };
                return _webCallHandler.ExecuteRequest<IList<SecurityBackendRequestFailureCtoModel>>(endpointService, "requestnewestsecuretokenpackageforvehicleoffline", HttpMethod.Post, requestBodyObject).Data?.Select(SecurityBackendRequestFailureCtoMapper.Map).ToList();
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }

        public IEnumerable<IPsdzEcuFailureResponseCto> SetLcs(IPsdzConnection connection, IEnumerable<IPsdzEcuLcsValueCto> lcsValues, IPsdzSvt svt)
        {
            try
            {
                SetLcsRequestModel requestBodyObject = new SetLcsRequestModel
                {
                    EcuLcsValues = lcsValues?.Select(EcuLcsValueCtoMapper.Map).ToList(),
                    Svt = SvtMapper.Map(svt)
                };
                return _webCallHandler.ExecuteRequest<IList<EcuFailureResponseCtoModel>>(endpointService, $"setlcs/{connection.Id}", HttpMethod.Post, requestBodyObject).Data?.Select(EcuFailureResponseCtoMapper.MapCto).ToList();
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                return null;
            }
        }

        public IEnumerable<IPsdzEcuFailureResponseCto> SwitchToSecureEcuFieldMode(IPsdzConnection connection, IEnumerable<IPsdzEcuIdentifier> ecus, IPsdzSvt svt)
        {
            try
            {
                SwitchToSecureEcuModeFieldRequestModel requestBodyObject = new SwitchToSecureEcuModeFieldRequestModel
                {
                    Ecus = ecus?.Select(EcuIdentifierMapper.Map).ToList(),
                    Svt = SvtMapper.Map(svt)
                };
                return _webCallHandler.ExecuteRequest<IList<EcuFailureResponseCtoModel>>(endpointService, $"switchtosecureecumodefield/{connection.Id}", HttpMethod.Post, requestBodyObject).Data?.Select(EcuFailureResponseCtoMapper.MapCto).ToList();
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                return null;
            }
        }

        public IEnumerable<IPsdzEcuFailureResponseCto> WriteSecureToken(IPsdzConnection connection, IEnumerable<IPsdzSecureTokenEto> secureTokens, IPsdzSvt svt)
        {
            try
            {
                WriteSecureTokenRequestModel requestBodyObject = new WriteSecureTokenRequestModel
                {
                    SecureTokens = secureTokens?.Select(SecureTokenEtoMapper.Map).ToList(),
                    Svt = SvtMapper.Map(svt)
                };
                return _webCallHandler.ExecuteRequest<IList<EcuFailureResponseCtoModel>>(endpointService, $"writesecuretoken/{connection.Id}", HttpMethod.Post, requestBodyObject).Data?.Select(EcuFailureResponseCtoMapper.MapCto).ToList();
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                return null;
            }
        }

        public IPsdzSecurityBackendRequestIdEto RequestDirectSecureTokensPackageWithoutCrlFiles(IEnumerable<string> backendUrlList, string client, string system, int retries, int timeout, IPsdzVin vin, IPsdzSvt svt, IPsdzSecureTokenRequestCto secureTokenRequest)
        {
            try
            {
                RequestDirectSecureTokensPackageWithoutCrlFilesRequestModel requestBodyObject = new RequestDirectSecureTokensPackageWithoutCrlFilesRequestModel
                {
                    BackendUrlList = backendUrlList.ToList(),
                    Client = client,
                    System = system,
                    Retries = retries,
                    Timeout = timeout,
                    Vin = VinMapper.Map(vin),
                    Svt = SvtMapper.Map(svt),
                    SecureTokenRequest = SecureTokenRequestCtoMapper.Map(secureTokenRequest)
                };
                return SecurityBackendRequestIdEtoMapper.Map(_webCallHandler.ExecuteRequest<SecurityBackendRequestIdEtoModel>(endpointService, "requestdirectsecuretokenspackagewithoutcrlfiles", HttpMethod.Post, requestBodyObject).Data);
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                return null;
            }
        }

        public IPsdzSecurityBackendRequestIdEto RequestDirectSecureTokensPackageWithoutCrlFilesWithReturnToken(IEnumerable<string> backendUrlList, string client, string system, int retries, int timeout, IPsdzVin vin, IPsdzSvt svt, IPsdzSecureTokenRequestCto secureTokenRequest)
        {
            try
            {
                RequestDirectSecureTokensPackageWithoutCrlFilesWithReturnTokenRequestModel requestBodyObject = new RequestDirectSecureTokensPackageWithoutCrlFilesWithReturnTokenRequestModel
                {
                    BackendUrlList = backendUrlList.ToList(),
                    Client = client,
                    System = system,
                    Retries = retries,
                    Timeout = timeout,
                    Vin = VinMapper.Map(vin),
                    Svt = SvtMapper.Map(svt),
                    SecureTokenRequest = SecureTokenRequestCtoMapper.Map(secureTokenRequest)
                };
                return SecurityBackendRequestIdEtoMapper.Map(_webCallHandler.ExecuteRequest<SecurityBackendRequestIdEtoModel>(endpointService, "requestdirectsecuretokenspackagewithoutcrlfileswithreturntoken", HttpMethod.Post, requestBodyObject).Data);
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                return null;
            }
        }

        public IEnumerable<IPsdzEcuFailureResponseCto> ResetEcus(IPsdzConnection connection, IPsdzSvt svt, IEnumerable<IPsdzEcuIdentifier> ecusToBeReset)
        {
            try
            {
                ResetEcusRequestModel requestBodyObject = new ResetEcusRequestModel
                {
                    Svt = SvtMapper.Map(svt),
                    Ecus = ecusToBeReset?.Select(EcuIdentifierCtoMapper.Map).ToList()
                };
                return _webCallHandler.ExecuteRequest<IList<EcuFailureResponseCtoModel>>(endpointService, $"resetecus/{connection.Id}", HttpMethod.Post, requestBodyObject).Data?.Select(EcuFailureResponseCtoMapper.MapCto).ToList();
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                return null;
            }
        }

        public IEnumerable<IPsdzEcuFailureResponseCto> ResetEcusFlashMode(IPsdzConnection connection, IPsdzSvt svt, IEnumerable<IPsdzEcuIdentifier> ecusToBeReset, bool performWithFlashMode)
        {
            try
            {
                ResetEcusFlashModeRequestModel requestBodyObject = new ResetEcusFlashModeRequestModel
                {
                    Svt = SvtMapper.Map(svt),
                    Ecus = ecusToBeReset?.Select(EcuIdentifierCtoMapper.Map).ToList(),
                    PerformWithFlashMode = performWithFlashMode
                };
                return _webCallHandler.ExecuteRequest<IList<EcuFailureResponseCtoModel>>(endpointService, $"resetecus/{connection.Id}", HttpMethod.Post, requestBodyObject).Data?.Select(EcuFailureResponseCtoMapper.MapCto).ToList();
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                return null;
            }
        }

        public IEnumerable<IPsdzSecurityBackendRequestFailureCto> RequestTokenDirectForVehicleOffline(string requestFilePath, string client, string system, IPsdzVin vin, IPsdzSvt svtIst, IPsdzSecureTokenRequestCto secureTokenRequest)
        {
            try
            {
                RequestTokenDirectForVehicleOfflineRequestModel requestBodyObject = new RequestTokenDirectForVehicleOfflineRequestModel
                {
                    RequestFilePath = requestFilePath,
                    Client = client,
                    System = system,
                    Vin = VinMapper.Map(vin),
                    Svt = SvtMapper.Map(svtIst),
                    SecureTokenRequest = SecureTokenRequestCtoMapper.Map(secureTokenRequest)
                };
                return _webCallHandler.ExecuteRequest<IList<SecurityBackendRequestFailureCtoModel>>(endpointService, "requesttokendirectforvehicleoffline", HttpMethod.Post, requestBodyObject).Data?.Select(SecurityBackendRequestFailureCtoMapper.Map).ToList();
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                return null;
            }
        }

        public IEnumerable<IPsdzEcuFailureResponseCto> WriteSecureTokenToECUs(IPsdzConnection pConnection, IPsdzSecureTokenForVehicleEto secureToken, IEnumerable<IPsdzEcuIdentifier> ecus, IPsdzSvt svt)
        {
            try
            {
                WriteSecureTokenToECUsRequestModel requestBodyObject = new WriteSecureTokenToECUsRequestModel
                {
                    SecureToken = SecureTokenForVehicleEtoMapper.Map(secureToken),
                    Svt = SvtMapper.Map(svt),
                    Ecus = ecus?.Select(EcuIdentifierMapper.Map).ToList()
                };
                return _webCallHandler.ExecuteRequest<IList<EcuFailureResponseCtoModel>>(endpointService, $"writesecuretokentoecus/{pConnection.Id}", HttpMethod.Post, requestBodyObject).Data?.Select(EcuFailureResponseCtoMapper.MapCto).ToList();
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                return null;
            }
        }
    }
}