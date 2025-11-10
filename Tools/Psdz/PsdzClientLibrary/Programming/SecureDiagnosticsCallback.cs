using BMW.Rheingold.Psdz.Model;
using System.Runtime.Serialization;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using PsdzClientLibrary;

namespace BMW.Rheingold.Psdz.Client
{
    [PreserveSource(Removed = true)]
    [DataContract]
    [ServiceKnownType(typeof(PsdzVin))]
    [ServiceKnownType(typeof(X509Certificate2))]
    internal class SecureDiagnosticsCallback : SecureDiagnosticsCallbackHandler
    {
        [DataMember]
        private readonly byte[] S29CertificateChainByteArray;

        [DataMember]
        private readonly byte[] SerializedPrivateKey;

        public SecureDiagnosticsCallback(byte[] s29CertificateChainByteArray, byte[] serializedPrivateKey)
        {
            S29CertificateChainByteArray = s29CertificateChainByteArray;
            SerializedPrivateKey = serializedPrivateKey;
        }

        public byte[] getAuthService29Certificate()
        {
            return S29CertificateChainByteArray;
        }

        public byte[] signAuthService29Challenge(byte[] par0)
        {
            return new Sec4DiagPoowHandler(SerializedPrivateKey).CalculateProofOfOwnership(par0);
        }
    }
}