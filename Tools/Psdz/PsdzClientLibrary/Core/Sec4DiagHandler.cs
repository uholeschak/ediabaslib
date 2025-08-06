using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Sec;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Operators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.X509.Extension;
using PsdzClient.Contracts;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using System.Text;
using System;

namespace PsdzClient.Core
{
    public sealed class Sec4DiagHandler : ISec4DiagHandler
    {
        private readonly byte[] roleMask = new byte[4] { 0, 0, 5, 75 };

        public AsymmetricCipherKeyPair IstaKeyPair { get; set; }

        public AsymmetricCipherKeyPair Service29KeyPair { get; set; }

        public ISec4DiagCertificates Sec4DiagCertificates { get; set; }

        public AsymmetricKeyParameter EdiabasPublicKey { get; set; }

        public string CertificateFilePathWithoutEnding { get; set; }

        public int RoleMaskAsInt => BitConverter.ToInt32(roleMask.Reverse().ToArray(), 0);

        private string _ediabaasS29Path { get; } = ConfigSettings.getConfigString("BMW.Rheingold.CoreFramework.Ediabas.S29Path", "..\\..\\..\\EDIABAS\\Security\\S29\\Certificates");

        private string _istaKeyPairPath { get; } = ConfigSettings.getConfigString("BMW.Rheingold.CoreFramework.Ista.KeyPair.Path", "..\\..\\..\\TesterGui\\keyContainer.pfx");

        private string _certificateFileNameWithoutEnding { get; } = $"certificates_{Process.GetCurrentProcess().Id}";

        public Sec4DiagHandler()
        {
            IstaKeyPair = LoadKeyPairFromFile(_istaKeyPairPath, "G#8x!9sD2@qZ6&lF1");
            Service29KeyPair = GenerateKeyPair();
        }

        public AsymmetricCipherKeyPair LoadKeyPairFromFile(string filePath, string password)
        {
            using (Stream input = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                Pkcs12Store pkcs12Store = new Pkcs12StoreBuilder().Build();
                pkcs12Store.Load(input, password.ToCharArray());
                foreach (string alias in pkcs12Store.Aliases)
                {
                    if (pkcs12Store.IsKeyEntry(alias))
                    {
                        AsymmetricKeyEntry key = pkcs12Store.GetKey(alias);
                        return new AsymmetricCipherKeyPair(pkcs12Store.GetCertificate(alias).Certificate.GetPublicKey(), key.Key);
                    }
                }
                throw new InvalidOperationException("Private key not found in PKCS#12 store.");
            }
        }

        public Sec4DiagRequestData BuildRequestModel(string vin17)
        {
            string empty = string.Empty;
            if (ConfigSettings.IsOssModeActive)
            {
                empty = CertReqProfile.EnumType.crp_M2M_3dParty_4_CUST_ControlOnly.ToString();
                return new Sec4DiagRequestData
                {
                    CertReqProfile = empty,
                    Vin17 = vin17,
                    PublicKey = ConvertToPEM(EdiabasPublicKey)
                };
            }
            empty = CertReqProfile.EnumType.crp_subCA_4ISTA.ToString();
            Sec4DiagRequestData sec4DiagRequestData = new Sec4DiagRequestData();
            sec4DiagRequestData.CertReqProfile = empty;
            sec4DiagRequestData.Vin17 = vin17;
            sec4DiagRequestData.PublicKey = "";
            sec4DiagRequestData.ProofOfPossession = new ProofOfPossession
            {
                SignatureType = "SHA512withECDSA"
            };
            string publicKey = ConvertToPEM(IstaKeyPair.Public);
            string message = vin17 + empty;
            string signature = SignData(message, (ECPrivateKeyParameters)IstaKeyPair.Private);
            sec4DiagRequestData.ProofOfPossession.Signature = signature;
            sec4DiagRequestData.PublicKey = publicKey;
            return sec4DiagRequestData;
        }

        public string SignData(string message, ECPrivateKeyParameters privateKey)
        {
            ISigner signer = SignerUtilities.GetSigner("SHA512withECDSA");
            signer.Init(forSigning: true, privateKey);
            byte[] bytes = Encoding.UTF8.GetBytes(message);
            signer.BlockUpdate(bytes, 0, bytes.Length);
            return Convert.ToBase64String(signer.GenerateSignature());
        }

