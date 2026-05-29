using EdiabasLib;
using PsdzRpcServer.Shared;
using StreamJsonRpc;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Net.Security;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

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
        private DateTime? _pingDateTime;
        private object _pingLock = new object();
        private CancellationTokenSource _keepAliveCts;
        private CancellationTokenSource _clientCts;

        public IPsdzRpcService RpcService { get; private set; }
        public event EventHandler<bool> ClientConnected;
        public event EventHandler<DateTime?> PingUpdated;
        public PsdzRpcCallbackHandler CallbackHandler { get; } = new PsdzRpcCallbackHandler();

        public DateTime? PingDateTime
        {
            get
            {
                lock (_pingLock)
                {
                    return _pingDateTime;
                }
            }
        }

        public PsdzRpcClient(TextWriter output = null, string caCertPath = null, string clientPfxPath = null, Assembly resourceAssembly = null)
        {
            _output = output;

            _clientCts = new CancellationTokenSource();
            CallbackHandler.DisconnectToken = _clientCts.Token;

            Assembly assembly = resourceAssembly ?? Assembly.GetExecutingAssembly();
            if (!string.IsNullOrEmpty(caCertPath))
            {
                _caCert = PsdzRpcCertificateHelper.LoadCertificate(caCertPath)
                          ?? PsdzRpcCertificateHelper.LoadEmbeddedCertificate(assembly, Path.GetFileName(caCertPath));
                if (_caCert == null)
                {
                    _output?.WriteLine($"Failed to load CA certificate: {caCertPath}");
                    throw new InvalidOperationException($"Failed to load CA certificate: {caCertPath}");
                }
            }

            if (!string.IsNullOrEmpty(clientPfxPath))
            {
                _clientCert = PsdzRpcCertificateHelper.LoadCertificate(clientPfxPath)
                              ?? PsdzRpcCertificateHelper.LoadEmbeddedCertificate(assembly, Path.GetFileName(clientPfxPath));
                if (_clientCert == null)
                {
                    _output?.WriteLine($"Failed to load client certificate: {clientPfxPath}");
                    throw new InvalidOperationException($"Failed to load client certificate: {clientPfxPath}");
                }
            }
            else
            {
                if (_caCert != null)
                {
                    _output?.WriteLine("No client certificate provided for CA certificate.");
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
        public async Task<bool> ConnectTcpAsync(string host, int port, SynchronizationContext synchronizationContext, CancellationToken ct, int timeoutSeconds = 10)
        {
            try
            {
                string hostName = string.IsNullOrEmpty(host) ? PsdzRpcServiceConstants.Localhost : host;
#if false
                _tcpClient = new TcpClient(AddressFamily.InterNetworkV6);
                _tcpClient.Client.DualMode = true; // IPv4 + IPv6 über einen Socket
#else
                _tcpClient = new TcpClient(AddressFamily.InterNetwork);
#endif
                _tcpClient.NoDelay = true;
                _tcpClient.SendBufferSize = 65536;
                _tcpClient.ReceiveBufferSize = 65536;

                _tcpClient.Client.SendTimeout = 30000; // 30s
                _tcpClient.Client.ReceiveTimeout = 30000; // 30s
                _tcpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
#if NET
                _tcpClient.Client.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveTime,      60); // Erste Probe nach 60s Inaktivität
                _tcpClient.Client.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveInterval,  15); // Proben alle 15s
                _tcpClient.Client.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveRetryCount,  5); // 5 Versuche
#endif
                using CancellationTokenSource timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
                using CancellationTokenSource linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct, timeoutCts.Token);

                _output?.WriteLine($"Connecting via TCP to {hostName}:{port}...");
#if NET
                await _tcpClient.ConnectAsync(hostName, port, linkedCts.Token).ConfigureAwait(false);
#else
                Task connectTask = _tcpClient.ConnectAsync(hostName, port);
                Task cancelTask = Task.Delay(Timeout.Infinite, linkedCts.Token);
                if (await Task.WhenAny(connectTask, cancelTask).ConfigureAwait(false) == cancelTask)
                {
                    linkedCts.Token.ThrowIfCancellationRequested();
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
            if (_caCert == null)
            {
                return true;
            }

            if (cert == null)
            {
                return false;
            }

            X509Certificate2 cert2 = cert as X509Certificate2;
            if (cert2 == null)
            {
                return false;
            }

            // CN des Server-Zertifikats prüfen
            string cn = cert2.GetNameInfo(X509NameType.SimpleName, forIssuer: false);
            if (string.Compare(cn, PsdzRpcServiceConstants.ServerCnName, StringComparison.OrdinalIgnoreCase) != 0)
            {
                _output?.WriteLine($"Server certificate CN mismatch: {cn}");
                return false;
            }

            try
            {
                Org.BouncyCastle.X509.X509Certificate bcCaCert =
                        new Org.BouncyCastle.X509.X509CertificateParser().ReadCertificate(_caCert.RawData);
                Org.BouncyCastle.X509.X509Certificate bcServerCert =
                    new Org.BouncyCastle.X509.X509CertificateParser().ReadCertificate(cert2.RawData);

                List<Org.BouncyCastle.X509.X509Certificate> rootCerts = new List<Org.BouncyCastle.X509.X509Certificate> { bcCaCert };
                List <Org.BouncyCastle.X509.X509Certificate> certChain = new List<Org.BouncyCastle.X509.X509Certificate> { bcServerCert };

                if (!EdBcTlsUtilities.ValidateCertChain(certChain, rootCerts))
                {
                    _output?.WriteLine("Certificate chain validation failed.");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _output?.WriteLine($"Certificate validation failed: {ex.Message}");
                return false;
            }
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
            _clientCts?.Cancel();
            ClientConnected?.Invoke(this, false);
        }

        // Periodischer Ping-Task
        private async Task KeepAliveLoopAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(2), ct).ConfigureAwait(false);
                    DateTime pingResult = await RpcService.PingAsync(ct).ConfigureAwait(false);
                    lock (_pingLock)
                    {
                        _pingDateTime = pingResult;
                    }

                    PingUpdated?.Invoke(this, pingResult);
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
            _keepAliveCts?.Cancel();
            _keepAliveCts?.Dispose();
            _keepAliveCts = null;

            _clientCts?.Cancel();
            _clientCts?.Dispose();
            _clientCts = null;

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
