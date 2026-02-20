using EdiabasLib;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Tls;
using Org.BouncyCastle.Tls.Crypto;
using Org.BouncyCastle.Tls.Crypto.Impl.BC;
using Org.BouncyCastle.Utilities.Encoders;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace CarSimulator;

public class BcTlsServer : DefaultTlsServer
{
    // These alogorithms are compatibe with Microsoft TLS 1.3 implementation
    private static readonly SignatureAndHashAlgorithm[] RsaSignatureAndHashAlgorithms =
    {
        SignatureAndHashAlgorithm.rsa_pss_rsae_sha256,
        SignatureAndHashAlgorithm.rsa_pss_rsae_sha384,
        SignatureAndHashAlgorithm.rsa_pss_rsae_sha512,
    };

    private readonly string m_privateCert = null;
    private readonly string m_publicCert = null;
    private readonly string m_certPassword = null;
    private AsymmetricKeyParameter m_privateKeyResource;
    private readonly string[] m_certResources;
    private readonly short m_supportedSignatureAlgorithm;
    private readonly SignatureAndHashAlgorithm[] m_supportedSigAndHashAlgs;
    private readonly List<Org.BouncyCastle.X509.X509Certificate> m_certificateAuthorities = null;
    private IList<X509Name> m_TrustedCaNames = null;
    private IList<X509Name> m_clientTrustedIssuers = null;

    public int HandshakeTimeout { get; set; } = 0;

    public BcTlsServer(string certBaseFile, string certPassword, List<X509CertificateStructure> certificateAuthorities) : base(new BcTlsCrypto(new SecureRandom()))
    {
        string certDir = Path.GetDirectoryName(certBaseFile);
        if (string.IsNullOrEmpty(certDir))
        {
            throw new ArgumentException("Certificate base file must contain a directory", nameof(certBaseFile));
        }

        m_publicCert = Path.ChangeExtension(certBaseFile, ".pem");
        m_privateCert = Path.ChangeExtension(certBaseFile, ".key");
        m_certPassword = certPassword;

        if (!File.Exists(m_publicCert) || !File.Exists(m_privateCert))
        {
            throw new FileNotFoundException("Certificate files not found", certBaseFile);
        }

        m_certificateAuthorities = new List<Org.BouncyCastle.X509.X509Certificate>();
        foreach (X509CertificateStructure certificateStructure in certificateAuthorities)
        {
            m_certificateAuthorities.Add(new Org.BouncyCastle.X509.X509Certificate(certificateStructure));
        }
        if (m_certificateAuthorities.Count == 0)
        {
            throw new FileNotFoundException("No certificate authorities found", certBaseFile);
        }

        m_TrustedCaNames = EdBcTlsUtilities.GetSubjectList(certificateAuthorities);
        if (m_TrustedCaNames.Count == 0)
        {
            throw new FileNotFoundException("No trusted CA names found", certBaseFile);
        }

        m_privateKeyResource = EdBcTlsUtilities.LoadBcPrivateKeyResource(m_privateCert, m_certPassword);
        if (m_privateKeyResource == null)
        {
            throw new FileNotFoundException("Private key file not valid", m_privateCert);
        }

        List<X509CertificateStructure> publicCerts = EdBcTlsUtilities.LoadBcCertificateResources(m_publicCert);
        if (publicCerts == null || publicCerts.Count == 0)
        {
            throw new FileNotFoundException("Public certificate file not valid", m_publicCert);
        }

        if (publicCerts.Count < 2)
        {
            throw new FileNotFoundException("Public certificate file does not contain CA certificate", m_publicCert);
        }

        short? supportedSignatureAlgorithm = EdBcTlsUtilities.GetSupportedSignatureAlgorithms(publicCerts[0], out m_supportedSigAndHashAlgs);
        if (supportedSignatureAlgorithm == null)
        {
            throw new FileNotFoundException("Public certificate does not contain supported signature algorithm", m_publicCert);
        }

        m_supportedSignatureAlgorithm = supportedSignatureAlgorithm.Value;
        m_certResources = new string[] { m_publicCert };
    }

    public override int GetHandshakeTimeoutMillis()
    {
        return HandshakeTimeout;
    }

