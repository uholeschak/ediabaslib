using Org.BouncyCastle.Tls;
using Org.BouncyCastle.Tls.Crypto.Impl.BC;
using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Tls.Crypto;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.Encoders;

namespace EdiabasLib
{
    public class EdBcTlsClient : DefaultTlsClient
    {
        public class CertInfo
        {
            public CertInfo(string privateCert, string publicCert, bool tempFile = false)
            {
                PrivateCert = privateCert;
                PublicCert = publicCert;
                TempFile = tempFile;
            }

            public string PrivateCert { get; }
            public string PublicCert { get; }
            public bool TempFile { get; }
        }

        private static readonly int[] ClientCipherSuites = new int[]
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

            // TODO[api] Remove RSA key exchange cipher suites from default list
            CipherSuite.TLS_RSA_WITH_AES_256_GCM_SHA384,
            CipherSuite.TLS_RSA_WITH_AES_128_GCM_SHA256,
            CipherSuite.TLS_RSA_WITH_AES_256_CBC_SHA256,
            CipherSuite.TLS_RSA_WITH_AES_128_CBC_SHA256,
            CipherSuite.TLS_RSA_WITH_AES_256_CBC_SHA,
            CipherSuite.TLS_RSA_WITH_AES_128_CBC_SHA,
        };

        private readonly EdiabasNet m_ediabasNet;
        private readonly List<CertInfo> m_privatePublicCertList;
        private readonly List<Org.BouncyCastle.X509.X509Certificate> m_certificateAuthorities;
        private readonly List<X509Name> m_TrustedCaNames = null;

        public int HandshakeTimeout { get; set; } = 0;

        public EdBcTlsClient(EdiabasNet ediabasNet, List<CertInfo> certInfoList, List<X509CertificateStructure> certificateAuthorities) : base(new BcTlsCrypto())
        {
            m_ediabasNet = ediabasNet;
            m_privatePublicCertList = certInfoList;

            m_certificateAuthorities = new List<Org.BouncyCastle.X509.X509Certificate>();
            foreach (X509CertificateStructure certificateStructure in certificateAuthorities)
            {
                m_certificateAuthorities.Add(new Org.BouncyCastle.X509.X509Certificate(certificateStructure));
            }

            m_TrustedCaNames = EdBcTlsUtilities.GetSubjectList(certificateAuthorities);
            if (m_TrustedCaNames.Count == 0)
            {
                throw new TlsFatalAlert(AlertDescription.unknown_ca);
            }
        }

        public override int GetHandshakeTimeoutMillis()
        {
            return HandshakeTimeout;
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

        protected override IList<X509Name> GetCertificateAuthorities()
        {
            return m_TrustedCaNames;
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

            m_ediabasNet?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "TLS client negotiated: {0}", serverVersion);
        }

        public override void NotifySelectedCipherSuite(int selectedCipherSuite)
        {
            base.NotifySelectedCipherSuite(selectedCipherSuite);

            m_ediabasNet?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "TLS client selected cipher suite: {0}", selectedCipherSuite);
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
            return TlsUtilities.GetSupportedCipherSuites(Crypto, ClientCipherSuites);
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
            private IList<X509Name> m_certificateAuthorities = null;

            public EdBcTlsAuthentication(EdBcTlsClient outer, TlsContext context)
            {
                m_outer = outer;
                m_context = context;
                m_ediabasNet = outer.m_ediabasNet;
            }

