using Org.BouncyCastle.Tls;
using Org.BouncyCastle.Tls.Crypto.Impl.BC;
using System.IO;
using System;
using System.Collections.Generic;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Tls.Crypto;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.Encoders;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.X509;

namespace EdiabasLib
{
    public class EdBcTlsClient : DefaultTlsClient
    {
        private static readonly int[] TlsCipherSuites = new int[]
        {
            /*
             * TLS 1.3
             */
            CipherSuite.TLS_AES_128_GCM_SHA256,
            CipherSuite.TLS_CHACHA20_POLY1305_SHA256,

            /*
             * pre-TLS 1.3
             */
            CipherSuite.TLS_ECDHE_ECDSA_WITH_AES_128_GCM_SHA256,
            CipherSuite.TLS_ECDHE_ECDSA_WITH_CHACHA20_POLY1305_SHA256,
            CipherSuite.TLS_ECDHE_ECDSA_WITH_AES_128_CBC_SHA256,
            CipherSuite.TLS_ECDHE_ECDSA_WITH_AES_128_CBC_SHA,
            CipherSuite.TLS_ECDHE_RSA_WITH_AES_128_GCM_SHA256,
            CipherSuite.TLS_ECDHE_RSA_WITH_CHACHA20_POLY1305_SHA256,
            CipherSuite.TLS_ECDHE_RSA_WITH_AES_128_CBC_SHA256,
            CipherSuite.TLS_ECDHE_RSA_WITH_AES_128_CBC_SHA,
            CipherSuite.TLS_DHE_RSA_WITH_AES_128_GCM_SHA256,
            CipherSuite.TLS_DHE_RSA_WITH_CHACHA20_POLY1305_SHA256,
            CipherSuite.TLS_DHE_RSA_WITH_AES_128_CBC_SHA256,
            CipherSuite.TLS_DHE_RSA_WITH_AES_128_CBC_SHA,
            CipherSuite.TLS_RSA_WITH_AES_128_GCM_SHA256,
            CipherSuite.TLS_RSA_WITH_AES_128_CBC_SHA256,
            CipherSuite.TLS_RSA_WITH_AES_128_CBC_SHA,
        };


        private readonly EdiabasNet m_ediabasNet;
        private readonly string m_publicCert;
        private readonly string m_privateCert;
        private readonly string m_caFile;
        private readonly string[] m_certResources;
        private readonly List<X509Name> m_certificateAuthorities;

        public EdBcTlsClient(EdiabasNet ediabasNet, string publicCert, string privateCert, List<string> trustedCaList = null) : base(new BcTlsCrypto())
        {
            m_ediabasNet = ediabasNet;
            m_publicCert = publicCert;
            m_privateCert = privateCert;

            if (!File.Exists(m_publicCert))
            {
                throw new FileNotFoundException("Public cert file not found: {0}", m_publicCert);
            }

            if (!File.Exists(m_privateCert))
            {
                throw new FileNotFoundException("Private cert file not found: {0}", m_privateCert);
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
                throw new FileNotFoundException("Public cert file not valid", m_publicCert);
            }

            X509Name publicIssuer = publicCerts[0].IssuerDN;
            X509Name publicSubject = publicCerts[0].SubjectDN;

            m_certificateAuthorities = new List<X509Name>();
            if (trustedCaList != null)
            {
                foreach (string caFile in trustedCaList)
                {
                    if (!File.Exists(caFile))
                    {
                        throw new FileNotFoundException("Trusted CA file not found: {0}", caFile);
                    }

                    X509CertificateStructure caResource = EdBcTlsUtilities.LoadBcCertificateResource(caFile);
                    if (caResource != null && caResource.Subject != null)
                    {
                        m_certificateAuthorities.Add(caResource.Subject);

                        if (publicIssuer.Equivalent(caResource.Subject))
                        {
                            m_caFile = caFile;
                        }
                    }
                }
            }

            if (m_caFile == null)
            {
                throw new FileNotFoundException("No valid CA found for", m_publicCert);
            }

            m_certResources = new string[] { m_publicCert, m_caFile };
        }

        public override IDictionary<int, byte[]> GetClientExtensions()
        {
            if (m_context.SecurityParameters.ClientRandom == null)
            {
                throw new TlsFatalAlert(AlertDescription.internal_error);
            }

            IDictionary<int, byte[]> clientExtensions = base.GetClientExtensions();
            return clientExtensions;
        }

        public override void NotifyAlertRaised(short alertLevel, short alertDescription, string message, Exception cause)
        {
            m_ediabasNet?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "TLS client alert: {0} {1}", AlertLevel.GetText(alertLevel), AlertDescription.GetText(alertDescription));
            if (message != null)
            {
                m_ediabasNet?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Alert message: {0}", message);
            }
            if (cause != null)
            {
                m_ediabasNet?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Alert cause: {0}", message);
            }
        }

