using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Channels;
using BMW.Rheingold.Psdz.Model;
using BMW.Rheingold.Psdz.Model.Ecu;
using BMW.Rheingold.Psdz.Model.Sfa;
using PsdzClient;

namespace BMW.Rheingold.Psdz.Client
{
    internal sealed class SecureFeatureActivationServiceClient : PsdzDuplexClientBase<ISecureFeatureActivationService, IPsdzProgressListener>, ISecureFeatureActivationService
    {
        internal SecureFeatureActivationServiceClient(IPsdzProgressListener progressListener, Binding binding, EndpointAddress remoteAddress)
            : base(progressListener, binding, remoteAddress)
        {
        }

        public IPsdzDiscoverFeatureStatusResultCto DiscoverAllFeatures(IPsdzConnection connection, IPsdzSvt svt)
        {
            return CallFunction((ISecureFeatureActivationService service) => service.DiscoverAllFeatures(connection, svt));
        }

        public IPsdzSecurityBackendRequestIdEto RequestNewestSecureTokenPackageForVehicle(IEnumerable<string> backendUrls, string client, string system, int retries, int timeout, IPsdzVin vin, IPsdzSvt svtIst, bool rebuildTokens)
        {
            return CallFunction((ISecureFeatureActivationService service) => service.RequestNewestSecureTokenPackageForVehicle(backendUrls, client, system, retries, timeout, vin, svtIst, rebuildTokens));
        }

        public IPsdzSecurityBackendRequestIdEto RequestDirectSecureTokensPackageWithoutCrlFiles(IEnumerable<string> backendUrlList, string client, string system, int retries, int timeout, IPsdzVin vin, IPsdzSvt svt, IPsdzSecureTokenRequestCto secureTokenRequest)
        {
            return CallFunction((ISecureFeatureActivationService service) => service.RequestDirectSecureTokensPackageWithoutCrlFiles(backendUrlList, client, system, retries, timeout, vin, svt, secureTokenRequest));
        }

        public IPsdzSecurityBackendRequestIdEto RequestDirectSecureTokensPackageWithoutCrlFilesWithReturnToken(IEnumerable<string> backendUrlList, string client, string system, int retries, int timeout, IPsdzVin vin, IPsdzSvt svt, IPsdzSecureTokenRequestCto secureTokenRequest)
        {
            return CallFunction((ISecureFeatureActivationService service) => service.RequestDirectSecureTokensPackageWithoutCrlFilesWithReturnToken(backendUrlList, client, system, retries, timeout, vin, svt, secureTokenRequest));
        }

        [Obsolete]
        public IEnumerable<IPsdzSecurityBackendRequestFailureCto> RequestDirectSecureTokensPackageOffline(string filePath, string client, string system, IPsdzVin vin, IPsdzSvt svt, IPsdzSecureTokenRequestCto secureTokenRequest)
        {
            return CallFunction((ISecureFeatureActivationService service) => service.RequestDirectSecureTokensPackageOffline(filePath, client, system, vin, svt, secureTokenRequest));
        }

        public IPsdzFetchCalculationSecureTokensResultCto FetchResultOfSecureTokenCalculation(IPsdzSecurityBackendRequestIdEto securityBackendRequestIdEto)
        {
            return CallFunction((ISecureFeatureActivationService service) => service.FetchResultOfSecureTokenCalculation(securityBackendRequestIdEto));
        }

        public IPsdzReadStatusResultCto ReadStatus(PsdzStatusRequestFeatureTypeEtoEnum statusRequestFeatureType, IPsdzConnection connection, IPsdzSvt svt, IEnumerable<IPsdzEcuIdentifier> ecus, bool extendedStatus, int retries, int timeBetweenRetries)
        {
            return CallFunction((ISecureFeatureActivationService service) => service.ReadStatus(statusRequestFeatureType, connection, svt, ecus, extendedStatus, retries, timeBetweenRetries));
        }

        public IEnumerable<IPsdzSecurityBackendRequestFailureCto> RequestNewestSecureTokenPackageForVehicleOffline(string requestFilePath, string client, string system, IPsdzVin vin, bool rebuildTokens)
        {
            return CallFunction((ISecureFeatureActivationService service) => service.RequestNewestSecureTokenPackageForVehicleOffline(requestFilePath, client, system, vin, rebuildTokens));
        }

