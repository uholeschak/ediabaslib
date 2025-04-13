using Org.BouncyCastle.Tls;
using Org.BouncyCastle.Tls.Crypto.Impl.BC;
using System.IO;
using System;
using static Org.BouncyCastle.Math.EC.ECCurve;
using System.Collections.Generic;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Tls.Crypto;
using Org.BouncyCastle.Utilities;

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


        private EdiabasNet m_ediabasNet = null;
        private string m_privateCert = null;
        private string m_publicCert = null;
        private string m_CaFile = null;

        public EdBcTlsClient(EdiabasNet ediabasNet, string privateCert, string publicCert, string caFile) : base(new BcTlsCrypto())
        {
            m_ediabasNet = ediabasNet;
            m_privateCert = privateCert;
            m_publicCert = publicCert;
            m_CaFile = caFile;
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

        public override TlsAuthentication GetAuthentication()
        {
            return new EdBcTlsAuthentication(this, m_context);
        }

        private class EdBcTlsAuthentication : TlsAuthentication
        {
            private readonly EdBcTlsClient m_outer;
            private readonly TlsContext m_context;

            internal EdBcTlsAuthentication(EdBcTlsClient outer, TlsContext context)
            {
                this.m_outer = outer;
                this.m_context = context;
            }

            public virtual void NotifyServerCertificate(TlsServerCertificate serverCertificate)
            {
                TlsCertificate[] chain = serverCertificate.Certificate.GetCertificateList();

                if (TlsTestConfig.Debug)
                {
                    Console.WriteLine("TLS client received server certificate chain of length " + chain.Length);
                    for (int i = 0; i < chain.Length; ++i)
                    {
                        X509CertificateStructure entry = X509CertificateStructure.GetInstance(chain[i].GetEncoded());
                        // TODO Create fingerprint based on certificate signature algorithm digest
                        Console.WriteLine("    fingerprint:SHA-256 " + TlsTestUtilities.Fingerprint(entry) + " ("
                                          + entry.Subject + ")");
                    }
                }

                bool isEmpty = serverCertificate == null || serverCertificate.Certificate == null
                                                         || serverCertificate.Certificate.IsEmpty;

                if (isEmpty)
                    throw new TlsFatalAlert(AlertDescription.bad_certificate);

                string[] trustedCertResources = new string[]
                {
                    "x509-server-dsa.pem", "x509-server-ecdh.pem",
                    "x509-server-ecdsa.pem", "x509-server-ed25519.pem", "x509-server-ed448.pem",
                    "x509-server-rsa_pss_256.pem", "x509-server-rsa_pss_384.pem", "x509-server-rsa_pss_512.pem",
                    "x509-server-rsa-enc.pem", "x509-server-rsa-sign.pem"
                };

                TlsCertificate[] certPath = TlsTestUtilities.GetTrustedCertPath(m_context.Crypto, chain[0],
                    trustedCertResources);

                if (null == certPath)
                    throw new TlsFatalAlert(AlertDescription.bad_certificate);

                if (m_outer.m_config.clientCheckSigAlgOfServerCerts)
                {
                    TlsUtilities.CheckPeerSigAlgs(m_context, certPath);
                }
            }

            public virtual TlsCredentials GetClientCredentials(CertificateRequest certificateRequest)
            {
                bool isTlsV13 = TlsUtilities.IsTlsV13(m_context);

                if (!isTlsV13)
                {
                    short[] certificateTypes = certificateRequest.CertificateTypes;
                    if (certificateTypes == null || !Arrays.Contains(certificateTypes, ClientCertificateType.rsa_sign))
                        return null;
                }

                var supportedSigAlgs = certificateRequest.SupportedSignatureAlgorithms;
                if (supportedSigAlgs != null && config.clientAuthSigAlg != null)
                {
                    supportedSigAlgs = TlsUtilities.VectorOfOne(config.clientAuthSigAlg);
                }

                // TODO[tls13] Check also supportedSigAlgsCert against the chain signature(s)

                TlsCredentialedSigner signerCredentials = TlsTestUtilities.LoadSignerCredentials(m_context,
                    supportedSigAlgs, SignatureAlgorithm.rsa, "x509-client-rsa.pem", "x509-client-key-rsa.pem");
                if (signerCredentials == null && supportedSigAlgs != null)
                {
                    SignatureAndHashAlgorithm pss = SignatureAndHashAlgorithm.rsa_pss_rsae_sha256;
                    if (TlsUtilities.ContainsSignatureAlgorithm(supportedSigAlgs, pss))
                    {
                        signerCredentials = TlsTestUtilities.LoadSignerCredentials(m_context,
                            new string[] { "x509-client-rsa.pem" }, "x509-client-key-rsa.pem", pss);
                    }
                }

                return signerCredentials;
            }
        }
    }
}
