using BMW.Rheingold.Psdz.Client;
using BMW.Rheingold.Psdz.Model;
using BMW.Rheingold.Psdz.Model.Exceptions;
using PsdzClient;
using System.ServiceModel;
using RheingoldPsdzWebApi.Adapter.Contracts;
using RheingoldPsdzWebApi.Adapter.Contracts.Model;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Services
{
    [PreserveSource(AttributesModified = true)]
    [ServiceContract(SessionMode = SessionMode.Required, CallbackContract = typeof(IPsdzProgressListener))]
    [ServiceKnownType(typeof(SecureDiagnosticsCallback))]
    [ServiceKnownType(typeof(PsdzConnection))]
    public interface ISecureDiagnosticsService
    {
        [PreserveSource(KeepAttribute = true)]
        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        void RegisterAuthService29Callback(byte[] s29CertificateChainByteArray, byte[] serializedPrivateKey, IPsdzConnection connection);
        [PreserveSource(KeepAttribute = true)]
        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        void UnlockGateway(IPsdzConnection connection);
        void CheckAndPerformAuthService29IfNeeded(IPsdzConnection connection);
    }
}