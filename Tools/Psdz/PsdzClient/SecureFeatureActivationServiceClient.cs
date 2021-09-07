using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient
{
	class SecureFeatureActivationServiceClient : PsdzDuplexClientBase<ISecureFeatureActivationService, IPsdzProgressListener>, ISecureFeatureActivationService
	{
		internal SecureFeatureActivationServiceClient(IPsdzProgressListener progressListener, Binding binding, EndpointAddress remoteAddress) : base(progressListener, binding, remoteAddress)
		{
		}

		public IPsdzSecurityBackendRequestIdEto RequestNewestSecureTokenPackageForVehicle(IEnumerable<string> backendUrls, IEnumerable<string> certificatRevocations, string client, string system, int retries, int timeout, IPsdzVin vin, IPsdzSvt svtIst, bool rebuildTokens)
		{
			return base.CallFunction<IPsdzSecurityBackendRequestIdEto>((ISecureFeatureActivationService service) => service.RequestNewestSecureTokenPackageForVehicle(backendUrls, certificatRevocations, client, system, retries, timeout, vin, svtIst, rebuildTokens));
		}

		public IPsdzSecurityBackendRequestIdEto RequestDirectSecureTokensPackage(IEnumerable<string> backendUrlList, IEnumerable<string> crl, string client, string system, int retries, int timeout, IPsdzVin vin, IPsdzSvt svt, IPsdzSecureTokenRequestCto secureTokenRequest)
		{
			return base.CallFunction<IPsdzSecurityBackendRequestIdEto>((ISecureFeatureActivationService service) => service.RequestDirectSecureTokensPackage(backendUrlList, crl, client, system, retries, timeout, vin, svt, secureTokenRequest));
		}

		public IEnumerable<IPsdzSecurityBackendRequestFailureCto> RequestDirectSecureTokensPackageOffline(string filePath, string client, string system, IPsdzVin vin, IPsdzSvt svt, IPsdzSecureTokenRequestCto secureTokenRequest)
		{
			return base.CallFunction<IEnumerable<IPsdzSecurityBackendRequestFailureCto>>((ISecureFeatureActivationService service) => service.RequestDirectSecureTokensPackageOffline(filePath, client, system, vin, svt, secureTokenRequest));
		}

		public IEnumerable<IPsdzSecurityBackendRequestFailureCto> RequestNewestSecureTokenPackageForEcuOffline(string requestFile, string client, string system, IPsdzVin vin, bool rebuildTokens, IPsdzEcuIdentifier ecu)
		{
			return base.CallFunction<IEnumerable<IPsdzSecurityBackendRequestFailureCto>>((ISecureFeatureActivationService service) => service.RequestNewestSecureTokenPackageForEcuOffline(requestFile, client, system, vin, rebuildTokens, ecu));
		}

		public IPsdzFetchCalculationSecureTokensResultCto FetchResultOfSecureTokenCalculation(IPsdzSecurityBackendRequestIdEto securityBackendRequestIdEto)
		{
			return base.CallFunction<IPsdzFetchCalculationSecureTokensResultCto>((ISecureFeatureActivationService service) => service.FetchResultOfSecureTokenCalculation(securityBackendRequestIdEto));
		}

		public IPsdzReadStatusResultCto ReadStatus(PsdzStatusRequestFeatureTypeEtoEnum statusRequestFeatureType, IPsdzConnection connection, IPsdzSvt svt, IEnumerable<IPsdzEcuIdentifier> ecus, bool extendedStatus, int retries, int timeBetweenRetries)
		{
			return base.CallFunction<IPsdzReadStatusResultCto>((ISecureFeatureActivationService service) => service.ReadStatus(statusRequestFeatureType, connection, svt, ecus, extendedStatus, retries, timeBetweenRetries));
		}

		public IEnumerable<IPsdzSecurityBackendRequestFailureCto> RequestNewestSecureTokenPackageForVehicleOffline(string requestFilePath, string client, string system, IPsdzVin vin, bool rebuildTokens)
		{
			return base.CallFunction<IEnumerable<IPsdzSecurityBackendRequestFailureCto>>((ISecureFeatureActivationService service) => service.RequestNewestSecureTokenPackageForVehicleOffline(requestFilePath, client, system, vin, rebuildTokens));
		}

		public IPsdzFetchCalculationSecureTokensResultCto FetchCalculationOfSecureTokensOffline(string secureTokenFilePath, IPsdzSvt svtIst)
		{
			return base.CallFunction<IPsdzFetchCalculationSecureTokensResultCto>((ISecureFeatureActivationService service) => service.FetchCalculationOfSecureTokensOffline(secureTokenFilePath, svtIst));
		}

		public IPsdzReadLcsResultCto ReadLcs(IPsdzConnection connection, IPsdzSvt svt, IEnumerable<IPsdzEcuIdentifier> whitelistedECUs, IEnumerable<IPsdzEcuIdentifier> blacklistedECUs)
		{
			return base.CallFunction<IPsdzReadLcsResultCto>((ISecureFeatureActivationService service) => service.ReadLcs(connection, svt, whitelistedECUs, blacklistedECUs));
		}

		public IPsdzReadSecureEcuModeResultCto ReadSecureEcuMode(IPsdzConnection connection, IEnumerable<IPsdzEcuIdentifier> pEcus, IPsdzSvt svt)
		{
			return base.CallFunction<IPsdzReadSecureEcuModeResultCto>((ISecureFeatureActivationService service) => service.ReadSecureEcuMode(connection, pEcus, svt));
		}

		public IEnumerable<IPsdzEcuFailureResponseCto> SwitchToSecureEcuFieldMode(IPsdzConnection connection, IEnumerable<IPsdzEcuIdentifier> pEcus, IPsdzSvt svt)
		{
			return base.CallFunction<IEnumerable<IPsdzEcuFailureResponseCto>>((ISecureFeatureActivationService service) => service.SwitchToSecureEcuFieldMode(connection, pEcus, svt));
		}

		public IEnumerable<IPsdzEcuFailureResponseCto> SetLcs(IPsdzConnection connection, IEnumerable<IPsdzEcuLcsValueCto> pLcsValues)
		{
			return base.CallFunction<IEnumerable<IPsdzEcuFailureResponseCto>>((ISecureFeatureActivationService service) => service.SetLcs(connection, pLcsValues));
		}

		public IEnumerable<IPsdzEcuFailureResponseCto> WriteSecureToken(IPsdzConnection pConnection, IEnumerable<IPsdzSecureTokenEto> secureTokens, IPsdzSvt svt)
		{
			return base.CallFunction<IEnumerable<IPsdzEcuFailureResponseCto>>((ISecureFeatureActivationService service) => service.WriteSecureToken(pConnection, secureTokens, svt));
		}

		public IEnumerable<IPsdzEcuFailureResponseCto> DeleteSecureToken(IPsdzConnection pConnection, IPsdzEcuIdentifier psdzEcuIdentifier, IPsdzFeatureIdCto psdzFeatureId)
		{
			return base.CallFunction<IEnumerable<IPsdzEcuFailureResponseCto>>((ISecureFeatureActivationService service) => service.DeleteSecureToken(pConnection, psdzEcuIdentifier, psdzFeatureId));
		}
	}
}
