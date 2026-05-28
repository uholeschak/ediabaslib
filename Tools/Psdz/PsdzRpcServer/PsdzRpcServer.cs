using PsdzRpcServer.Shared;
using StreamJsonRpc;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.IO.Pipes;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.AccessControl;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;

namespace PsdzRpcServer
{
    public class PsdzRpcServer : IDisposable
    {
        public const int TslHandshakeTimeoutSeconds = 15;
        private readonly string _dealerId;
        private readonly TextWriter _output;
        private readonly int? _tcpPort;
        private readonly X509Certificate2 _caCert;
        private readonly X509Certificate2 _serverCert;
        private readonly PsdzLicenseCheck _licenseCheck;
        private int _clientCount;
        private bool _hadClients;
        private bool _disposed;
        private readonly TaskCompletionSource<bool> _allClientsDisconnected = new TaskCompletionSource<bool>();
        private readonly ConcurrentDictionary<PsdzRpcService, bool> _activeServices = new ConcurrentDictionary<PsdzRpcService, bool>();

        /// <summary>
        /// Wird ausgelöst wenn der letzte Client die Verbindung trennt.
        /// </summary>
        public Task AllClientsDisconnected => _allClientsDisconnected.Task;

        public PsdzRpcServer(string dealerId, TextWriter output = null, PsdzLicenseCheck licenseCheck = null,
            int? tcpPort = null, string caCertPath = null, string serverPfxPath = null)
        {
            _dealerId = dealerId;
            _output = output;
            _licenseCheck = licenseCheck;
            _tcpPort = tcpPort;

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

            if (!string.IsNullOrEmpty(serverPfxPath))
            {
                _serverCert = PsdzRpcCertificateHelper.LoadCertificate(serverPfxPath)
                           ?? PsdzRpcCertificateHelper.LoadEmbeddedCertificate(assembly, Path.GetFileName(serverPfxPath));
                if (_serverCert == null)
                {
                    throw new InvalidOperationException("Failed to load server certificate.");
                }
            }
        }

        public async Task StartAsync(CancellationToken ct)
        {
            if (_tcpPort.HasValue)
            {
                await StartTcpAsync(_tcpPort.Value, ct).ConfigureAwait(false);
            }
            else
            {
                await StartPipeAsync(ct).ConfigureAwait(false);
            }
        }

        private async Task StartPipeAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                NamedPipeServerStream pipeServer = CreatePipeServer();
                try
                {
                    _output?.WriteLine("Wait for client connection (Pipe)...");
                    await pipeServer.WaitForConnectionAsync(ct).ConfigureAwait(false);
                    OnClientAccepted(pipeServer);
                }
                catch (OperationCanceledException)
                {
                    pipeServer.Dispose();
                    break;
                }
            }
        }

        private async Task StartTcpAsync(int port, CancellationToken ct)
        {
            TcpListener listener = new TcpListener(IPAddress.IPv6Any, port);
            listener.Server.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, false);
            listener.Start();
            string connectionType = _serverCert != null ? "SSL/TLS" : "plain";
            _output?.WriteLine($"TCP server listening on port {port} ({connectionType})...");

            try
            {
                while (!ct.IsCancellationRequested)
                {
                    Task<TcpClient> acceptTask = listener.AcceptTcpClientAsync();
                    Task cancelTask = Task.Delay(Timeout.Infinite, ct);
                    Task completed = await Task.WhenAny(acceptTask, cancelTask).ConfigureAwait(false);
                    if (completed == cancelTask)
                    {
                        break;
                    }

                    TcpClient tcpClient = await acceptTask.ConfigureAwait(false);
                    tcpClient.NoDelay = true;
                    tcpClient.SendBufferSize = 65536;
                    tcpClient.ReceiveBufferSize = 65536;
                    tcpClient.Client.SendTimeout = 30000; // 30s
                    tcpClient.Client.ReceiveTimeout = 30000; // 30s
                    tcpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
#if NET
                    tcpClient.Client.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveTime, 60); // Erste Probe nach 60s Inaktivität
                    tcpClient.Client.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveInterval, 15); // Proben alle 15s
                    tcpClient.Client.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveRetryCount, 5); // 5 Versuche
