using BMW.Rheingold.Psdz.Model.Exceptions;
using BMW.Rheingold.Psdz.Model;
using System.ServiceModel;

namespace BMW.Rheingold.Psdz.Client
{
    [ServiceContract(SessionMode = SessionMode.Required, CallbackContract = typeof(IPsdzProgressListener))]
    [ServiceKnownType(typeof(SecureDiagnosticsCallback))]
    [ServiceKnownType(typeof(PsdzConnection))]
    public interface ISecureDiagnosticsService
    {
        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        void RegisterAuthService29Callback(byte[] s29CertificateChainByteArray, byte[] serializedPrivateKey, IPsdzConnection connection);

        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        void UnlockGateway(IPsdzConnection connection);
    }
}