        public Org.BouncyCastle.X509.X509Certificate CreateCertificateFromBase64(string base64Certificate)
        {
            return new X509CertificateParser().ReadCertificate(new MemoryStream(Convert.FromBase64String(base64Certificate)));
        }

        public void InstallCertificate(X509Certificate2 cert)
        {
            using (X509Store x509Store = new X509Store(StoreName.My, StoreLocation.CurrentUser))
            {
                x509Store.Open(OpenFlags.ReadWrite);
                x509Store.Add(cert);
                x509Store.Close();
            }
        }

#pragma warning disable CS0618 // Typ oder Element ist veraltet
        public X509Certificate2 GenerateCertificate(Org.BouncyCastle.X509.X509Certificate issuerCert, AsymmetricKeyParameter publicKey, string vin)
        {
            X509Name subject = GetSubject(vin);
            X509V3CertificateGenerator x509V3CertificateGenerator = new X509V3CertificateGenerator();
            x509V3CertificateGenerator.SetPublicKey(publicKey);
            x509V3CertificateGenerator.SetSerialNumber(BigInteger.ProbablePrime(120, new Random()));
            x509V3CertificateGenerator.SetIssuerDN(issuerCert.SubjectDN);
            x509V3CertificateGenerator.SetNotBefore(DateTime.UtcNow.AddMinutes(-5.0));
            x509V3CertificateGenerator.SetNotAfter(DateTime.UtcNow.AddDays(4.0));
            x509V3CertificateGenerator.SetSubjectDN(subject);
            DerObjectIdentifier oid = new DerObjectIdentifier("1.3.6.1.4.1.513.29.30");
            byte[] contents = new byte[2] { 14, 243 };
            byte[] contents2 = new byte[2] { 14, 244 };
            byte[] contents3 = new byte[2] { 14, 245 };
            DerOctetString element = new DerOctetString(contents);
            DerOctetString element2 = new DerOctetString(contents2);
            DerOctetString element3 = new DerOctetString(contents3);
            DerSet extensionValue = new DerSet(new Asn1EncodableVector { element, element2, element3 });
            x509V3CertificateGenerator.AddExtension(oid, critical: true, extensionValue);
            DerObjectIdentifier oid2 = new DerObjectIdentifier("1.3.6.1.4.1.513.29.10");
            x509V3CertificateGenerator.AddExtension(oid2, critical: true, roleMask);
            x509V3CertificateGenerator.AddExtension(X509Extensions.KeyUsage, critical: false, new KeyUsage(128));
            x509V3CertificateGenerator.AddExtension(X509Extensions.SubjectKeyIdentifier, critical: false, new SubjectKeyIdentifierStructure(publicKey));
            x509V3CertificateGenerator.AddExtension(X509Extensions.BasicConstraints, critical: true, new BasicConstraints(cA: false));
            x509V3CertificateGenerator.AddExtension(X509Extensions.AuthorityKeyIdentifier, critical: false, new AuthorityKeyIdentifierStructure(issuerCert.GetPublicKey()));
            ISignatureFactory signatureFactory = new Asn1SignatureFactory("SHA512withECDSA", IstaKeyPair.Private);
            return new X509Certificate2(x509V3CertificateGenerator.Generate(signatureFactory).GetEncoded());
        }
#pragma warning restore CS0618 // Typ oder Element ist veraltet

        public AsymmetricCipherKeyPair GenerateKeyPair()
        {
            ECKeyPairGenerator obj = (ECKeyPairGenerator)GeneratorUtilities.GetKeyPairGenerator("ECDSA");
            obj.Init(new ECKeyGenerationParameters(SecObjectIdentifiers.SecP384r1, new SecureRandom()));
            return obj.GenerateKeyPair();
        }

        public void WriteCertificateToFile(ISec4DiagCertificates sec4DiagResponse)
        {
            byte[] rawCertData = sec4DiagResponse.S29Cert.GetRawCertData();
            byte[] rawCertData2 = sec4DiagResponse.SubCaCert.GetRawCertData();
            byte[] rawCertData3 = sec4DiagResponse.CaCert.GetRawCertData();
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("-----BEGIN CERTIFICATE-----");
            stringBuilder.AppendLine(Convert.ToBase64String(rawCertData));
            stringBuilder.AppendLine("-----END CERTIFICATE-----");
            stringBuilder.AppendLine("-----BEGIN CERTIFICATE-----");
            stringBuilder.AppendLine(Convert.ToBase64String(rawCertData2));
            stringBuilder.AppendLine("-----END CERTIFICATE-----");
            stringBuilder.AppendLine("-----BEGIN CERTIFICATE-----");
            stringBuilder.AppendLine(Convert.ToBase64String(rawCertData3));
            stringBuilder.AppendLine("-----END CERTIFICATE-----");
            string contents = stringBuilder.ToString();
            CertificateFilePathWithoutEnding = _certificateFileNameWithoutEnding;
            File.WriteAllText(Path.Combine(_ediabaasS29Path ?? "", _certificateFileNameWithoutEnding + ".pem"), contents);
        }

