using BMW.Rheingold.Psdz.Client;
using BMW.Rheingold.Psdz.Model;
using BMW.Rheingold.Psdz.Model.Exceptions;
using PsdzClient;
using System.ServiceModel;

namespace BMW.Rheingold.Psdz
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
    }
}