using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Tls.Crypto.Impl.BC;
using Org.BouncyCastle.Tls.Crypto;
using Org.BouncyCastle.Tls;
using Org.BouncyCastle.Utilities.Encoders;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System;
using System.Linq;
using Org.BouncyCastle.Crypto;
using System.Reflection;
using EdiabasLib;
using Org.BouncyCastle.X509;

namespace CarSimulator;

public class BcTlsServer : DefaultTlsServer
{
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

    private readonly string m_privateCert = null;
    private readonly string m_publicCert = null;
    private readonly string m_caFile = null;
    private readonly string[] m_certResources;

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

        m_publicCert = Path.ChangeExtension(certBaseFile, ".pem");
        m_privateCert = Path.ChangeExtension(certBaseFile, ".key");

        string[] trustedFiles = Directory.GetFiles(certDir, "*.crt", SearchOption.TopDirectoryOnly);
        foreach (string trustedFile in trustedFiles)
        {
            string certFileName = Path.GetFileName(trustedFile);
            if (string.IsNullOrEmpty(certFileName))
            {
                continue;
            }

            if (certFileName.EndsWith("CA.crt", StringComparison.OrdinalIgnoreCase))
            {
                m_caFile = trustedFile;
                break;
            }
        }

        if (!File.Exists(m_publicCert) || !File.Exists(m_privateCert))
        {
            throw new FileNotFoundException("Certificate files not found", certBaseFile);
        }

        if (!File.Exists(m_caFile))
        {
            throw new FileNotFoundException("CA file not found", m_caFile);
        }

        AsymmetricKeyParameter privateKeyResource = EdBcTlsUtilities.LoadBcPrivateKeyResource(m_privateCert);
        if (privateKeyResource == null)
        {
            throw new FileNotFoundException("Private key file not valid", m_privateCert);
        }

        IList<X509Certificate> publicCerts;
        using (Stream fileStream = new FileStream(m_publicCert, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            publicCerts = new X509CertificateParser().ReadCertificates(fileStream);
        }
        if (publicCerts == null || publicCerts.Count == 0)
        {
            throw new FileNotFoundException("Public certificate file not valid", m_publicCert);
        }

        X509CertificateStructure caResourceResource = EdBcTlsUtilities.LoadBcCertificateResource(m_caFile);
        if (caResourceResource == null)
        {
            throw new FileNotFoundException("CA file not valid", m_caFile);
        }

        if (publicCerts.Count > 1)
        {   // is chained cert
            m_certResources = new string[] { m_publicCert };
        }
        else
        {
            m_certResources = new string[] { m_publicCert, m_caFile };
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

        FieldInfo fieldServerRandom = securityParameters.GetType().GetField("m_serverRandom", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (fieldServerRandom == null)
        {
            Debug.WriteLine("Failed to get serverRandom field");
            return false;
        }
        byte[] serverRandom = m_context.NonceGenerator.GenerateNonce(32);
        fieldServerRandom.SetValue(securityParameters, serverRandom);

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

        IDictionary<int, byte[]> serverExtensions = GetServerExtensions();
        if (serverExtensions == null)
        {
            Debug.WriteLine("Failed to get server extensions");
            return false;
        }

        int[] cipherSuites = GetSupportedCipherSuites();
        if (cipherSuites == null)
        {
            Debug.WriteLine("Failed to get supported cipher suites");
            return false;
        }

        CertificateRequest certificateRequest = GetCertificateRequest();
        if (certificateRequest == null)
        {
            Debug.WriteLine("Failed to get certificate request");
            return false;
        }

        string certDir = Path.GetDirectoryName(m_caFile);
        string clientCert = Path.Combine(certDir, "client.pem");
        Certificate certificate = EdBcTlsUtilities.LoadCertificateChain(m_context, new[] { clientCert });
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

        if (!TlsUtilities.IsTlsV13(m_context))
        {
            byte[] tlsServerEndPoint = m_context.ExportChannelBinding(ChannelBinding.tls_server_end_point);
            byte[] tlsUnique = m_context.ExportChannelBinding(ChannelBinding.tls_unique);

            Debug.WriteLine("TLS server reports 'tls-server-end-point' = " + ToHexString(tlsServerEndPoint));
            Debug.WriteLine("TLS server reports 'tls-unique' = " + ToHexString(tlsUnique));
        }
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
        certificateAuthorities.Add(EdBcTlsUtilities.LoadBcCertificateResource(m_caFile).Subject);

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
            Debug.WriteLine("    fingerprint:SHA-256 " + EdBcTlsUtilities.Fingerprint(entry) + " (" + entry.Subject + ")");
        }

        TlsCertificate caCertificate = EdBcTlsUtilities.LoadCertificateResource(m_context.Crypto, m_caFile);
        if (caCertificate == null)
        {
            throw new TlsFatalAlert(AlertDescription.internal_error);
        }

        if (chain.Length < 2)
        {
            throw new TlsFatalAlert(AlertDescription.bad_certificate);
        }

        if (!EdBcTlsUtilities.AreSameCertificate(caCertificate, chain[^1]))
        {
            throw new TlsFatalAlert(AlertDescription.bad_certificate);
        }

        TlsUtilities.CheckPeerSigAlgs(m_context, chain);
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
        return EdBcTlsUtilities.LoadEncryptionCredentials(m_context, m_certResources, m_privateCert);
    }

    protected override TlsCredentialedSigner GetRsaSignerCredentials()
    {
        return LoadSignerCredentials(SignatureAlgorithm.rsa);
    }

    protected override int[] GetSupportedCipherSuites()
    {
        return TlsUtilities.GetSupportedCipherSuites(Crypto, TlsCipherSuites);
    }

    public virtual string ToHexString(byte[] data)
    {
        return data == null ? "(null)" : Hex.ToHexString(data);
    }

    private TlsCredentialedSigner LoadSignerCredentials(short signatureAlgorithm)
    {
        return EdBcTlsUtilities.LoadSignerCredentials(m_context, GetSupportedSignatureAlgorithms(), signatureAlgorithm, m_certResources, m_privateCert);
    }
}
