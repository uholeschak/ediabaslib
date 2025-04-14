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

        public static AsymmetricKeyParameter LoadBcPrivateKeyResource(string resource)
        {
            PemObject pem = LoadPemResource(resource);
            if (pem.Type.Equals("PRIVATE KEY"))
            {
                return PrivateKeyFactory.CreateKey(pem.Content);
            }
            if (pem.Type.Equals("ENCRYPTED PRIVATE KEY"))
            {
                throw new NotSupportedException("Encrypted PKCS#8 keys not supported");
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
            using (var p = new PemReader(new StreamReader(resource)))
            {
                return p.ReadPemObject();
            }
        }

        public static List<PemObject> LoadPemResources(string resource)
        {
            List <PemObject> pemObjects = new List<PemObject>();
            using (PemReader p = new PemReader(new StreamReader(resource)))
            {
                for (;;)
                {
                    PemObject pemObject = p.ReadPemObject();
                    if (pemObject == null)
                    {
                        break;
                    }

                    pemObjects.Add(pemObject);
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
            else
            {
                throw new NotSupportedException();
            }
        }

        public static Certificate LoadCertificateChain(ProtocolVersion protocolVersion, TlsCrypto crypto, string[] resources)
        {
            if (protocolVersion == null)
            {
                throw new TlsFatalAlert(AlertDescription.protocol_version);
            }

            if (TlsUtilities.IsTlsV13(protocolVersion))
            {
                CertificateEntry[] certificateEntryList = new CertificateEntry[resources.Length];
                for (int i = 0; i < resources.Length; ++i)
                {
                    TlsCertificate certificate = LoadCertificateResource(crypto, resources[i]);
                    certificateEntryList[i] = new CertificateEntry(certificate, null);
                }

                byte[] certificateRequestContext = TlsUtilities.EmptyBytes;
                return new Certificate(certificateRequestContext, certificateEntryList);
            }
            else
            {
                TlsCertificate[] chain = new TlsCertificate[resources.Length];
                for (int i = 0; i < resources.Length; ++i)
                {
                    chain[i] = LoadCertificateResource(crypto, resources[i]);
                }
                return new Certificate(chain);
            }
        }

        public static Certificate LoadCertificateChain(ProtocolVersion protocolVersion, TlsCrypto crypto, string resources)
        {
            if (protocolVersion == null)
            {
                throw new TlsFatalAlert(AlertDescription.protocol_version);
            }

            if (TlsUtilities.IsTlsV13(protocolVersion))
            {
                List<CertificateEntry> certificateEntryList = new List<CertificateEntry>();
                List<TlsCertificate> certificates = LoadCertificateResources(crypto, resources);
                foreach (TlsCertificate certificate in certificates)
                {
                    certificateEntryList.Add(new CertificateEntry(certificate, null));
                }

                byte[] certificateRequestContext = TlsUtilities.EmptyBytes;
                return new Certificate(certificateRequestContext, certificateEntryList.ToArray());
            }
            else
            {
                List<TlsCertificate> certificates = LoadCertificateResources(crypto, resources);
                return new Certificate(certificates.ToArray());
            }
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
    }
}
