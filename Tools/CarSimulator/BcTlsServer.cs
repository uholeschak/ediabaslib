using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Tls.Crypto.Impl.BC;
using Org.BouncyCastle.Tls.Crypto;
using Org.BouncyCastle.Tls;
using Org.BouncyCastle.Utilities.Encoders;
using Org.BouncyCastle.Utilities.IO.Pem;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System;
using System.Linq;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.Sec;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Utilities;
using System.Reflection;

namespace CarSimulator;

public class BcTlsServer : DefaultTlsServer
{
    public const string RootCaFileName = "rootCA.crt";

    private static readonly int[] TlsCipherSuites = new int[]
    {
        /*
         * TLS 1.3
         */
        CipherSuite.TLS_AES_256_GCM_SHA384,
        CipherSuite.TLS_AES_128_GCM_SHA256,
        CipherSuite.TLS_CHACHA20_POLY1305_SHA256,
        /*
         * pre-TLS 1.3
         */
        CipherSuite.TLS_ECDHE_RSA_WITH_AES_256_GCM_SHA384,
        CipherSuite.TLS_ECDHE_RSA_WITH_AES_128_GCM_SHA256,
        CipherSuite.TLS_ECDHE_RSA_WITH_CHACHA20_POLY1305_SHA256,
        CipherSuite.TLS_ECDHE_RSA_WITH_AES_256_CBC_SHA384,
        CipherSuite.TLS_ECDHE_RSA_WITH_AES_128_CBC_SHA256,
        CipherSuite.TLS_ECDHE_RSA_WITH_AES_256_CBC_SHA,
        CipherSuite.TLS_ECDHE_RSA_WITH_AES_128_CBC_SHA,
        CipherSuite.TLS_DHE_RSA_WITH_AES_256_GCM_SHA384,
        CipherSuite.TLS_DHE_RSA_WITH_AES_128_GCM_SHA256,
        CipherSuite.TLS_DHE_RSA_WITH_CHACHA20_POLY1305_SHA256,
        CipherSuite.TLS_DHE_RSA_WITH_AES_256_CBC_SHA256,
        CipherSuite.TLS_DHE_RSA_WITH_AES_128_CBC_SHA256,
        CipherSuite.TLS_DHE_RSA_WITH_AES_256_CBC_SHA,
        CipherSuite.TLS_DHE_RSA_WITH_AES_128_CBC_SHA,
        CipherSuite.TLS_RSA_WITH_AES_256_GCM_SHA384,
        CipherSuite.TLS_RSA_WITH_AES_128_GCM_SHA256,
        CipherSuite.TLS_RSA_WITH_AES_256_CBC_SHA256,
        CipherSuite.TLS_RSA_WITH_AES_128_CBC_SHA256,
        CipherSuite.TLS_RSA_WITH_AES_256_CBC_SHA,
        CipherSuite.TLS_RSA_WITH_AES_128_CBC_SHA,
    };

    private string m_privateCert = null;
    private string m_publicCert = null;
    private string m_CaFile = null;
    private string[] m_trustedCertResources;