    public override TlsCredentials GetCredentials()
    {
        if (m_context.ServerVersion == null)
        {
            throw new TlsFatalAlert(AlertDescription.protocol_version);
        }

        if (TlsUtilities.IsTlsV13(m_context))
        {
            return LoadSignerCredentials(m_supportedSignatureAlgorithm, m_supportedSigAndHashAlgs);
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

    public override void NotifyConnectionClosed()
    {
        m_clientTrustedIssuers = null;
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

        if (TlsUtilities.IsTlsV13(m_context))
        {
            byte[] certificateRequestContext = TlsUtilities.EmptyBytes;

            return new CertificateRequest(certificateRequestContext, serverSigAlgs, null, m_TrustedCaNames);
        }
        else
        {
            short[] certificateTypes = new []{ ClientCertificateType.rsa_sign, ClientCertificateType.dss_sign, ClientCertificateType.ecdsa_sign };

            return new CertificateRequest(certificateTypes, serverSigAlgs, m_TrustedCaNames);
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
            Debug.WriteLine("TLS server received no client certificate");
#if false
            short alertDescription = TlsUtilities.IsTlsV13(m_context)
                ? AlertDescription.certificate_required
                : AlertDescription.handshake_failure;

            throw new TlsFatalAlert(alertDescription);
#else
            return;
#endif
        }

        TlsCertificate[] chain = clientCertificate.GetCertificateList();

        Debug.WriteLine("TLS server received client certificate chain of length " + chain.Length);
        List<Org.BouncyCastle.X509.X509Certificate> certChain = new List<Org.BouncyCastle.X509.X509Certificate>();
        for (int i = 0; i < chain.Length; i++)
        {
            X509CertificateStructure entry = X509CertificateStructure.GetInstance(chain[i].GetEncoded());
            certChain.Add(new Org.BouncyCastle.X509.X509Certificate(entry));
            // TODO Create fingerprint based on certificate signature algorithm digest
            Debug.WriteLine("    fingerprint:SHA-256 " + EdBcTlsUtilities.Fingerprint(entry) + " (" + entry.Subject + ")");
        }

        if (certChain.Count > 0)
        {
            if (!EdBcTlsUtilities.ValidateCertChain(certChain,m_certificateAuthorities))
            {
                throw new TlsFatalAlert(AlertDescription.bad_certificate);
            }
        }

        TlsUtilities.CheckPeerSigAlgs(m_context, chain);
    }

    public override void ProcessClientExtensions(IDictionary<int, byte[]> clientExtensions)
    {
        if (m_context.SecurityParameters.ClientRandom == null)
            throw new TlsFatalAlert(AlertDescription.internal_error);

        base.ProcessClientExtensions(clientExtensions);
        m_clientTrustedIssuers = TlsExtensionsUtilities.GetCertificateAuthoritiesExtension(clientExtensions);

        if (m_clientTrustedIssuers != null && m_clientTrustedIssuers.Count > 0)
        {
            List<TlsCertificate> publicCertChain = EdBcTlsUtilities.LoadCertificateResources(Crypto, m_publicCert);
            if (publicCertChain == null || publicCertChain.Count == 0)
            {
                throw new TlsFatalAlert(AlertDescription.internal_error);
            }

            if (!EdBcTlsUtilities.CheckCertificateChainCa(Crypto, publicCertChain.ToArray(), m_clientTrustedIssuers.ToArray()))
            {
                throw new TlsFatalAlert(AlertDescription.bad_certificate);
            }
        }
    }

    public override int[] GetSupportedGroups()
    {
        // prefer secp256r1
        return new int[]
        {
            NamedGroup.secp256r1, NamedGroup.secp384r1,
            NamedGroup.x25519, NamedGroup.x448,
            NamedGroup.ffdhe2048, NamedGroup.ffdhe3072, NamedGroup.ffdhe4096
        };
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

    protected override TlsCredentialedDecryptor GetRsaEncryptionCredentials()
    {
        return EdBcTlsUtilities.LoadEncryptionCredentials(m_context, m_certResources, m_privateCert);
    }

    public virtual string ToHexString(byte[] data)
    {
        return data == null ? "(null)" : Hex.ToHexString(data);
    }

    private TlsCredentialedSigner LoadSignerCredentials(short signatureAlgorithm, SignatureAndHashAlgorithm[] signatureAndHashAlgorithms = null)
    {
        IList<SignatureAndHashAlgorithm> clientSignatureAlgorithms = m_context.SecurityParameters?.ClientSigAlgs;
        if (signatureAndHashAlgorithms != null)
        {
            TlsCredentialedSigner signerCredentials = EdBcTlsUtilities.LoadSignerCredentials(m_context, clientSignatureAlgorithms, m_certResources, m_privateCert, signatureAndHashAlgorithms);
            if (signerCredentials != null)
            {
                return signerCredentials;
            }
        }

        return EdBcTlsUtilities.LoadSignerCredentials(m_context, clientSignatureAlgorithms, signatureAlgorithm, m_certResources, m_privateCert);
    }
}