#endif
                    if (_serverCert != null)
                    {
                        // TLS-Handshake als fire-and-forget
                        _ = AcceptTlsClientAsync(tcpClient, _caCert, _serverCert);
                    }
                    else
                    {
                        OnClientAccepted(tcpClient.GetStream(), tcpClient);
                    }
                }
            }
            finally
            {
                listener.Stop();
            }
        }
        private void OnClientAccepted(Stream stream, IDisposable owner = null)
        {
            int count = Interlocked.Increment(ref _clientCount);
            _hadClients = true;
            _output?.WriteLine($"Client connected. Active clients: {count}");
            _ = HandleClientAsync(stream, owner);
        }

        private async Task HandleClientAsync(Stream stream, IDisposable owner = null)
        {
            bool disconnected = false;
            using CancellationTokenSource clientCts = new CancellationTokenSource();
            using JsonRpc jsonRpc = new JsonRpc(stream);
            using PsdzRpcService service = new PsdzRpcService(jsonRpc.Attach<IPsdzRpcServiceCallback>(), _dealerId, _licenseCheck);

            try
            {
                _activeServices.TryAdd(service, true);
                jsonRpc.AddLocalRpcTarget(service);
                jsonRpc.Disconnected += (s, e) =>
                {
                    try
                    {
                        if (Volatile.Read(ref disconnected))
                        {
                            return;
                        }

                        clientCts.Cancel();
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }; jsonRpc.StartListening();

                // Keep-Alive parallel zu jsonRpc.Completion
                Task completionTask = jsonRpc.Completion;
                Task keepAliveTask = KeepAliveLoopAsync(service, clientCts.Token);

                await Task.WhenAny(completionTask, keepAliveTask).ConfigureAwait(false);

                _output?.WriteLine("Client disconnected.");
            }
            finally
            {
                Volatile.Write(ref disconnected, true);
                _activeServices.TryRemove(service, out _);
                clientCts.Cancel();
                owner?.Dispose();
                stream.Dispose();

                int count = Interlocked.Decrement(ref _clientCount);
                _output?.WriteLine($"Active clients: {count}");

                if (count == 0 && _hadClients)
                {
                    _allClientsDisconnected.TrySetResult(true);
                }
            }
        }

        private async Task KeepAliveLoopAsync(PsdzRpcService service, CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(2), ct).ConfigureAwait(false);
                    await service.PingClient().ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _output?.WriteLine($"Keep-alive failed, client seems disconnected: {ex.Message}");
                    break;
                }
            }
        }
        private async Task AcceptTlsClientAsync(TcpClient tcpClient, X509Certificate2 caCert, X509Certificate2 serverCert)
        {
            SslStream sslStream = null;
            try
            {
                sslStream = new SslStream(
                    tcpClient.GetStream(),
                    leaveInnerStreamOpen: false,
                    userCertificateValidationCallback: (sender, cert, chain, errors) =>
                        ValidateClientCertificate(cert, caCert));
#if NET
                using CancellationTokenSource tlsTimeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(TslHandshakeTimeoutSeconds));
                await sslStream.AuthenticateAsServerAsync(new SslServerAuthenticationOptions
                {
                    ServerCertificate              = serverCert,
                    ClientCertificateRequired      = caCert != null,
                    EnabledSslProtocols            = PsdzRpcServiceConstants.DefaultSslProtocols,
                    CertificateRevocationCheckMode = X509RevocationMode.NoCheck
                }, tlsTimeoutCts.Token).ConfigureAwait(false);
#else
                // .NET Framework: kein CancellationToken-Overload, daher Task.WhenAny mit Timeout
                Task authTask = sslStream.AuthenticateAsServerAsync(
                    serverCertificate:          serverCert,
                    clientCertificateRequired:  caCert != null,
                    enabledSslProtocols:        PsdzRpcServiceConstants.DefaultSslProtocols,
                    checkCertificateRevocation: false);

                if (await Task.WhenAny(authTask, Task.Delay(TimeSpan.FromSeconds(TslHandshakeTimeoutSeconds))).ConfigureAwait(false) != authTask)
                {
                    throw new TimeoutException("TLS handshake timed out.");
                }

                await authTask.ConfigureAwait(false);
#endif
                _output?.WriteLine("TLS handshake successful.");
                OnClientAccepted(sslStream, tcpClient);
            }
            catch (OperationCanceledException)
            {
                _output?.WriteLine("TLS handshake timed out.");
                sslStream?.Dispose();
                tcpClient.Dispose();
            }
            catch (Exception ex)
            {
                _output?.WriteLine($"TLS handshake failed: {ex.Message}");
                sslStream?.Dispose();
                tcpClient.Dispose();
            }
        }

        private bool ValidateClientCertificate(X509Certificate cert, X509Certificate2 caCert)
        {
            if (caCert == null) return true;  // kein mTLS – alles erlauben
            if (cert == null)  return false;

            X509Chain chain = new X509Chain();
            chain.ChainPolicy.ExtraStore.Add(caCert);
            chain.ChainPolicy.RevocationMode    = X509RevocationMode.NoCheck;
            chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllowUnknownCertificateAuthority;

            bool valid = chain.Build(new X509Certificate2(cert));
            return valid && chain.ChainElements[chain.ChainElements.Count - 1]
                .Certificate.Thumbprint == caCert.Thumbprint;
        }

        private NamedPipeServerStream CreatePipeServer()
        {
            PipeSecurity pipeSecurity = new PipeSecurity();

            // Aktueller Benutzer (Server-Prozess) darf alles
            pipeSecurity.AddAccessRule(new PipeAccessRule(
                WindowsIdentity.GetCurrent().User,
                PipeAccessRights.FullControl,
                AccessControlType.Allow));

            pipeSecurity.AddAccessRule(new PipeAccessRule(
                new SecurityIdentifier(WellKnownSidType.AuthenticatedUserSid, null),
                PipeAccessRights.ReadWrite,
                AccessControlType.Allow));

            // IIS App Pool Identitäten sind NICHT in AuthenticatedUsers enthalten
            // → Everyone (World) hinzufügen für lokale IPC
            pipeSecurity.AddAccessRule(new PipeAccessRule(
                new SecurityIdentifier(WellKnownSidType.WorldSid, null),
                PipeAccessRights.ReadWrite,
                AccessControlType.Allow));

#if NET
            return NamedPipeServerStreamAcl.Create(
                PsdzRpcServiceConstants.PipeName,
                PipeDirection.InOut,
                NamedPipeServerStream.MaxAllowedServerInstances,
                PipeTransmissionMode.Byte,
                PipeOptions.Asynchronous,
                inBufferSize: 0,
                outBufferSize: 0,
                pipeSecurity);
#else
            return new NamedPipeServerStream(
                PsdzRpcServiceConstants.PipeName,
                PipeDirection.InOut,
                NamedPipeServerStream.MaxAllowedServerInstances,
                PipeTransmissionMode.Byte,
                PipeOptions.Asynchronous,
                inBufferSize: 0,
                outBufferSize: 0,
                pipeSecurity);
#endif
        }

        public int ClientCount => _clientCount;

        /// <summary>
        /// Prüft ob eine VIN bereits von einem anderen Client verwendet wird.
        /// </summary>
        public bool IsVinDuplicated(string vin, PsdzRpcService excludeService = null)
        {
            if (string.IsNullOrEmpty(vin))
            {
                return false;
            }

            foreach (PsdzRpcService service in _activeServices.Keys)
            {
                if (service == excludeService)
                {
                    continue;
                }

                string activeVin = service.GetActiveVin();
                if (string.Equals(activeVin, vin, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            _serverCert?.Dispose();
            _caCert?.Dispose();
        }
    }
}
