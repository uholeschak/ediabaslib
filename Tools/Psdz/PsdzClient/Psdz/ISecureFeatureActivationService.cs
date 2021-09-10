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

	[ServiceKnownType(typeof(PsdzSvt))]
	[ServiceKnownType(typeof(PsdzFetchCalculationSecureTokensResultCto))]
	[ServiceKnownType(typeof(PsdzReadStatusResultCto))]
	[ServiceKnownType(typeof(PsdzReadSecureEcuModeResultCto))]
	[ServiceKnownType(typeof(PsdzSecureTokenRequestCto))]
	[ServiceKnownType(typeof(PsdzEcuLcsValueCto))]
	[ServiceKnownType(typeof(PsdzVin))]
	[ServiceKnownType(typeof(PsdzReadLcsResultCto))]
	[ServiceKnownType(typeof(PsdzSecurityBackendRequestIdEto))]
	[ServiceKnownType(typeof(PsdzConnection))]
	[ServiceContract(SessionMode = SessionMode.Required, CallbackContract = typeof(IPsdzProgressListener))]
	[ServiceKnownType(typeof(PsdzEcuIdentifier))]
	public interface ISecureFeatureActivationService
	{
		[OperationContract]
		[FaultContract(typeof(PsdzRuntimeException))]
		IPsdzSecurityBackendRequestIdEto RequestNewestSecureTokenPackageForVehicle(IEnumerable<string> backendUrls, IEnumerable<string> certificatRevocations, string client, string system, int retries, int timeout, IPsdzVin vin, IPsdzSvt svtIst, bool rebuildTokens);

		[OperationContract]
		[FaultContract(typeof(PsdzRuntimeException))]
		IPsdzSecurityBackendRequestIdEto RequestDirectSecureTokensPackage(IEnumerable<string> backendUrlList, IEnumerable<string> crl, string client, string system, int retries, int timeout, IPsdzVin vin, IPsdzSvt svt, IPsdzSecureTokenRequestCto secureTokenRequest);

		[OperationContract]
		[FaultContract(typeof(PsdzRuntimeException))]
		IEnumerable<IPsdzSecurityBackendRequestFailureCto> RequestDirectSecureTokensPackageOffline(string filePath, string client, string system, IPsdzVin vin, IPsdzSvt svt, IPsdzSecureTokenRequestCto secureTokenRequest);

		[FaultContract(typeof(PsdzRuntimeException))]
		[OperationContract]
		IEnumerable<IPsdzSecurityBackendRequestFailureCto> RequestNewestSecureTokenPackageForEcuOffline(string requestFile, string client, string system, IPsdzVin vin, bool rebuildTokens, IPsdzEcuIdentifier ecu);

		[FaultContract(typeof(PsdzRuntimeException))]
		[OperationContract]
		IPsdzFetchCalculationSecureTokensResultCto FetchResultOfSecureTokenCalculation(IPsdzSecurityBackendRequestIdEto securityBackendRequestIdEto);

		[OperationContract]
		[FaultContract(typeof(PsdzRuntimeException))]
		IPsdzReadStatusResultCto ReadStatus(PsdzStatusRequestFeatureTypeEtoEnum statusRequestFeatureType, IPsdzConnection connection, IPsdzSvt svt, IEnumerable<IPsdzEcuIdentifier> ecus, bool extendedStatus, int retries, int timeBetweenRetries);

		[FaultContract(typeof(PsdzRuntimeException))]
		[OperationContract]
		IEnumerable<IPsdzSecurityBackendRequestFailureCto> RequestNewestSecureTokenPackageForVehicleOffline(string requestFilePath, string client, string system, IPsdzVin vin, bool rebuildTokens);

		[OperationContract]
		[FaultContract(typeof(PsdzRuntimeException))]
		IPsdzFetchCalculationSecureTokensResultCto FetchCalculationOfSecureTokensOffline(string secureTokenFilePath, IPsdzSvt svtIst);

		[FaultContract(typeof(PsdzRuntimeException))]
		[OperationContract]
		IPsdzReadLcsResultCto ReadLcs(IPsdzConnection pConnection, IPsdzSvt svt, IEnumerable<IPsdzEcuIdentifier> whitelistedECUs, IEnumerable<IPsdzEcuIdentifier> blacklistedECUs);

		[FaultContract(typeof(PsdzRuntimeException))]
		[OperationContract]
		IPsdzReadSecureEcuModeResultCto ReadSecureEcuMode(IPsdzConnection connection, IEnumerable<IPsdzEcuIdentifier> pEcus, IPsdzSvt svt);

		[FaultContract(typeof(PsdzRuntimeException))]
		[OperationContract]
		IEnumerable<IPsdzEcuFailureResponseCto> SwitchToSecureEcuFieldMode(IPsdzConnection connection, IEnumerable<IPsdzEcuIdentifier> pEcus, IPsdzSvt svt);

		[FaultContract(typeof(PsdzRuntimeException))]
		[OperationContract]
		IEnumerable<IPsdzEcuFailureResponseCto> SetLcs(IPsdzConnection pConnection, IEnumerable<IPsdzEcuLcsValueCto> pLcsValues);

		[FaultContract(typeof(PsdzRuntimeException))]
		[OperationContract]
		IEnumerable<IPsdzEcuFailureResponseCto> WriteSecureToken(IPsdzConnection pConnection, IEnumerable<IPsdzSecureTokenEto> secureTokens, IPsdzSvt svt);

		[OperationContract]
		[FaultContract(typeof(PsdzRuntimeException))]
		IEnumerable<IPsdzEcuFailureResponseCto> DeleteSecureToken(IPsdzConnection pConnection, IPsdzEcuIdentifier psdzEcuIdentifier, IPsdzFeatureIdCto psdzFeatureId);
	}
}
