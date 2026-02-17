using Newtonsoft.Json;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Operators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Signers;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.X509.Extension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace EdiabasLib
{
    public static class EdSec4Diag
    {
        public const string S29ProofOfOwnershipData = "S29UNIPOO";
        public const string S29IstaSubCaSubjectName = "ST=Production,O=BMW Group,OU=Service29-PKI-SubCA,CN=Service29-subCA-ISTA";
        public const string S29IstaSubCaEmeaSubjectName = "C=DE,O=BMW Group,OU=Sec4Diag-PKI-CA,ST=Production,L=EMEA_RoW,CN=Service29-CA1-EMEA-2025";
        public const string S29ThumbprintCa = "BMW.Rheingold.CoreFramework.Ediabas.Thumbprint.Ca";
        public const string S29ThumbprintSubCa = "BMW.Rheingold.CoreFramework.Ediabas.Thumbprint.SubCa";
        public const string S29MachinePublicName = "_public.pem";
        public const string IstaPkcs12KeyPwd = "G#8x!9sD2@qZ6&lF1";
        public static byte[] RoleMask = new byte[] { 0x00, 0x00, 0x05, 0x4B };

        public class CertReqProfile
        {
            [DataContract]
            public enum EnumType
            {
                [EnumMember]
                crp_subCA_4ISTA,
                [EnumMember]
                crp_M2M_3rdParty_4_CUST_ReadWriteControl
            }
        }

        public class ProofOfPossession
        {
            [JsonProperty("signatureType")]
            public string SignatureType { get; set; }

            [JsonProperty("signature")]
            public string Signature { get; set; }
        }

        public class Sec4DiagRequestData
        {
            [JsonProperty("vin17")]
            public string Vin17 { get; set; }

            [JsonProperty("certReqProfile")]
            public string CertReqProfile { get; set; }

            [JsonProperty("publicKey")]
            public string PublicKey { get; set; }

            [JsonProperty("proofOfPossession")]
            public ProofOfPossession ProofOfPossession { get; set; }
        }

        public class Sec4DiagResponseData
        {
            [JsonProperty("vin17")]
            public string Vin17 { get; set; }

            [JsonProperty("certificate")]
            public string Certificate { get; set; }

            [JsonProperty("certificateChain")]
            public string[] CertificateChain { get; set; }
        }

        public static int RoleMaskAsInt
        {
            get
            {
                return BitConverter.ToInt32(RoleMask.Reverse().ToArray(), 0);
            }
        }

        private static BigInteger[] SignDataBytes(byte[] message, ECPrivateKeyParameters privateKey)
        {
            Sha512Digest sha512Digest = new Sha512Digest();
            sha512Digest.BlockUpdate(message, 0, message.Length);
            byte[] array = new byte[sha512Digest.GetDigestSize()];
            sha512Digest.DoFinal(array, 0);
            ECDsaSigner eCDsaSigner = new ECDsaSigner();
            eCDsaSigner.Init(forSigning: true, privateKey);
            return eCDsaSigner.GenerateSignature(array);
        }

        private static bool VerifyDataSignature(byte[] message, BigInteger[] signatureInts, ECPublicKeyParameters publicKey)
        {
            Sha512Digest sha512Digest = new Sha512Digest();
            sha512Digest.BlockUpdate(message, 0, message.Length);
            byte[] array = new byte[sha512Digest.GetDigestSize()];
            sha512Digest.DoFinal(array, 0);
            ECDsaSigner eCDsaSigner = new ECDsaSigner();
            eCDsaSigner.Init(forSigning: false, publicKey);
            return eCDsaSigner.VerifySignature(array, signatureInts[0], signatureInts[1]);
        }

        public static byte[] CalculateProofOfOwnership(byte[] server_challenge, ECPrivateKeyParameters privateKey)
        {
            try
            {
                if (server_challenge == null || privateKey == null)
                {
                    return null;
                }

                byte[] randomData = new byte[16];
                RandomNumberGenerator.Create().GetBytes(randomData);
                byte[] prefixBytes = Encoding.ASCII.GetBytes(S29ProofOfOwnershipData);
                int prefixLength = prefixBytes.Length;
                byte[] signData = new byte[prefixLength + randomData.Length + server_challenge.Length + 2];
                prefixBytes.CopyTo(signData, 0);
                randomData.CopyTo(signData, prefixLength);
                server_challenge.CopyTo(signData, prefixLength + randomData.Length);
                signData[signData.Length - 2] = 0;
                signData[signData.Length - 1] = 16;

                BigInteger[] signatureInts = SignDataBytes(signData, privateKey);
                byte[] integerPart1 = signatureInts[0].ToByteArrayUnsigned();
                byte[] integerPart2 = signatureInts[1].ToByteArrayUnsigned();
                byte[] integerData = new byte[integerPart1.Length + integerPart2.Length];
                Buffer.BlockCopy(integerPart1, 0, integerData, 0, integerPart1.Length);
                Buffer.BlockCopy(integerPart2, 0, integerData, integerPart1.Length, integerPart2.Length);

                byte[] resultData = new byte[randomData.Length + integerData.Length];
                Buffer.BlockCopy(randomData, 0, resultData, 0, randomData.Length);
                Buffer.BlockCopy(integerData, 0, resultData, randomData.Length, integerData.Length);

                return resultData;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static bool VerifyProofOfOwnership(byte[] proofData, byte[] server_challenge, ECPublicKeyParameters publicKey)
        {
            try
            {
                if (proofData == null || server_challenge == null || publicKey == null)
                {
                    return false;
                }

                if (proofData.Length < 16 + 2 * 48)
                {
                    return false; // Minimum length check for random data and signature integers
                }

                byte[] randomData = new byte[16];
                Buffer.BlockCopy(proofData, 0, randomData, 0, randomData.Length);
                byte[] prefixBytes = Encoding.ASCII.GetBytes(S29ProofOfOwnershipData);
                int prefixLength = prefixBytes.Length;
                byte[] signData = new byte[prefixLength + randomData.Length + server_challenge.Length + 2];
                prefixBytes.CopyTo(signData, 0);
                randomData.CopyTo(signData, prefixLength);
                server_challenge.CopyTo(signData, prefixLength + randomData.Length);
                signData[signData.Length - 2] = 0;
                signData[signData.Length - 1] = 16;

                BigInteger[] signatureInts = new BigInteger[2];
                int integerDataLength = (proofData.Length - randomData.Length) / 2;
                byte[] integerPart1 = new byte[integerDataLength];
                byte[] integerPart2 = new byte[integerDataLength];
                Buffer.BlockCopy(proofData, randomData.Length, integerPart1, 0, integerPart1.Length);
                Buffer.BlockCopy(proofData, randomData.Length + integerPart1.Length, integerPart2, 0, integerPart2.Length);
                signatureInts[0] = new BigInteger(1, integerPart1);
                signatureInts[1] = new BigInteger(1, integerPart2);

                if (!VerifyDataSignature(signData, signatureInts, publicKey))
                {
                    return false;
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static X509Certificate2 GenerateCertificate(Org.BouncyCastle.X509.X509Certificate issuerCert, AsymmetricKeyParameter publicKey, AsymmetricKeyParameter issuerPrivateKey,
            string vin)
        {
            string subjectName = $"ST=Production, O=BMW Group, OU=Service29-PKI-SubCA, CN=Service29-ISTA-S29, GIVENNAME={vin}";
            X509Name subject = new X509Name(subjectName);
            X509V3CertificateGenerator x509V3CertificateGenerator = new X509V3CertificateGenerator();
            x509V3CertificateGenerator.SetPublicKey(publicKey);
            x509V3CertificateGenerator.SetSerialNumber(BigInteger.ProbablePrime(120, new Random()));
            x509V3CertificateGenerator.SetIssuerDN(issuerCert.SubjectDN);
            x509V3CertificateGenerator.SetNotBefore(DateTime.UtcNow.AddMinutes(-5.0));
            x509V3CertificateGenerator.SetNotAfter(DateTime.UtcNow.AddYears(1));
            x509V3CertificateGenerator.SetSubjectDN(subject);
            DerObjectIdentifier oid = new DerObjectIdentifier("1.3.6.1.4.1.513.29.30");
            DerSet extensionValue = new DerSet(new Asn1EncodableVector {
                new DerOctetString(new byte[] { 0x0E, 0xF3 }),
                new DerOctetString(new byte[] { 0x0E, 0xF4 }),
                new DerOctetString(new byte[] { 0x0E, 0xF5 })
            });
            x509V3CertificateGenerator.AddExtension(oid, critical: true, extensionValue);
            DerObjectIdentifier oid2 = new DerObjectIdentifier("1.3.6.1.4.1.513.29.10");
            x509V3CertificateGenerator.AddExtension(oid2, critical: true, RoleMask);
            KeyUsage keyUsage = new KeyUsage(KeyUsage.DigitalSignature);
            x509V3CertificateGenerator.AddExtension(X509Extensions.KeyUsage, critical: false, keyUsage);
            x509V3CertificateGenerator.AddExtension(X509Extensions.SubjectKeyIdentifier, critical: false, X509ExtensionUtilities.CreateSubjectKeyIdentifier(publicKey));
            x509V3CertificateGenerator.AddExtension(X509Extensions.BasicConstraints, critical: true, new BasicConstraints(cA: false));
            x509V3CertificateGenerator.AddExtension(X509Extensions.AuthorityKeyIdentifier, critical: false, X509ExtensionUtilities.CreateAuthorityKeyIdentifier(issuerCert.GetPublicKey()));
            ISignatureFactory signatureFactory = new Asn1SignatureFactory("SHA512withECDSA", issuerPrivateKey);
            byte[] encodedCert = x509V3CertificateGenerator.Generate(signatureFactory).GetEncoded();
#if NET9_0_OR_GREATER
            return X509CertificateLoader.LoadCertificate(encodedCert);
#else
            return new X509Certificate2(encodedCert);
#endif
        }

        public static X509Certificate2 GenerateSubCaCertificate(Org.BouncyCastle.X509.X509Certificate issuerCert, AsymmetricKeyParameter publicKey, AsymmetricKeyParameter issuerPrivateKey,
            int caPathLength)
        {
            string subjectName = caPathLength > 0 ? S29IstaSubCaEmeaSubjectName : S29IstaSubCaSubjectName;
            X509Name subject = new X509Name(subjectName);
            X509V3CertificateGenerator x509V3CertificateGenerator = new X509V3CertificateGenerator();
            x509V3CertificateGenerator.SetPublicKey(publicKey);
            x509V3CertificateGenerator.SetSerialNumber(BigInteger.ProbablePrime(60, new Random()));
            x509V3CertificateGenerator.SetIssuerDN(issuerCert.SubjectDN);
            x509V3CertificateGenerator.SetNotBefore(DateTime.UtcNow.AddMinutes(-5.0));
            x509V3CertificateGenerator.SetNotAfter(DateTime.UtcNow.AddYears(1));
            x509V3CertificateGenerator.SetSubjectDN(subject);
            DerObjectIdentifier oid = new DerObjectIdentifier("1.3.6.1.4.1.513.29.70");

            DerSet extensionValue;
            if (caPathLength > 0)
            {
                extensionValue = new DerSet(new Asn1EncodableVector {
                    new DerOctetString(new byte[] { 0x0E, 0xF2 }),
                    new DerOctetString(new byte[] { 0x0E, 0xF3 }),
                    new DerOctetString(new byte[] { 0x0E, 0xF4 }),
                    new DerOctetString(new byte[] { 0x0E, 0xF5 }),
                    new DerOctetString(new byte[] { 0x0E, 0xFE })
                });
            }
            else
            {
                extensionValue = new DerSet(new Asn1EncodableVector {
                    new DerOctetString(new byte[] { 0x0E, 0xF3 }),
                    new DerOctetString(new byte[] { 0x0E, 0xF4 }),
                    new DerOctetString(new byte[] { 0x0E, 0xF5 })
                });
            }
            x509V3CertificateGenerator.AddExtension(oid, critical: true, extensionValue);

            DerObjectIdentifier oid2 = new DerObjectIdentifier("1.3.6.1.4.1.513.29.60");
            x509V3CertificateGenerator.AddExtension(oid2, critical: true, new byte[] { 0x00 });

            DerObjectIdentifier oid3 = new DerObjectIdentifier("1.3.6.1.4.1.513.29.50");
            x509V3CertificateGenerator.AddExtension(oid3, critical: true, RoleMask);

            KeyUsage keyUsage = new KeyUsage(KeyUsage.DigitalSignature | KeyUsage.KeyCertSign | KeyUsage.CrlSign);
            x509V3CertificateGenerator.AddExtension(X509Extensions.KeyUsage, critical: true, keyUsage);
            x509V3CertificateGenerator.AddExtension(X509Extensions.SubjectKeyIdentifier, critical: false, X509ExtensionUtilities.CreateSubjectKeyIdentifier(publicKey));
            x509V3CertificateGenerator.AddExtension(X509Extensions.BasicConstraints, critical: true, new BasicConstraints(pathLenConstraint: caPathLength));
            x509V3CertificateGenerator.AddExtension(X509Extensions.AuthorityKeyIdentifier, critical: false, X509ExtensionUtilities.CreateAuthorityKeyIdentifier(issuerCert.GetPublicKey()));
            ISignatureFactory signatureFactory = new Asn1SignatureFactory("SHA512withECDSA", issuerPrivateKey);
            byte[] encodedCert = x509V3CertificateGenerator.Generate(signatureFactory).GetEncoded();
#if NET9_0_OR_GREATER
            return X509CertificateLoader.LoadCertificate(encodedCert);
#else
            return new X509Certificate2(encodedCert);
#endif
        }

        public static string GetMachineName()
        {
#if ANDROID
            string machineName = Android.OS.Build.Fingerprint;
            if (!string.IsNullOrEmpty(machineName))
            {
                const int maxLength = 16;
                char[] machineBytes = machineName.Where(char.IsAsciiLetterOrDigit).ToArray();

                if (machineBytes.Length > maxLength)
                {
                    machineBytes = machineBytes.Take(maxLength).ToArray();
                }
                machineName = new string(machineBytes);
            }
#else
            string machineName = Environment.MachineName;
#endif
            if (string.IsNullOrEmpty(machineName))
            {
                machineName = "LocalMachine";
            }

            return machineName;
        }

#if !ANDROID
        public static bool SetIstaConfigString(string key, string value = "")
        {
            try
            {
                Microsoft.Win32.Registry.SetValue("HKEY_CURRENT_USER\\SOFTWARE\\BMWGroup\\ISPI\\Rheingold", key, value);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static string GetIstaConfigString(string key)
        {
            try
            {
                object value = Microsoft.Win32.Registry.GetValue("HKEY_CURRENT_USER\\SOFTWARE\\BMWGroup\\ISPI\\Rheingold", key, null);
                string text = value as string;
                if (string.IsNullOrEmpty(text))
                {
                    return string.Empty; // Value is not a string
                }

                return text;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public static void InstallCertificate(X509Certificate2 cert)
        {
            using (X509Store x509Store = new X509Store(StoreName.My, StoreLocation.CurrentUser))
            {
                x509Store.Open(OpenFlags.ReadWrite);
                x509Store.Add(cert);
                x509Store.Close();
            }
        }

        public static bool DeleteCertificateBySubjectName(string subjectName)
        {
            try
            {
                if (string.IsNullOrEmpty(subjectName))
                {
                    return false; // No subject name provided
                }

                bool deleted = false;
                using (X509Store x509Store = new X509Store(StoreName.My, StoreLocation.CurrentUser))
                {
                    x509Store.Open(OpenFlags.ReadWrite);
                    foreach (X509Certificate2 x509Certificate in x509Store.Certificates.Find(X509FindType.FindBySubjectName, subjectName, false))
                    {
                        x509Store.Remove(x509Certificate);
                        deleted = true;
                    }
                    x509Store.Close();
                }

                return deleted;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool DeleteCertificateByThumbprint(string thumbprint)
        {
            try
            {
                if (string.IsNullOrEmpty(thumbprint))
                {
                    return false; // No thumbprint provided
                }

                bool deleted = false;
                using (X509Store x509Store = new X509Store(StoreName.My, StoreLocation.CurrentUser))
                {
                    x509Store.Open(OpenFlags.ReadWrite);
                    foreach (X509Certificate2 x509Certificate in x509Store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, false))
                    {
                        x509Store.Remove(x509Certificate);
                        deleted = true;
                    }
                    x509Store.Close();
                }

                return deleted;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static X509Certificate2 GetCertificateFromStoreBySubjectName(string subjectName)
        {
            using (X509Store x509Store = new X509Store(StoreName.My, StoreLocation.CurrentUser))
            {
                X509Certificate2 x509Certificate;
                try
                {
                    x509Store.Open(OpenFlags.ReadWrite);
                    X509Certificate2Collection x509Certificate2Collection = x509Store.Certificates.Find(X509FindType.FindBySubjectName, subjectName, false);
                    if (x509Certificate2Collection.Count > 0)
                    {
                        x509Certificate = x509Certificate2Collection[0];
                        if (DateTime.UtcNow > x509Certificate.NotAfter.AddDays(-1.0))
                        {   // expires in less than 1 day
                            x509Store.Remove(x509Certificate);
                        }
                    }
                    else
                    {
                        x509Certificate = null;
                    }
                }
                catch (Exception)
                {
                    x509Certificate = null;
                }
                finally
                {
                    x509Store.Close();
                }
                return x509Certificate;
            }
        }

        public static X509Certificate2 GetCertificateFromStoreByThumbprint(string thumbprint)
        {
            using (X509Store x509Store = new X509Store(StoreName.My, StoreLocation.CurrentUser))
            {
                X509Certificate2 x509Certificate;
                try
                {
                    x509Store.Open(OpenFlags.ReadWrite);
                    X509Certificate2Collection x509Certificate2Collection = x509Store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, false);
                    if (x509Certificate2Collection.Count > 0)
                    {
                        x509Certificate = x509Certificate2Collection[0];
                        if (DateTime.UtcNow > x509Certificate.NotAfter.AddDays(-1.0))
                        {   // expires in less than 1 day
                            x509Store.Remove(x509Certificate);
                        }
                    }
                    else
                    {
                        x509Certificate = null;
                    }
                }
                catch (Exception)
                {
                    x509Certificate = null;
                }
                finally
                {
                    x509Store.Close();
                }
                return x509Certificate;
            }
        }

        public static bool InstallCertificates(List<Org.BouncyCastle.X509.X509Certificate> x509CertChain)
        {
            try
            {
                if (x509CertChain == null || x509CertChain.Count < 1)
                {
                    return false;
                }

                foreach (Org.BouncyCastle.X509.X509Certificate x509Certificate in x509CertChain)
                {
#if NET9_0_OR_GREATER
                    X509Certificate2 cert = X509CertificateLoader.LoadCertificate(x509Certificate.GetEncoded());
#else
                    X509Certificate2 cert = new X509Certificate2(x509Certificate.GetEncoded());
#endif
                    string cnName = cert.GetNameInfo(X509NameType.SimpleName, false);
                    DeleteCertificateBySubjectName(cnName);
                    InstallCertificate(cert);
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
#endif
    }
}
