using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient
{
	class CertificateManagementServiceClient : PsdzClientBase<ICertificateManagementService>, ICertificateManagementService
	{
		public CertificateManagementServiceClient(Binding binding, EndpointAddress remoteAddress) : base(binding, remoteAddress)
		{
		}

		public CertificateManagementServiceClient(ChannelFactory<ICertificateManagementService> channelFactory) : base(channelFactory)
		{
		}

		public PsdzRequestEcuSecCheckingResult RequestEcuSecChecking(IPsdzConnection connection, IPsdzSvt svtIst, IPsdzEcuIdentifier[] ecus, int retries)
		{
			return base.CallFunction<PsdzRequestEcuSecCheckingResult>((ICertificateManagementService service) => service.RequestEcuSecChecking(connection, svtIst, ecus, retries));
		}

		public PsdzRequestEcuSecCheckingResult RequestEcuCertCheckingWithFiltering(IPsdzConnection connection, IPsdzSvt svtIst, IPsdzDiagAddress[] ecuWhiteList, IPsdzDiagAddress[] ecuBlackList, int retries)
		{
			return base.CallFunction<PsdzRequestEcuSecCheckingResult>((ICertificateManagementService service) => service.RequestEcuCertCheckingWithFiltering(connection, svtIst, ecuWhiteList, ecuBlackList, retries));
		}

		public PsdzFetchEcuCertCheckingResult FetchEcuSecChecking(IPsdzConnection connection, IPsdzSvt svtIst, IPsdzEcuIdentifier[] ecus, int retries)
		{
			return base.CallFunction<PsdzFetchEcuCertCheckingResult>((ICertificateManagementService service) => service.FetchEcuSecChecking(connection, svtIst, ecus, retries));
		}

		public PsdzCertMemoryObject[] CalculateBindingDistribution(PsdzCertMemoryObject[] bindingsFromCbb, PsdzCertMemoryObject[] bindingsFromVehicle)
		{
			return base.CallFunction<PsdzCertMemoryObject[]>((ICertificateManagementService service) => service.CalculateBindingDistribution(bindingsFromCbb, bindingsFromVehicle));
		}

		public PsdzCertMemoryObject[] DeleteCertificatesWithRole(string role, IPsdzEcuIdentifier[] ecus, PsdzCertMemoryObject[] memoryObjects)
		{
			return base.CallFunction<PsdzCertMemoryObject[]>((ICertificateManagementService service) => service.DeleteCertificatesWithRole(role, ecus, memoryObjects));
		}

		public PsdzFetchBindingCalculationResult FetchCertificatesBindingsAndKeypacks(PsdzBindingCalculationRequestId requestId)
		{
			return base.CallFunction<PsdzFetchBindingCalculationResult>((ICertificateManagementService service) => service.FetchCertificatesBindingsAndKeypacks(requestId));
		}

		public PsdzBindingCalculationRequestId RequestBindingCalculation(PsdzCertMemoryObject[] certificates, string[] cbbUrls, string client, string system, IPsdzVin vin, int retries, int timeout, string[] certificatesRevocationList)
		{
			return base.CallFunction<PsdzBindingCalculationRequestId>((ICertificateManagementService service) => service.RequestBindingCalculation(certificates, cbbUrls, client, system, vin, retries, timeout, certificatesRevocationList));
		}

		public PsdzReadCertMemoryObjectResult ReadSecurityMemoryObjects(IPsdzConnection connection, IPsdzSvt svtIst, IPsdzEcuIdentifier[] ecus, PsdzCertMemoryObjectType certMemoryObjectType)
		{
			return base.CallFunction<PsdzReadCertMemoryObjectResult>((ICertificateManagementService service) => service.ReadSecurityMemoryObjects(connection, svtIst, ecus, certMemoryObjectType));
		}

		public PsdzEcuFailureResponse[] WriteSecurityMemoryObjects(IPsdzConnection connection, IPsdzSvt svtIst, PsdzCertMemoryObject[] certificates)
		{
			return base.CallFunction<PsdzEcuFailureResponse[]>((ICertificateManagementService service) => service.WriteSecurityMemoryObjects(connection, svtIst, certificates));
		}

		public PsdzBindingCalculationFailure[] RequestCertificatesBindingsAndKeypacksOffline(PsdzCertMemoryObject[] certificates, string requestFile, string client, string system, IPsdzVin vin)
		{
			return base.CallFunction<PsdzBindingCalculationFailure[]>((ICertificateManagementService service) => service.RequestCertificatesBindingsAndKeypacksOffline(certificates, requestFile, client, system, vin));
		}

		public PsdzFetchBindingCalculationResult FetchCertificatesBindingsAndKeypacksOffline(string bindingsFile)
		{
			return base.CallFunction<PsdzFetchBindingCalculationResult>((ICertificateManagementService service) => service.FetchCertificatesBindingsAndKeypacksOffline(bindingsFile));
		}

		public PsdzResponse[] CheckBackendConnection(string[] cbbUrls, string[] certificatesRevocationList, int timeout)
		{
			return base.CallFunction<PsdzResponse[]>((ICertificateManagementService service) => service.CheckBackendConnection(cbbUrls, certificatesRevocationList, timeout));
		}
	}
}
