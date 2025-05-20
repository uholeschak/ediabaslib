using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.Sec;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Tls.Crypto.Impl.BC;
using Org.BouncyCastle.Tls.Crypto;
using Org.BouncyCastle.Tls;
using Org.BouncyCastle.Utilities.Encoders;
using Org.BouncyCastle.Utilities.IO.Pem;
using Org.BouncyCastle.Utilities;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Crypto.Operators;
using Org.BouncyCastle.Math;

namespace EdiabasLib
{
    public static class EdBcTlsUtilities
    {
        public static string Fingerprint(X509CertificateStructure c)
        {
            byte[] der = c.GetEncoded();
            byte[] hash = Sha256DigestOf(der);
            byte[] hexBytes = Hex.Encode(hash);
            string hex = Encoding.ASCII.GetString(hexBytes).ToUpperInvariant();

            StringBuilder fp = new StringBuilder();
            int i = 0;
            fp.Append(hex.Substring(i, 2));
            while ((i += 2) < hex.Length)
            {
                fp.Append(':');
                fp.Append(hex.Substring(i, 2));
            }
            return fp.ToString();
        }

        public static byte[] Sha256DigestOf(byte[] input)
        {
            return DigestUtilities.CalculateDigest("SHA256", input);
        }

        public static X509CertificateStructure LoadBcCertificateResource(string resource)
        {
            PemObject pem = LoadPemResource(resource);
            if (pem.Type.EndsWith("CERTIFICATE"))
            {
                return X509CertificateStructure.GetInstance(pem.Content);
            }
            throw new ArgumentException("doesn't specify a valid certificate", "resource");
        }

        public static List<X509CertificateStructure> LoadBcCertificateResources(string resource)
        {
            List<X509CertificateStructure> certificates = new List<X509CertificateStructure>();
            List<PemObject> pemObjects = LoadPemResources(resource);
            foreach (PemObject pem in pemObjects)
            {
                if (pem.Type.EndsWith("CERTIFICATE"))
                {
                    certificates.Add(X509CertificateStructure.GetInstance(pem.Content));
                }
                else
                {
                    throw new ArgumentException("doesn't specify a valid certificate", "resource");
                }
            }
            return certificates;
        }

        public static AsymmetricKeyParameter LoadBcPrivateKeyResource(string resource, string password = null)
        {
            PemObject pem = LoadPemResource(resource);
            if (pem.Type.Equals("PRIVATE KEY"))
            {
                return PrivateKeyFactory.CreateKey(pem.Content);
            }
            if (pem.Type.Equals("ENCRYPTED PRIVATE KEY"))
            {
                if (string.IsNullOrEmpty(password))
                {
                    throw new ArgumentException("password is required for encrypted private key", "password");
                }

                return PrivateKeyFactory.DecryptKey(password.ToCharArray(), EncryptedPrivateKeyInfo.GetInstance(pem.Content));
            }
            if (pem.Type.Equals("RSA PRIVATE KEY"))
            {
                RsaPrivateKeyStructure rsa = RsaPrivateKeyStructure.GetInstance(pem.Content);
                return new RsaPrivateCrtKeyParameters(rsa.Modulus, rsa.PublicExponent,
                    rsa.PrivateExponent, rsa.Prime1, rsa.Prime2, rsa.Exponent1,
                    rsa.Exponent2, rsa.Coefficient);
            }
            if (pem.Type.Equals("EC PRIVATE KEY"))
            {
                ECPrivateKeyStructure pKey = ECPrivateKeyStructure.GetInstance(pem.Content);
                AlgorithmIdentifier algId = new AlgorithmIdentifier(X9ObjectIdentifiers.IdECPublicKey, pKey.Parameters);
                PrivateKeyInfo privInfo = new PrivateKeyInfo(algId, pKey);
                return PrivateKeyFactory.CreateKey(privInfo);
            }
            throw new ArgumentException("doesn't specify a valid private key", "resource");
        }

        public static PemObject LoadPemResource(string resource)
        {
            using (StreamReader sr = new StreamReader(resource))
            {
                using (Org.BouncyCastle.OpenSsl.PemReader p = new Org.BouncyCastle.OpenSsl.PemReader(sr))
                {
                    return p.ReadPemObject();
                }
            }
        }

        public static object LoadPemObject(string resource)
        {
            using (StreamReader sr = new StreamReader(resource))
            {
                using (Org.BouncyCastle.OpenSsl.PemReader p = new Org.BouncyCastle.OpenSsl.PemReader(sr))
                {
                    return p.ReadObject();
                }
            }
        }

