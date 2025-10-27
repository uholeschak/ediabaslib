using System.ServiceModel;
using BMW.Rheingold.Psdz.Model;
using BMW.Rheingold.Psdz.Model.Certificate;
using BMW.Rheingold.Psdz.Model.Ecu;

namespace BMW.Rheingold.Psdz
{
    [ServiceContract(SessionMode = SessionMode.Required)]
    [ServiceKnownType(typeof(PsdzSvt))]
    [ServiceKnownType(typeof(PsdzConnection))]
    [ServiceKnownType(typeof(PsdzDiagAddress))]
    [ServiceKnownType(typeof(PsdzEcuIdentifier))]
    [ServiceKnownType(typeof(PsdzVin))]
    public interface ICertificateManagementService
    {
        [OperationContract]
        PsdzRequestEcuSecCheckingResult RequestEcuSecChecking(IPsdzConnection connection, IPsdzSvt svtIst, IPsdzEcuIdentifier[] ecus, int retries);

        [OperationContract]
        PsdzFetchEcuCertCheckingResult FetchEcuSecChecking(IPsdzConnection connection, IPsdzSvt svtIst, IPsdzEcuIdentifier[] ecus, int retries);

        [OperationContract]
        PsdzCertMemoryObject[] CalculateBindingDistribution(PsdzCertMemoryObject[] bindingsFromCbb, PsdzCertMemoryObject[] bindingsFromVehicle);

        [OperationContract]
        PsdzCertMemoryObject[] DeleteCertificatesWithRole(string role, IPsdzEcuIdentifier[] ecus, PsdzCertMemoryObject[] memoryObjects);

        [OperationContract]
        PsdzFetchBindingCalculationResult FetchCertificatesBindingsAndKeypacks(PsdzBindingCalculationRequestId requestId);

        [OperationContract]
        PsdzReadCertMemoryObjectResult ReadSecurityMemoryObjects(IPsdzConnection connection, IPsdzSvt svtIst, IPsdzEcuIdentifier[] ecus, PsdzCertMemoryObjectType certMemoryObjectType);

        [OperationContract]
        PsdzEcuFailureResponse[] WriteSecurityMemoryObjects(IPsdzConnection connection, IPsdzSvt svtIst, PsdzCertMemoryObject[] certificates);

        [OperationContract]
        PsdzBindingCalculationFailure[] RequestCertificatesBindingsAndKeypacksOffline(PsdzCertMemoryObject[] certificates, string requestFile, string client, string system, IPsdzVin vin, IPsdzSvt svt);

        [OperationContract]
        PsdzFetchBindingCalculationResult FetchCertificatesBindingsAndKeypacksOffline(string bindingsFile);

        [OperationContract]
        PsdzResponse[] CheckBackendConnection(string[] cbbUrls, int timeout);
    }
}
