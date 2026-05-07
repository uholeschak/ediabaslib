using Org.BouncyCastle.Tls;
using PsdzRpcServer.Shared;
using StreamJsonRpc;
using System;
using System.IO;
using System.IO.Pipes;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;

namespace PsdzRpcClient
{
    public class PsdzRpcClient : IDisposable
    {
        private readonly TextWriter _output;
        private readonly X509Certificate2 _caCert;
        private readonly X509Certificate2 _clientCert;
        private Stream _stream;
        private TcpClient _tcpClient;
        private JsonRpc _jsonRpc;
        private CancellationTokenSource _keepAliveCts;

        public IPsdzRpcService RpcService { get; private set; }
        public event EventHandler<bool> ClientConnected;
        public PsdzRpcCallbackHandler CallbackHandler { get; } = new PsdzRpcCallbackHandler();

        public PsdzRpcClient(TextWriter output = null, string caCertPath = null, string clientPfxPath = null)
        {
            _output = output;

            Assembly assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
            if (!string.IsNullOrEmpty(caCertPath))
            {
                _caCert = PsdzRpcCertificateHelper.LoadCertificate(caCertPath)
                          ?? PsdzRpcCertificateHelper.LoadEmbeddedCertificate(assembly, Path.GetFileName(caCertPath));
                if (_caCert == null)
                {
                    throw new InvalidOperationException("Failed to load CA certificate.");
                }
            }

            if (!string.IsNullOrEmpty(clientPfxPath))
            {
                _clientCert = PsdzRpcCertificateHelper.LoadCertificate(clientPfxPath)
                              ?? PsdzRpcCertificateHelper.LoadEmbeddedCertificate(assembly, Path.GetFileName(clientPfxPath));
                if (_clientCert == null)
                {
                    throw new InvalidOperationException("Failed to load client certificate.");
                }
            }
        }

        /// <summary>Verbindet via Named Pipe (bestehender Server).</summary>
        public async Task ConnectPipeAsync(SynchronizationContext synchronizationContext, CancellationToken ct)
        {
            NamedPipeClientStream pipeClient = new NamedPipeClientStream(
                ".",
                PsdzRpcServiceConstants.PipeName,
                PipeDirection.InOut,
                PipeOptions.Asynchronous);

            _output?.WriteLine("Connecting via pipe...");
            await pipeClient.ConnectAsync(ct).ConfigureAwait(false);
            _stream = pipeClient;
            _output?.WriteLine("Connected!");

            StartJsonRpc(synchronizationContext);
        }

        /// <summary>Verbindet via TCP (kein automatischer Serverstart erforderlich).</summary>
        public async Task<bool> ConnectTcpAsync(string host, int port, SynchronizationContext synchronizationContext, CancellationToken ct)
        {
            try
            {
                string hostName = string.IsNullOrEmpty(host) ? PsdzRpcServiceConstants.Localhost : host;
                _tcpClient = new TcpClient { NoDelay = true };
                _tcpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
#if NET
                _tcpClient.Client.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveTime,     10); // First probe
                _tcpClient.Client.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveInterval, 10); // Time between probes
                _tcpClient.Client.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveRetryCount, 3);
#endif

                _output?.WriteLine($"Connecting via TCP to {hostName}:{port}...");
#if NET
                await _tcpClient.ConnectAsync(hostName, port, ct).ConfigureAwait(false);
#else
                Task connectTask = _tcpClient.ConnectAsync(hostName, port);
                Task cancelTask = Task.Delay(Timeout.Infinite, ct);
                if (await Task.WhenAny(connectTask, cancelTask).ConfigureAwait(false) == cancelTask)
                {
                    ct.ThrowIfCancellationRequested();
                }
                await connectTask.ConfigureAwait(false);
#endif
                if (_caCert != null)
                {
                    SslStream sslStream = new SslStream(
                        _tcpClient.GetStream(),
                        leaveInnerStreamOpen: false,
                        userCertificateValidationCallback: (sender, cert, chain, errors) =>
                            ValidateServerCertificate(cert));

#if NET
                    SslClientAuthenticationOptions options = new SslClientAuthenticationOptions
                    {
                        TargetHost = _caCert != null ? PsdzRpcServiceConstants.ServerCnName : hostName,
                        EnabledSslProtocols = PsdzRpcServiceConstants.DefaultSslProtocols,
                        CertificateRevocationCheckMode = X509RevocationMode.NoCheck
                    };

                    if (_clientCert != null)
                        options.ClientCertificates = new X509CertificateCollection { _clientCert };

                    await sslStream.AuthenticateAsClientAsync(options).ConfigureAwait(false);
#else
                    X509CertificateCollection clientCerts = _clientCert != null
                        ? new X509CertificateCollection { _clientCert }
                        : null;

                    await sslStream.AuthenticateAsClientAsync(
                        targetHost: hostName,
                        clientCertificates: clientCerts,
                        enabledSslProtocols: PsdzRpcServiceConstants.DefaultSslProtocols,
                        checkCertificateRevocation: false).ConfigureAwait(false);
#endif

                    _stream = sslStream;
                    _output?.WriteLine("Connected (SSL/TLS)");
                }
                else
                {
                    _stream = _tcpClient.GetStream();
                    _output?.WriteLine("Connected (plain)");
                }