        public static List<PemObject> LoadPemResources(string resource)
        {
            List <PemObject> pemObjects = new List<PemObject>();
            using (StreamReader sr = new StreamReader(resource))
            {
                using (Org.BouncyCastle.OpenSsl.PemReader p = new Org.BouncyCastle.OpenSsl.PemReader(sr))
                {
                    for (; ; )
                    {
                        PemObject pemObject = p.ReadPemObject();
                        if (pemObject == null)
                        {
                            break;
                        }

                        pemObjects.Add(pemObject);
                    }
                }
            }

            return pemObjects;
        }

        public static TlsCredentialedDecryptor LoadEncryptionCredentials(TlsContext context, string[] certResources,
            string keyResource)
        {
            TlsCrypto crypto = context.Crypto;
            Certificate certificate = LoadCertificateChain(context, certResources);

            // TODO[tls-ops] Need to have TlsCrypto construct the credentials from the certs/key (as raw data)
            if (crypto is BcTlsCrypto)
            {
                AsymmetricKeyParameter privateKey = LoadBcPrivateKeyResource(keyResource);

                return new BcDefaultTlsCredentialedDecryptor((BcTlsCrypto)crypto, certificate, privateKey);
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public static TlsCredentialedSigner LoadSignerCredentials(TlsContext context,
            IList<SignatureAndHashAlgorithm> supportedSignatureAlgorithms, short signatureAlgorithm,
            string[] certResources, string keyResource)
        {
            if (supportedSignatureAlgorithms == null)
            {
                supportedSignatureAlgorithms = TlsUtilities.GetDefaultSignatureAlgorithms(signatureAlgorithm);
            }

            SignatureAndHashAlgorithm signatureAndHashAlgorithm = null;

            foreach (SignatureAndHashAlgorithm alg in supportedSignatureAlgorithms)
            {
                if (alg.Signature == signatureAlgorithm)
                {
                    // Just grab the first one we find
                    signatureAndHashAlgorithm = alg;
                    break;
                }
            }

            if (signatureAndHashAlgorithm == null)
                return null;

            return LoadSignerCredentials(context, certResources, keyResource, signatureAndHashAlgorithm);
        }

        public static TlsCredentialedSigner LoadSignerCredentials(TlsContext context, IList<SignatureAndHashAlgorithm> supportedSignatureAlgorithms, string[] certResources,
            string keyResource, SignatureAndHashAlgorithm[] signatureAndHashAlgorithms)
        {
            TlsCrypto crypto = context.Crypto;
            TlsCryptoParameters cryptoParams = new TlsCryptoParameters(context);

            foreach (SignatureAndHashAlgorithm signatureAndHashAlgorithm in signatureAndHashAlgorithms)
            {
                if (TlsUtilities.ContainsSignatureAlgorithm(supportedSignatureAlgorithms, signatureAndHashAlgorithm))
                {
                    TlsCredentialedSigner credentialedSigner = LoadSignerCredentials(cryptoParams, crypto, certResources, keyResource, signatureAndHashAlgorithm);
                    if (credentialedSigner != null)
                    {
                        return credentialedSigner;
                    }
                }
            }

            return null;
        }

        public static TlsCredentialedSigner LoadSignerCredentials(TlsContext context, string[] certResources,
            string keyResource, SignatureAndHashAlgorithm signatureAndHashAlgorithm)
        {
            TlsCrypto crypto = context.Crypto;
            TlsCryptoParameters cryptoParams = new TlsCryptoParameters(context);

            return LoadSignerCredentials(cryptoParams, crypto, certResources, keyResource, signatureAndHashAlgorithm);
        }

        public static TlsCredentialedSigner LoadSignerCredentials(TlsCryptoParameters cryptoParams, TlsCrypto crypto,
            string[] certResources, string keyResource, SignatureAndHashAlgorithm signatureAndHashAlgorithm)
        {
            if (cryptoParams?.ServerVersion == null)
            {
                throw new TlsFatalAlert(AlertDescription.protocol_version);
            }

            Certificate certificate = LoadCertificateChain(cryptoParams.ServerVersion, crypto, certResources);

            // TODO[tls-ops] Need to have TlsCrypto construct the credentials from the certs/key (as raw data)
            if (crypto is BcTlsCrypto)
            {
                AsymmetricKeyParameter privateKey = LoadBcPrivateKeyResource(keyResource);

                return new BcDefaultTlsCredentialedSigner(cryptoParams, (BcTlsCrypto)crypto, privateKey, certificate, signatureAndHashAlgorithm);
            }

            throw new NotSupportedException();
        }

        public static bool CheckCertificateChainCa(TlsCrypto crypto, TlsCertificate[] chain, X509Name[] trustedIssuers)
        {
            if (chain.Length < 1)
            {
                return false;
            }

            BcTlsCrypto bcTlsCrypto = crypto as BcTlsCrypto;
            if (bcTlsCrypto == null)
            {
                return false;
            }

            for (int i = chain.Length - 1; i >= 0; i--)
            {
                TlsCertificate tlsCertificate = chain[i];
                X509Name issuer = BcTlsCertificate.Convert(bcTlsCrypto, tlsCertificate)?.X509CertificateStructure?.Issuer;
                if (issuer == null)
                {
                    continue;
                }

                foreach (X509Name trustedIssuer in trustedIssuers)
                {
                    if (issuer.Equivalent(trustedIssuer))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static Certificate LoadCertificateChain(ProtocolVersion protocolVersion, TlsCrypto crypto, string[] resources)
        {
            if (protocolVersion == null)
            {
                throw new TlsFatalAlert(AlertDescription.protocol_version);
            }

            List<TlsCertificate> certificates = new List<TlsCertificate>();
            foreach (string resource in resources)
            {
                List<TlsCertificate> resourceCerts = LoadCertificateResources(crypto, resource);
                foreach (TlsCertificate resourceCert in resourceCerts)
                {
                    bool existing = false;
                    foreach (TlsCertificate cert in certificates)
                    {
                        if (AreSameCertificate(resourceCert, cert))
                        {
                            existing = true;
                            break;
                        }
                    }

                    if (existing)
                    {
                        continue;
                    }

                    certificates.Add(resourceCert);
                }
            }

            if (TlsUtilities.IsTlsV13(protocolVersion))
            {
                List<CertificateEntry> certificateEntryList = new List<CertificateEntry>();
                foreach (TlsCertificate certificate in certificates)
                {
                    certificateEntryList.Add(new CertificateEntry(certificate, null));
                }

                byte[] certificateRequestContext = TlsUtilities.EmptyBytes;
                return new Certificate(certificateRequestContext, certificateEntryList.ToArray());
            }

            return new Certificate(certificates.ToArray());
        }

        public static Certificate LoadCertificateChain(TlsContext context, string[] resources)
        {
            return LoadCertificateChain(context.ServerVersion, context.Crypto, resources);
        }

        public static TlsCertificate LoadCertificateResource(TlsCrypto crypto, string resource)
        {
            PemObject pem = LoadPemResource(resource);
            if (pem.Type.EndsWith("CERTIFICATE"))
            {
                return crypto.CreateCertificate(pem.Content);
            }
            throw new ArgumentException("doesn't specify a valid certificate", "resource");
        }

        public static List<TlsCertificate> LoadCertificateResources(TlsCrypto crypto, string resource)
        {
            List<TlsCertificate> certificates = new List<TlsCertificate>();
            List<PemObject> pemObjects = LoadPemResources(resource);
            foreach (PemObject pem in pemObjects)
            {
                if (pem.Type.EndsWith("CERTIFICATE"))
                {
                    certificates.Add(crypto.CreateCertificate(pem.Content));
                }
                else
                {
                    throw new ArgumentException("doesn't specify a valid certificate", "resource");
                }
            }

            return certificates;
        }

        public static bool AreSameCertificate(TlsCertificate a, TlsCertificate b)
        {
            // TODO[tls-ops] Support equals on TlsCertificate?
            return Arrays.AreEqual(a.GetEncoded(), b.GetEncoded());
        }

        public static TlsCertificate[] GetTrustedCertPath(TlsCrypto crypto, TlsCertificate cert, string[] resources, string caFile)
        {
            foreach (string eeCertResource in resources)
            {
                TlsCertificate eeCert = LoadCertificateResource(crypto, eeCertResource);
                if (AreSameCertificate(cert, eeCert))
                {
                    TlsCertificate caCert = LoadCertificateResource(crypto, caFile);
                    if (null != caCert)
                    {
                        return new TlsCertificate[] { eeCert, caCert };
                    }
                }
            }
            return null;
        }

        public static byte[] CreatePkcs12Data(string certResource, string keyResource, string password = null)
        {
            try
            {
                Pkcs12Store store = new Pkcs12StoreBuilder().Build();
                List<X509CertificateStructure> certificateStructures = LoadBcCertificateResources(certResource);
                if (certificateStructures.Count == 0)
                {
                    return null;
                }

                List<X509CertificateEntry> certificateEntries = new List<X509CertificateEntry>();
                string friendlyName = null;
                foreach (X509CertificateStructure certificateStructure in certificateStructures)
                {
                    X509Certificate certificate = new X509Certificate(certificateStructure);
                    X509CertificateEntry certificateEntry = new X509CertificateEntry(certificate);
                    certificateEntries.Add(certificateEntry);
                    if (friendlyName == null)
                    {
                        friendlyName = certificate.SubjectDN.ToString();
                    }
                }

                if (string.IsNullOrEmpty(friendlyName))
                {
                    return null;
                }

                AsymmetricKeyParameter privateKey = LoadBcPrivateKeyResource(keyResource, password);
                AsymmetricKeyEntry keyEntry = new AsymmetricKeyEntry(privateKey);
                store.SetKeyEntry(friendlyName, keyEntry, certificateEntries.ToArray());
                using (MemoryStream stream = new MemoryStream())
                {
                    store.Save(stream, null, new SecureRandom());
                    return stream.ToArray();
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static AsymmetricKeyParameter LoadPkcs12Key(string certResource, string password, out X509CertificateEntry publicCert)
        {
            publicCert = null;
            try
            {
                using (FileStream fs = new FileStream(certResource, FileMode.Open, FileAccess.Read))
                {
                    Pkcs12Store store = new Pkcs12StoreBuilder().Build();
                    char[] passwordChars = password?.ToCharArray();
                    store.Load(fs, passwordChars);

                    string keyAlisas = null;
                    foreach (string alias in store.Aliases)
                    {
                        if (store.IsKeyEntry(alias))
                        {
                            keyAlisas = alias;
                            break;
                        }
                    }

                    if (keyAlisas == null)
                    {
                        return null;
                    }

                    AsymmetricKeyEntry keyEntry = store.GetKey(keyAlisas);
                    if (keyEntry == null)
                    {
                        return null;
                    }

                    X509CertificateEntry[] chain = store.GetCertificateChain(keyAlisas);
                    if (chain == null || chain.Length != 1)
                    {
                        return null;
                    }

                    publicCert = chain[0];
                    if (!publicCert.Certificate.IsValid(DateTime.UtcNow.AddHours(12.0)))
                    {
                        return null;
                    }

                    return keyEntry.Key;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static bool GenerateEcKeyPair(string privateKeyFile, string publicKeyFile, string password = null)
        {
            try
            {
                SecureRandom secureRandom = new SecureRandom();
                IAsymmetricCipherKeyPairGenerator kpg = GeneratorUtilities.GetKeyPairGenerator("EC");
                kpg.Init(new ECKeyGenerationParameters(SecObjectIdentifiers.SecP384r1, secureRandom));
                AsymmetricCipherKeyPair kp = kpg.GenerateKeyPair();

                Pkcs12Store store = new Pkcs12StoreBuilder().Build();

                List<X509CertificateEntry> certificateEntries = new List<X509CertificateEntry>();
                X509V3CertificateGenerator certGen = new X509V3CertificateGenerator();
                X509Name dnName = new X509Name("CN=EdiabasLib");
                certGen.SetSerialNumber(BigInteger.One);
                certGen.SetIssuerDN(dnName);
                certGen.SetNotBefore(DateTime.UtcNow.AddMinutes(-5.0));
                certGen.SetNotAfter(DateTime.UtcNow.AddDays(7.0));
                certGen.SetSubjectDN(dnName);
                certGen.SetPublicKey(kp.Public);

                ISignatureFactory signatureFactory = new Asn1SignatureFactory("SHA1withECDSA", kp.Private, secureRandom);
                X509Certificate certificate = certGen.Generate(signatureFactory);
                X509CertificateEntry certificateEntry = new X509CertificateEntry(certificate);
                certificateEntries.Add(certificateEntry);

                AsymmetricKeyEntry keyEntry = new AsymmetricKeyEntry(kp.Private);
                store.SetKeyEntry("EdiabasLib", keyEntry, certificateEntries.ToArray());
                using (FileStream stream = new FileStream(privateKeyFile, FileMode.Create, FileAccess.Write))
                {
                    store.Save(stream, password?.ToCharArray(), secureRandom);
                }

                using (Org.BouncyCastle.OpenSsl.PemWriter pemWriter = new Org.BouncyCastle.OpenSsl.PemWriter(new StreamWriter(publicKeyFile)))
                {
                    pemWriter.WriteObject(kp.Public);
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