            public void NotifyServerCertificate(TlsServerCertificate serverCertificate)
            {
                bool isEmpty = serverCertificate?.Certificate == null || serverCertificate.Certificate.IsEmpty;

                if (isEmpty)
                {
                    throw new TlsFatalAlert(AlertDescription.bad_certificate);
                }

                TlsCertificate[] chain = serverCertificate.Certificate.GetCertificateList();

                m_certificateAuthorities = new List<X509Name>();
                BcTlsCrypto bcTlsCrypto = m_outer.Crypto as BcTlsCrypto;
                if (bcTlsCrypto != null)
                {
                    for (int i = chain.Length - 1; i >= 0; i--)
                    {
                        TlsCertificate tlsCertificate = chain[i];
                        X509Name issuer = BcTlsCertificate.Convert(bcTlsCrypto, tlsCertificate)?.X509CertificateStructure?.Issuer;
                        if (issuer != null)
                        {
                            if (!m_certificateAuthorities.Contains(issuer))
                            {
                                m_certificateAuthorities.Add(issuer);
                            }
                        }
                    }
                }

                m_ediabasNet?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "TLS client server chain length: {0}", chain.Length);
                List<Org.BouncyCastle.X509.X509Certificate> certChain = new List<Org.BouncyCastle.X509.X509Certificate>();
                for (int i = 0; i < chain.Length; ++i)
                {
                    X509CertificateStructure entry = X509CertificateStructure.GetInstance(chain[i].GetEncoded());
                    certChain.Add(new Org.BouncyCastle.X509.X509Certificate(entry));
                    m_ediabasNet?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "fingerprint:SHA-256: {0} ({1})", EdBcTlsUtilities.Fingerprint(entry), entry.Subject);
                }

                if (!EdBcTlsUtilities.ValidateCertChain(certChain, m_outer.m_certificateAuthorities))
                {
                    throw new TlsFatalAlert(AlertDescription.unknown_ca);
                }

                TlsUtilities.CheckPeerSigAlgs(m_context, chain);
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

                if (m_outer.m_privatePublicCertList == null || m_outer.m_privatePublicCertList.Count == 0)
                {
                    return null;
                }

                IList<X509Name> certificateAuthorities = m_certificateAuthorities;
                if (certificateAuthorities == null || certificateAuthorities.Count == 0)
                {
                    throw new TlsFatalAlert(AlertDescription.unknown_ca);
                }

                BcTlsCrypto bcTlsCrypto = m_outer.Crypto as BcTlsCrypto;
                if (bcTlsCrypto == null)
                {
                    throw new TlsFatalAlert(AlertDescription.internal_error);
                }

                string selectedPrivateCert = null;
                string selectedPublicCert = null;
                short selectedSigAlg = SignatureAlgorithm.anonymous;
                SignatureAndHashAlgorithm[] selectedSigAndHashAlgs = null;

                foreach (CertInfo certInfo in m_outer.m_privatePublicCertList)
                {
                    if (string.IsNullOrEmpty(certInfo.PrivateCert) || string.IsNullOrEmpty(certInfo.PublicCert))
                    {
                        continue;
                    }

                    short? supportedSignatureAlgorithm = null;
                    SignatureAndHashAlgorithm[] supportedSigAndHashAlgs = null;
                    List<X509CertificateStructure> publicCertificates = EdBcTlsUtilities.LoadBcCertificateResources(certInfo.PublicCert);
                    foreach (X509CertificateStructure publicCertificate in publicCertificates)
                    {
                        supportedSignatureAlgorithm = EdBcTlsUtilities.GetSupportedSignatureAlgorithms(publicCertificate, out supportedSigAndHashAlgs);
                        if (supportedSignatureAlgorithm != null)
                        {
                            break;
                        }
                    }

                    if (supportedSignatureAlgorithm == null)
                    {
                        continue;
                    }

                    bool acceptCert = false;
                    if (EdBcTlsUtilities.CheckCertificateChainCa(publicCertificates.ToArray(), m_certificateAuthorities.ToArray()))
                    {
                        acceptCert = true;
                    }
                    else
                    {
                        if (publicCertificates.Count == 1)
                        {
                            acceptCert = true;
                        }
                    }

                    if (acceptCert)
                    {
                        selectedPrivateCert = certInfo.PrivateCert;
                        selectedPublicCert = certInfo.PublicCert;
                        selectedSigAlg = supportedSignatureAlgorithm.Value;
                        selectedSigAndHashAlgs = supportedSigAndHashAlgs;
                        break;
                    }
                }

                if (selectedPrivateCert == null || selectedPublicCert == null)
                {
                    throw new TlsFatalAlert(AlertDescription.bad_certificate);
                }

                IList<SignatureAndHashAlgorithm> supportedSigAlgs = certificateRequest.SupportedSignatureAlgorithms;
                if (supportedSigAlgs != null)
                {
                    TlsCredentialedSigner signerCredentials = EdBcTlsUtilities.LoadSignerCredentials(m_context, supportedSigAlgs, new[] { selectedPublicCert }, selectedPrivateCert, selectedSigAndHashAlgs);
                    if (signerCredentials != null)
                    {
                        return signerCredentials;
                    }
                }

                return EdBcTlsUtilities.LoadSignerCredentials(m_context, supportedSigAlgs, selectedSigAlg, new[] { selectedPublicCert }, selectedPrivateCert);
            }
        }
    }
}
