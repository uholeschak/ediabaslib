using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.Psdz.Model;
using BMW.Rheingold.Psdz.Model.Ecu;
using BMW.Rheingold.Psdz.Model.Exceptions;
using BMW.Rheingold.Psdz.Model.Sfa;

namespace BMW.Rheingold.Psdz
{
    public enum PsdzStatusRequestFeatureTypeEtoEnum
    {
        ALL_FEATURES,
        SYSTEM_FEATURES,
        APPLICATION_FEATURES
    }

    [ServiceContract(SessionMode = SessionMode.Required, CallbackContract = typeof(IPsdzProgressListener))]
    [ServiceKnownType(typeof(PsdzVin))]
    [ServiceKnownType(typeof(PsdzSecurityBackendRequestIdEto))]
    [ServiceKnownType(typeof(PsdzConnection))]
    [ServiceKnownType(typeof(PsdzSvt))]
    [ServiceKnownType(typeof(PsdzEcuIdentifier))]
    [ServiceKnownType(typeof(PsdzReadStatusResultCto))]
    [ServiceKnownType(typeof(PsdzFetchCalculationSecureTokensResultCto))]
    [ServiceKnownType(typeof(PsdzReadLcsResultCto))]
    [ServiceKnownType(typeof(PsdzReadSecureEcuModeResultCto))]
    [ServiceKnownType(typeof(PsdzSecureTokenRequestCto))]
    [ServiceKnownType(typeof(PsdzSecureTokenForVehicleEto))]
    [ServiceKnownType(typeof(PsdzEcuLcsValueCto))]
    [ServiceKnownType(typeof(PsdzDiscoverFeatureStatusResultCto))]
    public interface ISecureFeatureActivationService
    {
        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        IPsdzDiscoverFeatureStatusResultCto DiscoverAllFeatures(IPsdzConnection connection, IPsdzSvt svt);

        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        IPsdzSecurityBackendRequestIdEto RequestNewestSecureTokenPackageForVehicle(IEnumerable<string> backendUrls, string client, string system, int retries, int timeout, IPsdzVin vin, IPsdzSvt svtIst, bool rebuildTokens);

        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        [Obsolete]
        IPsdzSecurityBackendRequestIdEto RequestDirectSecureTokensPackage(IEnumerable<string> backendUrlList, IEnumerable<string> crl, string client, string system, int retries, int timeout, IPsdzVin vin, IPsdzSvt svt, IPsdzSecureTokenRequestCto secureTokenRequest);

        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        IPsdzSecurityBackendRequestIdEto RequestDirectSecureTokensPackageWithoutCrlFiles(IEnumerable<string> backendUrlList, string client, string system, int retries, int timeout, IPsdzVin vin, IPsdzSvt svt, IPsdzSecureTokenRequestCto secureTokenRequest);

        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        IPsdzSecurityBackendRequestIdEto RequestDirectSecureTokensPackageWithoutCrlFilesWithReturnToken(IEnumerable<string> backendUrlList, string client, string system, int retries, int timeout, IPsdzVin vin, IPsdzSvt svt, IPsdzSecureTokenRequestCto secureTokenRequest);

        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        [Obsolete]
        IEnumerable<IPsdzSecurityBackendRequestFailureCto> RequestDirectSecureTokensPackageOffline(string filePath, string client, string system, IPsdzVin vin, IPsdzSvt svt, IPsdzSecureTokenRequestCto secureTokenRequest);

        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        IEnumerable<IPsdzSecurityBackendRequestFailureCto> RequestNewestSecureTokenPackageForEcuOffline(string requestFile, string client, string system, IPsdzVin vin, bool rebuildTokens, IPsdzEcuIdentifier ecu);

        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        IPsdzFetchCalculationSecureTokensResultCto FetchResultOfSecureTokenCalculation(IPsdzSecurityBackendRequestIdEto securityBackendRequestIdEto);

        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        IPsdzReadStatusResultCto ReadStatus(PsdzStatusRequestFeatureTypeEtoEnum statusRequestFeatureType, IPsdzConnection connection, IPsdzSvt svt, IEnumerable<IPsdzEcuIdentifier> ecus, bool extendedStatus, int retries, int timeBetweenRetries);

        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        IEnumerable<IPsdzSecurityBackendRequestFailureCto> RequestNewestSecureTokenPackageForVehicleOffline(string requestFilePath, string client, string system, IPsdzVin vin, bool rebuildTokens);

        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        IPsdzFetchCalculationSecureTokensResultCto FetchCalculationOfSecureTokensOffline(string secureTokenFilePath, IPsdzSvt svtIst);

        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        IPsdzReadLcsResultCto ReadLcs(IPsdzConnection pConnection, IPsdzSvt svt, IEnumerable<IPsdzEcuIdentifier> whitelistedECUs, IEnumerable<IPsdzEcuIdentifier> blacklistedECUs);

        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        IPsdzReadSecureEcuModeResultCto ReadSecureEcuMode(IPsdzConnection connection, IEnumerable<IPsdzEcuIdentifier> pEcus, IPsdzSvt svt);

        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        IEnumerable<IPsdzEcuFailureResponseCto> SwitchToSecureEcuFieldMode(IPsdzConnection connection, IEnumerable<IPsdzEcuIdentifier> pEcus, IPsdzSvt svt);

        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        IEnumerable<IPsdzEcuFailureResponseCto> SetLcs(IPsdzConnection pConnection, IEnumerable<IPsdzEcuLcsValueCto> pLcsValues, IPsdzSvt svt);

        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        IEnumerable<IPsdzEcuFailureResponseCto> WriteSecureToken(IPsdzConnection pConnection, IEnumerable<IPsdzSecureTokenEto> secureTokens, IPsdzSvt svt);

        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        IEnumerable<IPsdzEcuFailureResponseCto> WriteSecureTokenToECUs(IPsdzConnection pConnection, IPsdzSecureTokenForVehicleEto secureToken, IEnumerable<IPsdzEcuIdentifier> ecus, IPsdzSvt svt);

        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        IEnumerable<IPsdzEcuFailureResponseCto> DeleteSecureToken(IPsdzConnection pConnection, IPsdzEcuIdentifier psdzEcuIdentifier, IPsdzFeatureIdCto psdzFeatureId);

        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        IEnumerable<IPsdzEcuFailureResponseCto> ResetEcus(IPsdzConnection connection, IPsdzSvt svt, IEnumerable<IPsdzEcuIdentifier> ecusToBeReset);

        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        IEnumerable<IPsdzEcuFailureResponseCto> ResetEcusFlashMode(IPsdzConnection connection, IPsdzSvt svt, IEnumerable<IPsdzEcuIdentifier> ecusToBeReset, bool performWithFlashMode);

        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        IEnumerable<IPsdzSecurityBackendRequestFailureCto> RequestTokenDirectForVehicleOffline(string requestFilePath, string client, string system, IPsdzVin vin, IPsdzSvt svtIst, IPsdzSecureTokenRequestCto secureTokenRequest);
    }
}