        public IPsdzFetchCalculationSecureTokensResultCto FetchCalculationOfSecureTokensOffline(string secureTokenFilePath, IPsdzSvt svtIst)
        {
            return CallFunction((ISecureFeatureActivationService service) => service.FetchCalculationOfSecureTokensOffline(secureTokenFilePath, svtIst));
        }

        public IPsdzReadLcsResultCto ReadLcs(IPsdzConnection connection, IPsdzSvt svt, IEnumerable<IPsdzEcuIdentifier> whitelistedECUs, IEnumerable<IPsdzEcuIdentifier> blacklistedECUs)
        {
            return CallFunction((ISecureFeatureActivationService service) => service.ReadLcs(connection, svt, whitelistedECUs, blacklistedECUs));
        }

        public IPsdzReadSecureEcuModeResultCto ReadSecureEcuMode(IPsdzConnection connection, IEnumerable<IPsdzEcuIdentifier> pEcus, IPsdzSvt svt)
        {
            return CallFunction((ISecureFeatureActivationService service) => service.ReadSecureEcuMode(connection, pEcus, svt));
        }

        public IEnumerable<IPsdzEcuFailureResponseCto> SwitchToSecureEcuFieldMode(IPsdzConnection connection, IEnumerable<IPsdzEcuIdentifier> pEcus, IPsdzSvt svt)
        {
            return CallFunction((ISecureFeatureActivationService service) => service.SwitchToSecureEcuFieldMode(connection, pEcus, svt));
        }

        public IEnumerable<IPsdzEcuFailureResponseCto> SetLcs(IPsdzConnection connection, IEnumerable<IPsdzEcuLcsValueCto> pLcsValues, IPsdzSvt svt)
        {
            return CallFunction((ISecureFeatureActivationService service) => service.SetLcs(connection, pLcsValues, svt));
        }

        public IEnumerable<IPsdzEcuFailureResponseCto> WriteSecureToken(IPsdzConnection pConnection, IEnumerable<IPsdzSecureTokenEto> secureTokens, IPsdzSvt svt)
        {
            return CallFunction((ISecureFeatureActivationService service) => service.WriteSecureToken(pConnection, secureTokens, svt));
        }

        public IEnumerable<IPsdzEcuFailureResponseCto> WriteSecureTokenToECUs(IPsdzConnection pConnection, IPsdzSecureTokenForVehicleEto secureToken, IEnumerable<IPsdzEcuIdentifier> ecus, IPsdzSvt svt)
        {
            return CallFunction((ISecureFeatureActivationService service) => service.WriteSecureTokenToECUs(pConnection, secureToken, ecus, svt));
        }

        public IEnumerable<IPsdzEcuFailureResponseCto> DeleteSecureToken(IPsdzConnection pConnection, IPsdzEcuIdentifier psdzEcuIdentifier, IPsdzFeatureIdCto psdzFeatureId)
        {
            return CallFunction((ISecureFeatureActivationService service) => service.DeleteSecureToken(pConnection, psdzEcuIdentifier, psdzFeatureId));
        }

        public IEnumerable<IPsdzEcuFailureResponseCto> ResetEcus(IPsdzConnection connection, IPsdzSvt svt, IEnumerable<IPsdzEcuIdentifier> ecusToBeReset)
        {
            return CallFunction((ISecureFeatureActivationService service) => service.ResetEcus(connection, svt, ecusToBeReset));
        }

        public IEnumerable<IPsdzEcuFailureResponseCto> ResetEcusFlashMode(IPsdzConnection connection, IPsdzSvt svt, IEnumerable<IPsdzEcuIdentifier> ecusToBeReset, bool performWithFlashMode)
        {
            return CallFunction((ISecureFeatureActivationService service) => service.ResetEcusFlashMode(connection, svt, ecusToBeReset, performWithFlashMode));
        }

        [PreserveSource(Hint = "Dummy")]
        public IEnumerable<IPsdzSecurityBackendRequestFailureCto> RequestTokenDirectForVehicleOffline(string requestFilePath, string client, string system, IPsdzVin vin, IPsdzSvt svtIst, IPsdzSecureTokenRequestCto secureTokenRequest)
        {
            return CallFunction((ISecureFeatureActivationService service) => service.RequestTokenDirectForVehicleOffline(requestFilePath, client, system, vin, svtIst, secureTokenRequest));
        }
    }
}
