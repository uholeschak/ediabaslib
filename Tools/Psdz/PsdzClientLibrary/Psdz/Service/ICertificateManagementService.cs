using BMW.Rheingold.Psdz.Model;
using BMW.Rheingold.Psdz.Model.Certificate;
using BMW.Rheingold.Psdz.Model.Ecu;
using PsdzClient;
using System.ServiceModel;

namespace BMW.Rheingold.Psdz
{
    [PreserveSource(AttributesModified = true)]
    [ServiceContract(SessionMode = SessionMode.Required)]
    [ServiceKnownType(typeof(PsdzSvt))]
    [ServiceKnownType(typeof(PsdzConnection))]
    [ServiceKnownType(typeof(PsdzDiagAddress))]
    [ServiceKnownType(typeof(PsdzEcuIdentifier))]
    [ServiceKnownType(typeof(PsdzVin))]
    public interface ICertificateManagementService
    {
        [PreserveSource(KeepAttribute = true)]
        [OperationContract]
        PsdzRequestEcuSecCheckingResult RequestEcuSecChecking(IPsdzConnection connection, IPsdzSvt svtIst, IPsdzEcuIdentifier[] ecus, int retries);

        [PreserveSource(KeepAttribute = true)]
        [OperationContract]
        PsdzFetchEcuCertCheckingResult FetchEcuSecChecking(IPsdzConnection connection, IPsdzSvt svtIst, IPsdzEcuIdentifier[] ecus, int retries);

        [PreserveSource(KeepAttribute = true)]
        [OperationContract]
        PsdzCertMemoryObject[] CalculateBindingDistribution(PsdzCertMemoryObject[] bindingsFromCbb, PsdzCertMemoryObject[] bindingsFromVehicle);

        [PreserveSource(KeepAttribute = true)]
        [OperationContract]
        PsdzCertMemoryObject[] DeleteCertificatesWithRole(string role, IPsdzEcuIdentifier[] ecus, PsdzCertMemoryObject[] memoryObjects);

        [PreserveSource(KeepAttribute = true)]
        [OperationContract]
        PsdzFetchBindingCalculationResult FetchCertificatesBindingsAndKeypacks(PsdzBindingCalculationRequestId requestId);

        [PreserveSource(KeepAttribute = true)]
        [OperationContract]
        PsdzReadCertMemoryObjectResult ReadSecurityMemoryObjects(IPsdzConnection connection, IPsdzSvt svtIst, IPsdzEcuIdentifier[] ecus, PsdzCertMemoryObjectType certMemoryObjectType);

        [PreserveSource(KeepAttribute = true)]
        [OperationContract]
        PsdzEcuFailureResponse[] WriteSecurityMemoryObjects(IPsdzConnection connection, IPsdzSvt svtIst, PsdzCertMemoryObject[] certificates);

        [PreserveSource(KeepAttribute = true)]
        [OperationContract]
        PsdzBindingCalculationFailure[] RequestCertificatesBindingsAndKeypacksOffline(PsdzCertMemoryObject[] certificates, string requestFile, string client, string system, IPsdzVin vin, IPsdzSvt svt);

        [PreserveSource(KeepAttribute = true)]
        [OperationContract]
        PsdzFetchBindingCalculationResult FetchCertificatesBindingsAndKeypacksOffline(string bindingsFile);

        [PreserveSource(KeepAttribute = true)]
        [OperationContract]
        PsdzResponse[] CheckBackendConnection(string[] cbbUrls, int timeout);
    }
}
