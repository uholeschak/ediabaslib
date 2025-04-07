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

namespace CarSimulator;

public class BcTlsServer : DefaultTlsServer
{
    private static readonly int[] TlsV3CipherSuites = new int[]
    {
                /*
                 * TLS 1.3
                 */
                CipherSuite.TLS_AES_256_GCM_SHA384,
                CipherSuite.TLS_AES_128_GCM_SHA256,
                CipherSuite.TLS_CHACHA20_POLY1305_SHA256,
    };

    private string m_privateCert = null;
    private string m_publicCert = null;
    private string m_CaFile = null;
    private string m_certPassword = null;

    public BcTlsServer(string certBaseFile, string certPassword) : base(new BcTlsCrypto(new SecureRandom()))
    {
        m_publicCert = Path.ChangeExtension(certBaseFile, ".crt");
        m_privateCert = Path.ChangeExtension(certBaseFile, ".key");
        m_CaFile = Path.Combine(Path.GetDirectoryName(certBaseFile) ?? string.Empty, "rootCA.crt");
        m_certPassword = certPassword;
        if (!File.Exists(m_publicCert) || !File.Exists(m_privateCert))
        {
            throw new FileNotFoundException("Certificate files not found", certBaseFile);
        }
        if (!File.Exists(m_CaFile))
        {
            throw new FileNotFoundException("CA file not found", m_CaFile);
        }
    }

    public override TlsCredentials GetCredentials()
    {
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

    public override Org.BouncyCastle.Tls.CertificateRequest GetCertificateRequest()
    {
        IList<SignatureAndHashAlgorithm> serverSigAlgs = null;
        if (TlsUtilities.IsSignatureAlgorithmsExtensionAllowed(m_context.ServerVersion))
        {
            serverSigAlgs = TlsUtilities.GetDefaultSupportedSignatureAlgorithms(m_context);
        }

        var certificateAuthorities = new List<X509Name>();
        certificateAuthorities.Add(LoadBcCertificateResource(m_CaFile).Subject);

        if (TlsUtilities.IsTlsV13(m_context))
        {
            // TODO[tls13] Support for non-empty request context
            byte[] certificateRequestContext = TlsUtilities.EmptyBytes;

            // TODO[tls13] Add TlsTestConfig.serverCertReqSigAlgsCert
            IList<SignatureAndHashAlgorithm> serverSigAlgsCert = null;

            return new Org.BouncyCastle.Tls.CertificateRequest(certificateRequestContext, serverSigAlgs, serverSigAlgsCert,
                certificateAuthorities);
        }
        else
        {
            short[] certificateTypes = new short[]{ ClientCertificateType.rsa_sign,
                    ClientCertificateType.dss_sign, ClientCertificateType.ecdsa_sign };

            return new Org.BouncyCastle.Tls.CertificateRequest(certificateTypes, serverSigAlgs, certificateAuthorities);
        }
    }

    public override void NotifyClientCertificate(Certificate clientCertificate)
    {
        bool isEmpty = (clientCertificate == null || clientCertificate.IsEmpty);

#if false
                if (isEmpty)
                {
                    short alertDescription = TlsUtilities.IsTlsV13(m_context)
                        ? AlertDescription.certificate_required
                        : AlertDescription.handshake_failure;

                    throw new TlsFatalAlert(alertDescription);
                }
#endif

        TlsCertificate[] chain = clientCertificate.GetCertificateList();

        Debug.WriteLine("TLS server received client certificate chain of length " + chain.Length);
        for (int i = 0; i < chain.Length; ++i)
        {
            X509CertificateStructure entry = X509CertificateStructure.GetInstance(chain[0].GetEncoded());
            // TODO Create fingerprint based on certificate signature algorithm digest
            Debug.WriteLine("    fingerprint:SHA-256 " + Fingerprint(entry) + " (" + entry.Subject + ")");
        }
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
        return LoadEncryptionCredentials(m_context,
            new string[] { "x509-server-rsa-enc.pem", "x509-ca-rsa.pem" }, "x509-server-key-rsa-enc.pem");
    }

    protected override TlsCredentialedSigner GetRsaSignerCredentials()
    {
        return LoadSignerCredentials(SignatureAlgorithm.rsa);
    }

    protected override int[] GetSupportedCipherSuites()
    {
        return TlsUtilities.GetSupportedCipherSuites(Crypto, TlsV3CipherSuites);
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

    private static X509CertificateStructure LoadBcCertificateResource(string resource)
    {
        PemObject pem = LoadPemResource(resource);
        if (pem.Type.EndsWith("CERTIFICATE"))
        {
            return X509CertificateStructure.GetInstance(pem.Content);
        }
        throw new ArgumentException("doesn't specify a valid certificate", "resource");
    }

    private static PemObject LoadPemResource(string resource)
    {
        using (var p = new PemReader(new StreamReader(resource)))
        {
            return p.ReadPemObject();
        }
    }

    private TlsCredentialedSigner LoadSignerCredentials(short signatureAlgorithm)
    {
#if false
                return LoadSignerCredentialsServer(m_context, GetSupportedSignatureAlgorithms(),
                    signatureAlgorithm);
#else
        return null;
#endif
    }

    private static TlsCredentialedDecryptor LoadEncryptionCredentials(TlsContext context, string[] certResources,
        string keyResource)
    {
#if false
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
#else
        throw new NotSupportedException();
#endif
    }
}