        public override void NotifyAlertReceived(short alertLevel, short alertDescription)
        {
            m_ediabasNet?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "TLS client received alert: {0} {1}", AlertLevel.GetText(alertLevel), AlertDescription.GetText(alertDescription));
        }

        public override void NotifyHandshakeComplete()
        {
            base.NotifyHandshakeComplete();

            if (!TlsUtilities.IsTlsV13(m_context))
            {
                byte[] tlsServerEndPoint = m_context.ExportChannelBinding(ChannelBinding.tls_server_end_point);
                byte[] tlsUnique = m_context.ExportChannelBinding(ChannelBinding.tls_unique);

                m_ediabasNet?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "tls-server-end-point: {0}", ToHexString(tlsServerEndPoint));
                m_ediabasNet?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "tls-unique: {0}", ToHexString(tlsUnique));
            }
        }

        public override void NotifyServerVersion(ProtocolVersion serverVersion)
        {
            base.NotifyServerVersion(serverVersion);

            m_ediabasNet?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "TLS client negotiated : {0}", serverVersion);
        }

        public override TlsAuthentication GetAuthentication()
        {
            return new EdBcTlsAuthentication(this, m_context);
        }

        public override void ProcessServerExtensions(IDictionary<int, byte[]> serverExtensions)
        {
            if (m_context.SecurityParameters.ServerRandom == null)
                throw new TlsFatalAlert(AlertDescription.internal_error);

            base.ProcessServerExtensions(serverExtensions);
        }

        protected override int[] GetSupportedCipherSuites()
        {
            return TlsUtilities.GetSupportedCipherSuites(Crypto, TlsCipherSuites);
        }

        protected virtual string ToHexString(byte[] data)
        {
            return data == null ? "(null)" : Hex.ToHexString(data);
        }

        private class EdBcTlsAuthentication : TlsAuthentication
        {
            private readonly EdBcTlsClient m_outer;
            private readonly TlsContext m_context;
            private EdiabasNet m_ediabasNet = null;

            public EdBcTlsAuthentication(EdBcTlsClient outer, TlsContext context)
            {
                m_outer = outer;
                m_context = context;
                m_ediabasNet = outer.m_ediabasNet;
            }

            public virtual void NotifyServerCertificate(TlsServerCertificate serverCertificate)
            {
                TlsCertificate[] chain = serverCertificate.Certificate.GetCertificateList();

                m_ediabasNet?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "TLS client server chain length: {0}", chain.Length);
                for (int i = 0; i < chain.Length; ++i)
                {
                    X509CertificateStructure entry = X509CertificateStructure.GetInstance(chain[i].GetEncoded());
                    m_ediabasNet?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "fingerprint:SHA-256: {0} ({1})", EdBcTlsUtilities.Fingerprint(entry), entry.Subject);
                }

                bool isEmpty = serverCertificate == null || serverCertificate.Certificate == null
                                                         || serverCertificate.Certificate.IsEmpty;

                if (isEmpty)
                    throw new TlsFatalAlert(AlertDescription.bad_certificate);

#if false
                string[] trustedCertResources = new string[]
                {
                    // trusted server certs
                };

                TlsCertificate[] certPath = EdBcTlsUtilities.GetTrustedCertPath(m_context.Crypto, chain[0], trustedCertResources, m_outer.m_caFile);

                if (null == certPath)
                    throw new TlsFatalAlert(AlertDescription.bad_certificate);

                TlsUtilities.CheckPeerSigAlgs(m_context, certPath);
#else
                TlsUtilities.CheckPeerSigAlgs(m_context, chain);
#endif
            }

            public TlsCredentials GetClientCredentials(CertificateRequest certificateRequest)
            {
                bool isTlsV13 = TlsUtilities.IsTlsV13(m_context);

                if (!isTlsV13)
                {
                    short[] certificateTypes = certificateRequest.CertificateTypes;
                    if (certificateTypes == null || !Arrays.Contains(certificateTypes, ClientCertificateType.rsa_sign))
                        return null;
                }

                IList<SignatureAndHashAlgorithm> supportedSigAlgs = certificateRequest.SupportedSignatureAlgorithms;

                TlsCredentialedSigner signerCredentials = EdBcTlsUtilities.LoadSignerCredentials(m_context,
                    supportedSigAlgs, SignatureAlgorithm.rsa, m_outer.m_certResources, m_outer.m_privateCert);
                if (signerCredentials == null && supportedSigAlgs != null)
                {
                    SignatureAndHashAlgorithm pss = SignatureAndHashAlgorithm.rsa_pss_rsae_sha256;
                    if (TlsUtilities.ContainsSignatureAlgorithm(supportedSigAlgs, pss))
                    {
                        signerCredentials = EdBcTlsUtilities.LoadSignerCredentials(m_context, m_outer.m_certResources, m_outer.m_privateCert, pss);
                    }
                }

                return signerCredentials;
            }
        }
    }
}