        public byte[] CalculateProofOfOwnership(byte[] server_challenge)
        {
            ECPrivateKeyParameters eCPrivateKeyParameters = (ECPrivateKeyParameters)Service29KeyPair.Private;
            Stopwatch stopwatch = Stopwatch.StartNew();
            byte[] array = new byte[16];
            RandomNumberGenerator.Create().GetBytes(array);
            int num = Encoding.ASCII.GetBytes("S29UNIPOO").Length;
            BitConverter.GetBytes(10);
            byte[] array2 = new byte[num + array.Length + server_challenge.Length + 2];
            Encoding.ASCII.GetBytes("S29UNIPOO").CopyTo(array2, 0);
            array.CopyTo(array2, num);
            server_challenge.CopyTo(array2, num + array.Length);
            array2[num + array.Length + server_challenge.Length + 2 - 2] = 0;
            array2[num + array.Length + server_challenge.Length + 2 - 1] = 16;
            byte[] source = SignDataByte(array2, eCPrivateKeyParameters);
            int count = eCPrivateKeyParameters.Parameters.N.BitLength / 8;
            BigInteger bigInteger = new BigInteger(1, source.Take(count).ToArray());
            BigInteger bigInteger2 = new BigInteger(1, source.Skip(count).Take(count).ToArray());
            byte[] array3 = bigInteger.ToByteArrayUnsigned();
            byte[] array4 = bigInteger2.ToByteArrayUnsigned();
            byte[] array5 = new byte[array3.Length + array4.Length];
            Buffer.BlockCopy(array3, 0, array5, 0, array3.Length);
            Buffer.BlockCopy(array4, 0, array5, array3.Length, array4.Length);
            ISigner signer = SignerUtilities.GetSigner("SHA512withECDSA");
            signer.Init(forSigning: true, eCPrivateKeyParameters);
            signer.BlockUpdate(server_challenge, 0, server_challenge.Length);
            byte[] array6 = new byte[array.Length + array5.Length];
            Buffer.BlockCopy(array, 0, array6, 0, array.Length);
            Buffer.BlockCopy(array5, 0, array6, array.Length, array5.Length);
            byte[] array7 = new byte[2]
            {
                (byte)((array6.Length >> 8) & 0xFF),
                (byte)(array6.Length & 0xFF)
            };
            byte[] array8 = new byte[2 + array7.Length + array6.Length + 2];
            array8[0] = 41;
            array8[1] = 3;
            Buffer.BlockCopy(array7, 0, array8, 2, array7.Length);
            Buffer.BlockCopy(array6, 0, array8, 2 + array7.Length, array6.Length);
            array8[array8.Length - 2] = 0;
            array8[array8.Length - 1] = 0;
            stopwatch.Stop();
            Log.Info(Log.CurrentMethod(), $"The Proof of Ownership take: {0}s", stopwatch.Elapsed.Seconds);
            return array8;
        }

        public string ConvertToPEM(AsymmetricKeyParameter publicKey)
        {
            using (StringWriter stringWriter = new StringWriter())
            {
                using (PemWriter pemWriter = new PemWriter(stringWriter))
                {
                    pemWriter.WriteObject(publicKey);
                    pemWriter.Writer.Flush();
                    return stringWriter.ToString().Replace("-----BEGIN PUBLIC KEY-----", "").Replace("-----END PUBLIC KEY-----", "")
                        .Replace("\r", "")
                        .Replace("\n", "")
                        .Trim();
                }
            }
        }

