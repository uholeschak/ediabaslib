using System.Security.Cryptography.X509Certificates;
using BMW.ISPI.TRIC.ISTA.Contracts.Interfaces.Sec4Diag;

namespace BMW.ISPI.TRIC.ISTA.Contracts.Implementations.Sec4Diag
{
    public sealed class Sec4DiagCertificates : ISec4DiagCertificates
    {
        public X509Certificate2 S29Cert { get; set; }

        public X509Certificate2 SubCaCert { get; set; }

        public X509Certificate2 CaCert { get; set; }

        public X509Certificate2 S29CertPSdZ { get; set; }
    }
}