using BMW.Rheingold.Psdz.Model;
using System.ServiceModel.Channels;
using System.ServiceModel;

namespace BMW.Rheingold.Psdz.Client
{
    internal class SecureDiagnosticsServiceClient : PsdzDuplexClientBase<ISecureDiagnosticsService, IPsdzProgressListener>, ISecureDiagnosticsService
    {
        public SecureDiagnosticsServiceClient(IPsdzProgressListener callbackInstance, Binding binding, EndpointAddress remoteAddress)
            : base(callbackInstance, binding, remoteAddress)
        {
        }

        public void RegisterAuthService29Callback(byte[] s29CertificateChainByteArray, byte[] serializedPrivateKey, IPsdzConnection connection)
        {
            CallMethod(delegate (ISecureDiagnosticsService service)
            {
                service.RegisterAuthService29Callback(s29CertificateChainByteArray, serializedPrivateKey, connection);
            });
        }

        public void UnlockGateway(IPsdzConnection connection)
        {
            CallMethod(delegate (ISecureDiagnosticsService service)
            {
                service.UnlockGateway(connection);
            });
        }
    }
}