    public BcTlsServer(string certBaseFile, string certPassword) : base(new BcTlsCrypto(new SecureRandom()))
    {
        if (!string.IsNullOrEmpty(certPassword))
        {
            throw new NotSupportedException("Password protected certificates not supported");
        }

        string certDir = Path.GetDirectoryName(certBaseFile);
        if (string.IsNullOrEmpty(certDir))
        {
            throw new ArgumentException("Certificate base file must contain a directory", nameof(certBaseFile));
        }

        m_publicCert = Path.ChangeExtension(certBaseFile, ".crt");
        m_privateCert = Path.ChangeExtension(certBaseFile, ".key");
        m_CaFile = Path.Combine(certDir, RootCaFileName);

        string[] trustedFiles = Directory.GetFiles(certDir, "*.crt", SearchOption.TopDirectoryOnly);
        List<string> trustedCertList = new List<string>();
        foreach (string trustedFile in trustedFiles)
        {
            string certFileName = Path.GetFileName(trustedFile);
            if (string.IsNullOrEmpty(certFileName))
            {
                continue;
            }

            if (string.Compare(certFileName, RootCaFileName, StringComparison.OrdinalIgnoreCase) == 0)
            {
                continue;
            }

            if (certFileName.EndsWith("_full.crt", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            trustedCertList.Add(trustedFile);
        }

        m_trustedCertResources = trustedCertList.ToArray();

        if (!File.Exists(m_publicCert) || !File.Exists(m_privateCert))
        {
            throw new FileNotFoundException("Certificate files not found", certBaseFile);
        }

        if (!File.Exists(m_CaFile))
        {
            throw new FileNotFoundException("CA file not found", m_CaFile);
        }
    }

    public bool Test1()
    {
        SecurityParameters securityParameters = m_context.SecurityParameters;
        FieldInfo fieldVersion = securityParameters.GetType().GetField("m_negotiatedVersion", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (fieldVersion == null)
        {
            Debug.WriteLine("Failed to get negotiated version field");
            return false;
        }
        fieldVersion.SetValue(securityParameters, ProtocolVersion.TLSv13);

        FieldInfo fieldAlgCert = securityParameters.GetType().GetField("m_serverSigAlgsCert", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (fieldAlgCert == null)
        {
            Debug.WriteLine("Failed to get serverSigAlgsCert field");
            return false;
        }
        fieldAlgCert.SetValue(securityParameters, new SignatureAndHashAlgorithm[] { new SignatureAndHashAlgorithm(HashAlgorithm.sha256, SignatureAlgorithm.rsa) });

        TlsCredentialedDecryptor credentialedDecryptor = GetRsaEncryptionCredentials();
        if (credentialedDecryptor == null)
        {
            Debug.WriteLine("Failed to load RSA encryption credentials");
            return false;
        }

        TlsCredentialedSigner credentialedSigner = GetRsaSignerCredentials();
        if (credentialedSigner == null)
        {
            Debug.WriteLine("Failed to load RSA signer credentials");
            return false;
        }

        CertificateRequest certificateRequest = GetCertificateRequest();
        if (certificateRequest == null)
        {
            Debug.WriteLine("Failed to get certificate request");
            return false;
        }

        string certDir = Path.GetDirectoryName(m_CaFile);
        string clientCert = Path.Combine(certDir, "client.crt");
        Certificate certificate = LoadCertificateChain(m_context, new[] { clientCert, m_CaFile });
        NotifyClientCertificate(certificate);
        return true;
    }

    public override TlsCredentials GetCredentials()
    {
        if (m_context.ServerVersion == null)
        {
            throw new TlsFatalAlert(AlertDescription.protocol_version);
        }

        if (TlsUtilities.IsTlsV13(m_context))
        {
            return GetRsaSignerCredentials();
        }

        return base.GetCredentials();
    }

    public override void NotifyAlertRaised(short alertLevel, short alertDescription, string message, Exception cause)
    {
        Debug.WriteLine("TLS server raised alert: " + AlertLevel.GetText(alertLevel) + ", " + AlertDescription.GetText(alertDescription));
        if (message != null)
        {
            Debug.WriteLine("> " + message);
        }
        if (cause != null)
        {
            Debug.WriteLine(cause);
        }
    }

    public override void NotifyAlertReceived(short alertLevel, short alertDescription)
    {
        Debug.WriteLine("TLS server received alert: " + AlertLevel.GetText(alertLevel) + ", " + AlertDescription.GetText(alertDescription));
    }

    public override void NotifyHandshakeComplete()
    {
        base.NotifyHandshakeComplete();

        byte[] tlsServerEndPoint = m_context.ExportChannelBinding(ChannelBinding.tls_server_end_point);
        byte[] tlsUnique = m_context.ExportChannelBinding(ChannelBinding.tls_unique);

        Debug.WriteLine("TLS server reports 'tls-server-end-point' = " + ToHexString(tlsServerEndPoint));
        Debug.WriteLine("TLS server reports 'tls-unique' = " + ToHexString(tlsUnique));
    }

    public override CertificateRequest GetCertificateRequest()
    {
        if (m_context.ServerVersion == null)
        {
            throw new TlsFatalAlert(AlertDescription.protocol_version);
        }

        IList<SignatureAndHashAlgorithm> serverSigAlgs = null;
        if (TlsUtilities.IsSignatureAlgorithmsExtensionAllowed(m_context.ServerVersion))
        {
            serverSigAlgs = TlsUtilities.GetDefaultSupportedSignatureAlgorithms(m_context);
        }

        var certificateAuthorities = new List<X509Name>();
        certificateAuthorities.Add(LoadBcCertificateResource(m_CaFile).Subject);

        if (TlsUtilities.IsTlsV13(m_context))
        {
            byte[] certificateRequestContext = TlsUtilities.EmptyBytes;

            return new CertificateRequest(certificateRequestContext, serverSigAlgs, null, certificateAuthorities);
        }
        else
        {
            short[] certificateTypes = new []{ ClientCertificateType.rsa_sign, ClientCertificateType.dss_sign, ClientCertificateType.ecdsa_sign };

            return new CertificateRequest(certificateTypes, serverSigAlgs, certificateAuthorities);
        }
    }

    public override void NotifyClientCertificate(Certificate clientCertificate)
    {
        if (m_context.ServerVersion == null)
        {
            throw new TlsFatalAlert(AlertDescription.protocol_version);
        }

        bool isEmpty = (clientCertificate == null || clientCertificate.IsEmpty);

        if (isEmpty)
        {
            short alertDescription = TlsUtilities.IsTlsV13(m_context)
                ? AlertDescription.certificate_required
                : AlertDescription.handshake_failure;

            throw new TlsFatalAlert(alertDescription);
        }

        TlsCertificate[] chain = clientCertificate.GetCertificateList();

        Debug.WriteLine("TLS server received client certificate chain of length " + chain.Length);
        for (int i = 0; i < chain.Length; ++i)
        {
            X509CertificateStructure entry = X509CertificateStructure.GetInstance(chain[i].GetEncoded());
            // TODO Create fingerprint based on certificate signature algorithm digest
            Debug.WriteLine("    fingerprint:SHA-256 " + Fingerprint(entry) + " (" + entry.Subject + ")");
        }

        TlsCertificate[] certPath = GetTrustedCertPath(m_context.Crypto, chain[0], m_trustedCertResources);

        if (null == certPath)
            throw new TlsFatalAlert(AlertDescription.bad_certificate);

        TlsUtilities.CheckPeerSigAlgs(m_context, certPath);
    }

    public override void ProcessClientExtensions(IDictionary<int, byte[]> clientExtensions)
    {
        if (m_context.SecurityParameters.ClientRandom == null)
            throw new TlsFatalAlert(AlertDescription.internal_error);

        base.ProcessClientExtensions(clientExtensions);
    }

    public override IDictionary<int, byte[]> GetServerExtensions()
    {
        if (m_context.SecurityParameters.ServerRandom == null)
            throw new TlsFatalAlert(AlertDescription.internal_error);

        return base.GetServerExtensions();
    }

    public override void GetServerExtensionsForConnection(IDictionary<int, byte[]> serverExtensions)
    {
        if (m_context.SecurityParameters.ServerRandom == null)
            throw new TlsFatalAlert(AlertDescription.internal_error);

        base.GetServerExtensionsForConnection(serverExtensions);
    }

    protected virtual IList<SignatureAndHashAlgorithm> GetSupportedSignatureAlgorithms()
    {
        return m_context.SecurityParameters.ClientSigAlgs;
    }

    protected override TlsCredentialedSigner GetDsaSignerCredentials()
    {
        return LoadSignerCredentials(SignatureAlgorithm.dsa);
    }

    protected override TlsCredentialedSigner GetECDsaSignerCredentials()
    {
        // TODO[RFC 8422] Code should choose based on client's supported sig algs?
        return LoadSignerCredentials(SignatureAlgorithm.ecdsa);
        //return LoadSignerCredentials(SignatureAlgorithm.ed25519);
        //return LoadSignerCredentials(SignatureAlgorithm.ed448);
    }

    protected override TlsCredentialedDecryptor GetRsaEncryptionCredentials()
    {
        return LoadEncryptionCredentials(m_context, new [] { m_publicCert, m_CaFile }, m_privateCert);
    }

    protected override TlsCredentialedSigner GetRsaSignerCredentials()
    {
        return LoadSignerCredentials(SignatureAlgorithm.rsa);
    }

    protected override int[] GetSupportedCipherSuites()
    {
        return TlsUtilities.GetSupportedCipherSuites(Crypto, TlsCipherSuites);
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

    public virtual string ToHexString(byte[] data)
    {
        return data == null ? "(null)" : Hex.ToHexString(data);
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

    private TlsCredentialedSigner LoadSignerCredentials(short signatureAlgorithm)
    {
        return LoadSignerCredentials(m_context, GetSupportedSignatureAlgorithms(), signatureAlgorithm, m_publicCert, m_privateCert);
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
        string certResource, string keyResource)
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

        return LoadSignerCredentials(context, new string[] { certResource }, keyResource, signatureAndHashAlgorithm);
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

    private static Certificate LoadCertificateChain(ProtocolVersion protocolVersion, TlsCrypto crypto, string[] resources)
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

    public static bool AreSameCertificate(TlsCertificate a, TlsCertificate b)
    {
        // TODO[tls-ops] Support equals on TlsCertificate?
        return Arrays.AreEqual(a.GetEncoded(), b.GetEncoded());
    }

    private TlsCertificate[] GetTrustedCertPath(TlsCrypto crypto, TlsCertificate cert, string[] resources)
    {
        foreach (string eeCertResource in resources)
        {
            TlsCertificate eeCert = LoadCertificateResource(crypto, eeCertResource);
            if (AreSameCertificate(cert, eeCert))
            {
                TlsCertificate caCert = LoadCertificateResource(crypto, m_CaFile);
                if (null != caCert)
                {
                    return new TlsCertificate[] { eeCert, caCert };
                }
            }
        }
        return null;
    }
}
