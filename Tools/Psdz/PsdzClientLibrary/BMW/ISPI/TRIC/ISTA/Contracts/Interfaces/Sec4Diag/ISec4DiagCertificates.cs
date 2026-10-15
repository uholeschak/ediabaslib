using System.Security.Cryptography.X509Certificates;

namespace BMW.ISPI.TRIC.ISTA.Contracts.Interfaces.Sec4Diag
{
    public interface ISec4DiagCertificates
    {
        X509Certificate2 S29Cert { get; set; }

        X509Certificate2 SubCaCert { get; set; }

        X509Certificate2 CaCert { get; set; }

        X509Certificate2 S29CertPSdZ { get; set; }
    }
}