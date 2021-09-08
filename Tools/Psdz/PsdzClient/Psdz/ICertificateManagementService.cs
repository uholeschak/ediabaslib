using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Psdz
{
	[ServiceKnownType(typeof(PsdzConnection))]
	[ServiceKnownType(typeof(PsdzSvt))]
	[ServiceKnownType(typeof(PsdzVin))]
	[ServiceKnownType(typeof(PsdzEcuIdentifier))]
	[ServiceKnownType(typeof(PsdzDiagAddress))]
	[ServiceContract(SessionMode = SessionMode.Required)]
	public interface ICertificateManagementService
	{
		[OperationContract]
		PsdzRequestEcuSecCheckingResult RequestEcuSecChecking(IPsdzConnection connection, IPsdzSvt svtIst, IPsdzEcuIdentifier[] ecus, int retries);

		[OperationContract]
		PsdzRequestEcuSecCheckingResult RequestEcuCertCheckingWithFiltering(IPsdzConnection connection, IPsdzSvt svtIst, IPsdzDiagAddress[] ecuWhiteList, IPsdzDiagAddress[] ecuBlackList, int retries);

		[OperationContract]
		PsdzFetchEcuCertCheckingResult FetchEcuSecChecking(IPsdzConnection connection, IPsdzSvt svtIst, IPsdzEcuIdentifier[] ecus, int retries);

		[OperationContract]
		PsdzCertMemoryObject[] CalculateBindingDistribution(PsdzCertMemoryObject[] bindingsFromCbb, PsdzCertMemoryObject[] bindingsFromVehicle);

		[OperationContract]
		PsdzCertMemoryObject[] DeleteCertificatesWithRole(string role, IPsdzEcuIdentifier[] ecus, PsdzCertMemoryObject[] memoryObjects);

		[OperationContract]
		PsdzFetchBindingCalculationResult FetchCertificatesBindingsAndKeypacks(PsdzBindingCalculationRequestId requestId);

		[OperationContract]
		PsdzBindingCalculationRequestId RequestBindingCalculation(PsdzCertMemoryObject[] certificates, string[] cbbUrls, string client, string system, IPsdzVin vin, int retries, int timeout, string[] certificatesRevocationList);

		[OperationContract]
		PsdzReadCertMemoryObjectResult ReadSecurityMemoryObjects(IPsdzConnection connection, IPsdzSvt svtIst, IPsdzEcuIdentifier[] ecus, PsdzCertMemoryObjectType certMemoryObjectType);

		[OperationContract]
		PsdzEcuFailureResponse[] WriteSecurityMemoryObjects(IPsdzConnection connection, IPsdzSvt svtIst, PsdzCertMemoryObject[] certificates);

		[OperationContract]
		PsdzBindingCalculationFailure[] RequestCertificatesBindingsAndKeypacksOffline(PsdzCertMemoryObject[] certificates, string requestFile, string client, string system, IPsdzVin vin);

		[OperationContract]
		PsdzFetchBindingCalculationResult FetchCertificatesBindingsAndKeypacksOffline(string bindingsFile);

		[OperationContract]
		PsdzResponse[] CheckBackendConnection(string[] cbbUrls, string[] certificatesRevocationList, int timeout);
	}
}