        public void DeleteCertificateFile()
        {
            string path = Path.Combine(_ediabaasS29Path, _certificateFileNameWithoutEnding + ".pem");
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        public void GenerateS29ForPSdZ(string vin)
        {
            Sec4DiagCertificates.S29CertPSdZ = GenerateCertificate(DotNetUtilities.FromX509Certificate(Sec4DiagCertificates.SubCaCert), Service29KeyPair.Public, vin);
        }

        public AsymmetricKeyParameter GetPublicKeyFromEdiabas()
        {
            using (StreamReader reader = File.OpenText(Path.Combine(_ediabaasS29Path, Environment.MachineName + "_public.pem")))
            {
                using (PemReader pemReader = new PemReader(reader))
                {
                    return pemReader.ReadObject() as AsymmetricKeyParameter;
                }
            }
        }

        public bool CheckIfEdiabasPublicKeyExists()
        {
            return File.Exists(Path.Combine(_ediabaasS29Path, Environment.MachineName + "_public.pem"));
        }

        public Sec4DiagCertificateState SearchForCertificatesInWindowsStore(string caThumbPrint, string subCaThumbPrint, out X509Certificate2Collection subCaCertificate, out X509Certificate2Collection caCertificate)
        {
            string method = "ECUKom.SearchForCertificatesInWindowsStore";
            X509Store x509Store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            x509Store.Open(OpenFlags.ReadWrite);
            X509Certificate2Collection certificates = x509Store.Certificates;
            subCaCertificate = certificates.Find(X509FindType.FindByThumbprint, subCaThumbPrint, validOnly: false);
            caCertificate = certificates.Find(X509FindType.FindByThumbprint, caThumbPrint, validOnly: false);
            if (subCaCertificate.Count == 0 || caCertificate.Count == 0)
            {
                Log.Info(method, "Not Certification for given Thumbprint found");
                return Sec4DiagCertificateState.NotFound;
            }
            if (DateTime.Now < subCaCertificate[0].NotAfter.AddDays(-1.0) || DateTime.Now < caCertificate[0].NotAfter.AddDays(-1.0))
            {
                if (DateTime.Now > subCaCertificate[0].NotAfter.AddDays(-7.0) || DateTime.Now > caCertificate[0].NotAfter.AddDays(-7.0))
                {
                    Log.Info(method, "Certificte is over the 3 Weeks. We are requesting new Certificates but if this failes we are using the old one.");
                    return Sec4DiagCertificateState.NotYetExpired;
                }
                Org.BouncyCastle.X509.X509Certificate x509Certificate = DotNetUtilities.FromX509Certificate(subCaCertificate[0]);
                if (!AreKeyPairsEqual(x509Certificate.GetPublicKey(), IstaKeyPair))
                {
                    Log.Info(method, "Certificate does not match the KeyPair from ISTA");
                    return Sec4DiagCertificateState.NotFound;
                }
                Log.Info(method, "Certification for given Thumbprint found and valid");
                return Sec4DiagCertificateState.Valid;
            }
            Log.Info(method, "Certification for given Thumbprint found but not valid. Removing old once.");
            X509Certificate2Enumerator enumerator = subCaCertificate.GetEnumerator();
            while (enumerator.MoveNext())
            {
                X509Certificate2 current = enumerator.Current;
                x509Store.Remove(current);
            }
            enumerator = caCertificate.GetEnumerator();
            while (enumerator.MoveNext())
            {
                X509Certificate2 current2 = enumerator.Current;
                x509Store.Remove(current2);
            }
            ConfigSettings.putConfigString("BMW.Rheingold.CoreFramework.Ediabas.Thumbprint.Ca", string.Empty, overrideIsMaster: true);
            ConfigSettings.putConfigString("BMW.Rheingold.CoreFramework.Ediabas.Thumbprint.SubCa", string.Empty, overrideIsMaster: true);
            subCaCertificate = null;
            caCertificate = null;
            return Sec4DiagCertificateState.Expired;
        }

        public void CreateS29CertificateInstallCertificatesAndWriteToFile(IVciDevice device, string subCa, string ca, bool testRun)
        {
            Org.BouncyCastle.X509.X509Certificate x509Certificate = CreateCertificateFromBase64(subCa);
            Org.BouncyCastle.X509.X509Certificate x509Certificate2 = CreateCertificateFromBase64(ca);
            X509Certificate2 s29Cert = new X509Certificate2();
            if (!testRun)
            {
                s29Cert = GenerateCertificate(x509Certificate, EdiabasPublicKey, device.VIN);
            }
            Sec4DiagCertificates = new Sec4DiagCertificates
            {
                SubCaCert = new X509Certificate2(x509Certificate.GetEncoded()),
                CaCert = new X509Certificate2(x509Certificate2.GetEncoded()),
                S29Cert = s29Cert
            };
            if (!CoreFramework.OSSModeActive)
            {
                InstallCertificates(Sec4DiagCertificates);
            }
            if (!testRun)
            {
                WriteCertificateToFile(Sec4DiagCertificates);
            }
            Log.Info(Log.CurrentMethod(), "Certificates installed and written to file. Thumbprint added to Registry.");
        }

        private void InstallCertificates(ISec4DiagCertificates sec4DiagCertificates)
        {
            ConfigSettings.putConfigString("BMW.Rheingold.CoreFramework.Ediabas.Thumbprint.Ca", Sec4DiagCertificates.CaCert.Thumbprint, overrideIsMaster: true);
            ConfigSettings.putConfigString("BMW.Rheingold.CoreFramework.Ediabas.Thumbprint.SubCa", Sec4DiagCertificates.SubCaCert.Thumbprint, overrideIsMaster: true);
            InstallCertificate(Sec4DiagCertificates.SubCaCert);
            InstallCertificate(Sec4DiagCertificates.CaCert);
        }

        public BoolResultObject CertificatesAreFoundAndValid(IVciDevice device, X509Certificate2Collection subCaCertificate, X509Certificate2Collection caCertificate)
        {
            BoolResultObject boolResultObject = new BoolResultObject();
            if (Sec4DiagCertificates?.S29Cert != null)
            {
                boolResultObject.Result = true;
                return boolResultObject;
            }
            Org.BouncyCastle.X509.X509Certificate issuerCert = DotNetUtilities.FromX509Certificate(subCaCertificate[0]);
            X509Certificate2 s29Cert = GenerateCertificate(issuerCert, EdiabasPublicKey, device.VIN);
            Sec4DiagCertificates = new Sec4DiagCertificates
            {
                SubCaCert = subCaCertificate[0],
                CaCert = caCertificate[0],
                S29Cert = s29Cert
            };
            try
            {
                WriteCertificateToFile(Sec4DiagCertificates);
            }
            catch (IOException ex)
            {
                Log.ErrorException(Log.CurrentMethod(), ex);
                boolResultObject.ErrorMessage = ex.Message;
                boolResultObject.ErrorCodeInt = 4;
                return boolResultObject;
            }
            Log.Info(Log.CurrentMethod(), "Certificates are valid, S29 Certificate created and written to file.");
            boolResultObject.Result = true;
            return boolResultObject;
        }

        public string ReadoutExpirationTime()
        {
            string configString = ConfigSettings.getConfigString("BMW.Rheingold.CoreFramework.Ediabas.Thumbprint.SubCa");
            X509Store x509Store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            x509Store.Open(OpenFlags.ReadWrite);
            X509Certificate2Collection x509Certificate2Collection = x509Store.Certificates.Find(X509FindType.FindByThumbprint, configString, validOnly: false);
            if (x509Certificate2Collection.Count == 0)
            {
                Log.Info("ReadoutExpirationTime", "Not Certification for given Thumbprint found");
                return string.Empty;
            }
            return x509Certificate2Collection[0].GetExpirationDateString();
        }

        private bool AreKeyPairsEqual(AsymmetricKeyParameter subCaKeyPair, AsymmetricCipherKeyPair istaKeyPair)
        {
            return subCaKeyPair.Equals(istaKeyPair.Public);
        }

        private X509Name GetSubject(string vin)
        {
            return new X509Name("ST=Production, O=BMW Group, OU=Service29-PKI-SubCA, CN=Service29-ISTA-S29, GIVENNAME=" + vin);
        }

        private string StringToHex(string text)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            StringBuilder stringBuilder = new StringBuilder(bytes.Length * 2);
            byte[] array = bytes;
            foreach (byte b in array)
            {
                stringBuilder.AppendFormat("{0:X2}", b);
            }
            return stringBuilder.ToString();
        }

        private static byte[] SignDataByte(byte[] message, ECPrivateKeyParameters privateKey)
        {
            ISigner signer = SignerUtilities.GetSigner("SHA512withECDSA");
            signer.Init(forSigning: true, privateKey);
            signer.BlockUpdate(message, 0, message.Length);
            return signer.GenerateSignature();
        }
    }
}