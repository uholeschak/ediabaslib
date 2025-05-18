﻿using Org.BouncyCastle.Asn1.X509;
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
    private readonly string[] m_certResources;
    private readonly IList<X509Name> m_certificateAuthorities = null;
    private IList<X509Name> m_clientTrustedIssuers = null;

    public int HandshakeTimeout { get; set; } = 0;

    public BcTlsServer(string certBaseFile, string certPassword) : base(new BcTlsCrypto(new SecureRandom()))
    {
        string certDir = Path.GetDirectoryName(certBaseFile);
        if (string.IsNullOrEmpty(certDir))
        {
            throw new ArgumentException("Certificate base file must contain a directory", nameof(certBaseFile));
        }

        m_publicCert = Path.ChangeExtension(certBaseFile, ".pem");
        m_privateCert = Path.ChangeExtension(certBaseFile, ".key");
        m_certPassword = certPassword;

        m_certificateAuthorities = new List<X509Name>();
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
                X509Name trustedIssuer = EdBcTlsUtilities.LoadBcCertificateResource(trustedFile)?.Subject;
                if (trustedIssuer != null)
                {
                    m_certificateAuthorities.Add(trustedIssuer);
                }
            }
        }

        if (!File.Exists(m_publicCert) || !File.Exists(m_privateCert))
        {
            throw new FileNotFoundException("Certificate files not found", certBaseFile);
        }

        if (m_certificateAuthorities.Count == 0)
        {
            throw new FileNotFoundException("No trusted CA files found", certBaseFile);
        }

        AsymmetricKeyParameter privateKeyResource = EdBcTlsUtilities.LoadBcPrivateKeyResource(m_privateCert, m_certPassword);
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

        if (publicCerts.Count < 2)
        {
            throw new FileNotFoundException("Public certificate file does not contain CA certificate", m_publicCert);
        }

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

            return new CertificateRequest(certificateRequestContext, serverSigAlgs, null, m_certificateAuthorities);
        }
        else
        {
            short[] certificateTypes = new []{ ClientCertificateType.rsa_sign, ClientCertificateType.dss_sign, ClientCertificateType.ecdsa_sign };

            return new CertificateRequest(certificateTypes, serverSigAlgs, m_certificateAuthorities);
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
        for (int i = 0; i < chain.Length; ++i)
        {
            X509CertificateStructure entry = X509CertificateStructure.GetInstance(chain[i].GetEncoded());
            // TODO Create fingerprint based on certificate signature algorithm digest
            Debug.WriteLine("    fingerprint:SHA-256 " + EdBcTlsUtilities.Fingerprint(entry) + " (" + entry.Subject + ")");
        }

        if (!EdBcTlsUtilities.CheckCertificateChainCa(Crypto, chain, m_certificateAuthorities.ToArray()))
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
        return LoadSignerCredentials(SignatureAlgorithm.rsa, RsaSignatureAndHashAlgorithms);
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
            TlsCredentialedSigner signerCredentials = EdBcTlsUtilities.LoadSignerCredentials(m_context, clientSignatureAlgorithms, m_certResources, m_privateCert, RsaSignatureAndHashAlgorithms);
            if (signerCredentials != null)
            {
                return signerCredentials;
            }
        }

        return EdBcTlsUtilities.LoadSignerCredentials(m_context, clientSignatureAlgorithms, signatureAlgorithm, m_certResources, m_privateCert);
    }
}
