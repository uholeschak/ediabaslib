using System.Security.Cryptography.X509Certificates;

namespace PsdzClient.Core
{
    public sealed class Sec4DiagCertificates : ISec4DiagCertificates
    {
        public X509Certificate2 S29Cert { get; set; }

        public X509Certificate2 SubCaCert { get; set; }

        public X509Certificate2 CaCert { get; set; }

        public X509Certificate2 S29CertPSdZ { get; set; }
    }
}