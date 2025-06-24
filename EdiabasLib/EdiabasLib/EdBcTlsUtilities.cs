﻿using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.Sec;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Operators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Signers;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Pkix;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Tls;
using Org.BouncyCastle.Tls.Crypto;
using Org.BouncyCastle.Tls.Crypto.Impl.BC;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.Encoders;
using Org.BouncyCastle.Utilities.IO.Pem;
using Org.BouncyCastle.X509;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace EdiabasLib
{
    public static class EdBcTlsUtilities
    {
        public const string S29ProofOfOwnershipPrefix = "S29UNIPOO";

        private class CertPathChecker
            : PkixCertPathChecker
        {
            private static int count;

            public override void Init(bool forward)
            {
            }

            public override bool IsForwardCheckingSupported()
            {
                return true;
            }

            public override ISet<string> GetSupportedExtensions()
            {
                return null;
            }

            public override void Check(X509Certificate cert, ISet<string> unresolvedCritExts)
            {
                count++;
            }

            public int GetCount()
            {
                return count;
            }
        }

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
            List<PemObject> pemObjects = LoadPemResources(resource);
            foreach (PemObject pem in pemObjects)
            {
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

                if (pem.Type.Equals("EC PARAMETERS"))
                {
                    continue;
                }
                break;
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

        public static short? GetSupportedSignatureAlgorithms(X509CertificateStructure certificateStructure, out SignatureAndHashAlgorithm[] supportedSigAndHashAlgs)
        {
            supportedSigAndHashAlgs = null;

            AlgorithmIdentifier algorithmIdentifier = certificateStructure.SubjectPublicKeyInfo?.Algorithm;
            if (algorithmIdentifier == null)
            {
                return null;
            }

            DerObjectIdentifier algorithm = algorithmIdentifier.Algorithm;
            if (algorithm == null)
            {
                return null;
            }

            // Select signature algorithm compatible to Microsoft TLS Server implementation
            short? supportedSignatureAlgorithm = null;
            if (algorithm.Equals(X9ObjectIdentifiers.IdECPublicKey))
            {
                supportedSignatureAlgorithm = SignatureAlgorithm.ecdsa;

                Asn1Encodable parameters = algorithmIdentifier.Parameters;
                if (parameters != null)
                {
                    if (parameters.Equals(SecObjectIdentifiers.SecP256r1))
                    {
                        supportedSigAndHashAlgs = new[] { SignatureScheme.GetSignatureAndHashAlgorithm(SignatureScheme.ecdsa_secp256r1_sha256) };
                    }
                    else if (parameters.Equals(SecObjectIdentifiers.SecP384r1))
                    {
                        supportedSigAndHashAlgs = new[] { SignatureScheme.GetSignatureAndHashAlgorithm(SignatureScheme.ecdsa_secp384r1_sha384) };
                    }
                    else if (parameters.Equals(SecObjectIdentifiers.SecP521r1))
                    {
                        supportedSigAndHashAlgs = new[] { SignatureScheme.GetSignatureAndHashAlgorithm(SignatureScheme.ecdsa_secp521r1_sha512) };
                    }
                }
            }

            if (algorithm.Equals(PkcsObjectIdentifiers.RsaEncryption))
            {
                supportedSignatureAlgorithm = SignatureAlgorithm.rsa;

                DerObjectIdentifier sigAlg = certificateStructure.SignatureAlgorithm?.Algorithm;
                if (sigAlg != null)
                {
                    if (sigAlg.Equals(PkcsObjectIdentifiers.Sha256WithRsaEncryption))
                    {
                        supportedSigAndHashAlgs = new[] { SignatureAndHashAlgorithm.rsa_pss_rsae_sha256 };
                    }
                    else if (sigAlg.Equals(PkcsObjectIdentifiers.Sha384WithRsaEncryption))
                    {
                        supportedSigAndHashAlgs = new[] { SignatureAndHashAlgorithm.rsa_pss_rsae_sha384 };
                    }
                    else if (sigAlg.Equals(PkcsObjectIdentifiers.Sha512WithRsaEncryption))
                    {
                        supportedSigAndHashAlgs = new[] { SignatureAndHashAlgorithm.rsa_pss_rsae_sha512 };
                    }
                }
            }

            return supportedSignatureAlgorithm;
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
            if (signatureAndHashAlgorithms == null)
            {
                return null;
            }

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
            if (crypto is BcTlsCrypto bcTlsCrypto)
            {
                AsymmetricKeyParameter privateKey = LoadBcPrivateKeyResource(keyResource);

                return new BcDefaultTlsCredentialedSigner(cryptoParams, bcTlsCrypto, privateKey, certificate, signatureAndHashAlgorithm);
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
                X509CertificateStructure tlsCertificateStructure = BcTlsCertificate.Convert(bcTlsCrypto, tlsCertificate)?.X509CertificateStructure;
                X509Name issuer = tlsCertificateStructure?.Issuer;
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

        public static bool CheckCertificateChainCa(X509CertificateStructure[] chain, X509Name[] trustedIssuers)
        {
            if (chain.Length < 1)
            {
                return false;
            }

            for (int i = chain.Length - 1; i >= 0; i--)
            {
                X509CertificateStructure certificate = chain[i];
                X509Name issuer = certificate?.Issuer;
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

        public static List<X509CertificateEntry> GetCertificateEntries(List<X509CertificateStructure> certificateStructures)
        {
            if (certificateStructures == null || certificateStructures.Count == 0)
            {
                return null;
            }

            List<X509CertificateEntry> certificateEntries = new List<X509CertificateEntry>();
            foreach (X509CertificateStructure certificateStructure in certificateStructures)
            {
                X509Certificate certificate = new X509Certificate(certificateStructure);
                X509CertificateEntry certificateEntry = new X509CertificateEntry(certificate);
                certificateEntries.Add(certificateEntry);
            }

            return certificateEntries;
        }

        public static byte[] CreatePkcs12Data(string certResource, string keyResource, string password = null)
        {
            try
            {
                Pkcs12Store store = new Pkcs12StoreBuilder().Build();
                List<X509CertificateEntry> certificateEntries = GetCertificateEntries(LoadBcCertificateResources(certResource));
                if (certificateEntries == null || certificateEntries.Count < 1)
                {
                    return null;
                }

                string friendlyName = certificateEntries[0].Certificate?.SubjectDN?.ToString();
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

        public static AsymmetricKeyParameter LoadPkcs12Key(string pkcs12File, string password, out X509CertificateEntry[] publicChain)
        {
            publicChain = null;
            try
            {
                using (FileStream fs = new FileStream(pkcs12File, FileMode.Open, FileAccess.Read))
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
                    if (chain == null || chain.Length < 1)
                    {
                        return null;
                    }

                    foreach (X509CertificateEntry certEntry in chain)
                    {
                        if (!certEntry.Certificate.IsValid(DateTime.UtcNow.AddHours(12.0)))
                        {
                            return null;
                        }
                    }

                    publicChain = chain;
                    return keyEntry.Key;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static bool ExtractPkcs12Key(string pkcs12File, string password, string privateFile, string publicFile)
        {
            try
            {
                AsymmetricKeyParameter asymmetricKeyPar = LoadPkcs12Key(pkcs12File, password, out X509CertificateEntry[] publicChain);
                if (asymmetricKeyPar == null || publicChain == null || publicChain.Length < 1)
                {
                    return false;
                }

                using (Org.BouncyCastle.OpenSsl.PemWriter pemWriter = new Org.BouncyCastle.OpenSsl.PemWriter(new StreamWriter(privateFile)))
                {
                    pemWriter.WriteObject(asymmetricKeyPar);
                }

                using (Org.BouncyCastle.OpenSsl.PemWriter pemWriter = new Org.BouncyCastle.OpenSsl.PemWriter(new StreamWriter(publicFile)))
                {
                    foreach (X509CertificateEntry certificateEntry in publicChain)
                    {
                        pemWriter.WriteObject(certificateEntry.Certificate);
                    }
                }

                return true;
            }
            catch (Exception )
            {
                return false;
            }
        }

        public static string ConvertPublicKeyToPEM(AsymmetricKeyParameter publicKey)
        {
            try
            {
                if (publicKey == null)
                {
                    return null;
                }

                using (StringWriter stringWriter = new StringWriter())
                {
                    using (Org.BouncyCastle.OpenSsl.PemWriter pemWriter = new Org.BouncyCastle.OpenSsl.PemWriter(stringWriter))
                    {
                        pemWriter.WriteObject(publicKey);
                        pemWriter.Writer.Flush();
                        string text = stringWriter.ToString().Replace("-----BEGIN PUBLIC KEY-----", "").Replace("-----END PUBLIC KEY-----", "")
                            .Replace("\r", "")
                            .Replace("\n", "")
                            .Trim();
                        return text;
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static AsymmetricKeyParameter ConvertPemToPublicKey(string pemContent)
        {
            try
            {
                if (string.IsNullOrEmpty(pemContent))
                {
                    return null;
                }

                StringBuilder sb = new StringBuilder();
                sb.AppendLine("-----BEGIN PUBLIC KEY-----");
                sb.AppendLine(pemContent.Trim());
                sb.AppendLine("-----END PUBLIC KEY-----");
                using (StringReader stringReader = new StringReader(sb.ToString()))
                {
                    using (Org.BouncyCastle.OpenSsl.PemReader pemReader = new Org.BouncyCastle.OpenSsl.PemReader(stringReader))
                    {
                        object pemObject = pemReader.ReadObject();
                        if (pemObject is AsymmetricKeyParameter keyParameter)
                        {
                            return keyParameter;
                        }
                    }
                }

                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static X509Certificate CreateCertificateFromBase64(string base64Certificate)
        {
            byte[] certificateBytes = Convert.FromBase64String(base64Certificate);
            return new X509CertificateParser().ReadCertificate(certificateBytes);
        }

        public static string SignData(string message, ECPrivateKeyParameters privateKey, string algorithm = "SHA512withECDSA")
        {
            ISigner signer = SignerUtilities.GetSigner(algorithm);
            signer.Init(true, privateKey);
            byte[] bytes = Encoding.UTF8.GetBytes(message);
            signer.BlockUpdate(bytes, 0, bytes.Length);
            return Convert.ToBase64String(signer.GenerateSignature());
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
                byte[] prefixBytes = Encoding.ASCII.GetBytes(S29ProofOfOwnershipPrefix);
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
                byte[] prefixBytes = Encoding.ASCII.GetBytes(S29ProofOfOwnershipPrefix);
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

        public static bool GenerateEcKeyPair(string privateKeyFile, string publicKeyFile, DerObjectIdentifier paramSet, string password = null)
        {
            try
            {
                SecureRandom secureRandom = new SecureRandom();
                IAsymmetricCipherKeyPairGenerator kpg = GeneratorUtilities.GetKeyPairGenerator("EC");
                kpg.Init(new ECKeyGenerationParameters(paramSet, secureRandom));
                AsymmetricCipherKeyPair kp = kpg.GenerateKeyPair();

                Pkcs12Store store = new Pkcs12StoreBuilder().Build();

                List<X509CertificateEntry> certificateEntries = new List<X509CertificateEntry>();
                X509V3CertificateGenerator certGen = new X509V3CertificateGenerator();
                X509Name dnName = new X509Name("CN=SelfSigned");
                certGen.SetSerialNumber(BigInteger.One);
                certGen.SetIssuerDN(dnName);
                certGen.SetNotBefore(DateTime.UtcNow.AddMinutes(-5.0));
                certGen.SetNotAfter(DateTime.UtcNow.AddYears(1));
                certGen.SetSubjectDN(dnName);
                certGen.SetPublicKey(kp.Public);

                ISignatureFactory signatureFactory = new Asn1SignatureFactory("SHA512withECDSA", kp.Private, secureRandom);
                X509Certificate certificate = certGen.Generate(signatureFactory);
                X509CertificateEntry certificateEntry = new X509CertificateEntry(certificate);
                certificateEntries.Add(certificateEntry);

                AsymmetricKeyEntry keyEntry = new AsymmetricKeyEntry(kp.Private);
                store.SetKeyEntry("alias", keyEntry, certificateEntries.ToArray());
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

        public static bool ValidateCertChain(List<X509Certificate> certChain)
        {
            if (certChain == null || certChain.Count < 2)
            {
                return false;
            }

            X509Certificate rootCert = certChain[certChain.Count - 1];
            PkixCertPath cp = new PkixCertPath(certChain);
            PkixCertPathValidator cpv = new PkixCertPathValidator();
            HashSet<TrustAnchor> trust = new HashSet<TrustAnchor>();
            PkixParameters param = new PkixParameters(trust);
            CertPathChecker checker = new CertPathChecker();

            param.AddCertPathChecker(checker);
            PkixCertPathValidatorResult result = cpv.Validate(cp, param);

            if (!result.TrustAnchor.TrustedCert.Equals(rootCert))
            {
                return false;
            }

            return true;
        }
    }
}