                StartJsonRpc(synchronizationContext);
                return true;
            }
            catch (Exception ex)
            {
                _output?.WriteLine($"TCP connect failed: {ex.Message}");
                _tcpClient?.Dispose();
                _tcpClient = null;
                return false;
            }
        }

            private bool ValidateServerCertificate(X509Certificate cert)
        {
            if (_caCert == null) return true;
            if (cert == null)    return false;

            X509Certificate2 cert2 = cert as X509Certificate2;
            if (cert2 == null) return false;

            // CN des Server-Zertifikats prüfen
            string cn = cert2.GetNameInfo(X509NameType.SimpleName, forIssuer: false);
            if (string.Compare(cn, PsdzRpcServiceConstants.ServerCnName, StringComparison.OrdinalIgnoreCase) != 0)
            {
                _output?.WriteLine($"Server certificate CN mismatch: {cn}");
                return false;
            }

            // CA-Kette prüfen
            X509Chain chain = new X509Chain();
            chain.ChainPolicy.ExtraStore.Add(_caCert);
            chain.ChainPolicy.RevocationMode    = X509RevocationMode.NoCheck;
            chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllowUnknownCertificateAuthority;

            bool valid = chain.Build(cert2);
            return valid && chain.ChainElements[chain.ChainElements.Count - 1]
                .Certificate.Thumbprint == _caCert.Thumbprint;
        }

        private void StartJsonRpc(SynchronizationContext synchronizationContext)
        {
            _keepAliveCts?.Cancel();
            _keepAliveCts?.Dispose();
            _keepAliveCts = new CancellationTokenSource();

            _jsonRpc = new JsonRpc(_stream);
            _jsonRpc.AddLocalRpcTarget(CallbackHandler);
            _jsonRpc.Disconnected += OnJsonRpcDisconnected;

            if (synchronizationContext != null)
                _jsonRpc.SynchronizationContext = synchronizationContext;

            RpcService = _jsonRpc.Attach<IPsdzRpcService>();
            _jsonRpc.StartListening();
            ClientConnected?.Invoke(this, true);

            _ = KeepAliveLoopAsync(_keepAliveCts.Token);
        }

        private void OnJsonRpcDisconnected(object sender, JsonRpcDisconnectedEventArgs e)
        {
            if (e.Reason != DisconnectedReason.LocallyDisposed)
            {
                _output?.WriteLine($"RPC disconnected: {e.Reason} – {e.Description}");
            }

            _keepAliveCts?.Cancel();
            ClientConnected?.Invoke(this, false);
        }

        // Periodischer Ping-Task
        private async Task KeepAliveLoopAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(10), ct).ConfigureAwait(false);
                    await RpcService.PingAsync(ct).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    _output?.WriteLine("Keep-alive loop cancelled.");
                    break;
                }
                catch (Exception ex)
                {
                    _output?.WriteLine($"Keep-alive ping failed: {ex.Message}");
                    ClientConnected?.Invoke(this, false);
                    break;
                }
            }
        }

        public void Dispose()
        {
            _keepAliveCts?.Cancel(); // Loop abbrechen
            _keepAliveCts?.Dispose();
            _keepAliveCts = null;

            _jsonRpc?.Dispose();
            _jsonRpc = null;

            _stream?.Dispose();
            _stream = null;

            _tcpClient?.Dispose();
            _tcpClient = null;

            _caCert?.Dispose();
            _clientCert?.Dispose();

            RpcService?.Dispose();
            RpcService = null;
        }
    }
}
