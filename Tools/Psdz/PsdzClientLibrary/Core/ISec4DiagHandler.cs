using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using System.Security.Cryptography.X509Certificates;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using PsdzClient.Contracts;

namespace PsdzClient.Core
{
    public enum Sec4DiagCertificateState
    {
        Valid,
        Expired,
        NotYetExpired,
        NotFound
    }

    public interface ISec4DiagHandler
    {
        int RoleMaskAsInt { get; }

        AsymmetricCipherKeyPair IstaKeyPair { get; set; }

        AsymmetricCipherKeyPair Service29KeyPair { get; set; }

        AsymmetricKeyParameter EdiabasPublicKey { get; set; }

        string CertificateFilePathWithoutEnding { get; }

        ISec4DiagCertificates Sec4DiagCertificates { get; set; }

        void InstallCertificate(X509Certificate2 cert);
        AsymmetricCipherKeyPair LoadKeyPairFromFile(string filePath, string password);
        Sec4DiagRequestData BuildRequestModel(string vin17);
        X509Certificate2 GenerateCertificate(Org.BouncyCastle.X509.X509Certificate issuerCert, AsymmetricKeyParameter publicKey, string vin);
        AsymmetricCipherKeyPair GenerateKeyPair();
        string SignData(string message, ECPrivateKeyParameters privateKey);
        Org.BouncyCastle.X509.X509Certificate CreateCertificateFromBase64(string base64Certificate);
        void WriteCertificateToFile(ISec4DiagCertificates sec4DiagResponse);
        byte[] CalculateProofOfOwnership(byte[] message);
        string ConvertToPEM(AsymmetricKeyParameter publicKey);
        void DeleteCertificateFile();
        void GenerateS29ForPSdZ(string vin);
        AsymmetricKeyParameter GetPublicKeyFromEdiabas();
        Sec4DiagCertificateState SearchForCertificatesInWindowsStore(string caThumbPrint, string subCaThumbPrint, out X509Certificate2Collection subCaCertificate, out X509Certificate2Collection caCertificate);
        void CreateS29CertificateInstallCertificatesAndWriteToFile(IVciDevice device, string subCa, string ca);
        void CreateS29CertificateInstallCertificatesAndWriteToFileForAos(IVciDevice device, string subCa, string ca, string s29);
        BoolResultObject CertificatesAreFoundAndValid(IVciDevice device, X509Certificate2Collection subCaCertificate, X509Certificate2Collection caCertificate);
        bool CheckIfEdiabasPublicKeyExists();
        string ReadoutExpirationTime();
    }
}