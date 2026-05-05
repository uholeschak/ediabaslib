using PsdzRpcServer.Shared;
using StreamJsonRpc;
using System;
using System.IO;
using System.IO.Pipes;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace PsdzRpcClient
{
    public class PsdzRpcClient : IDisposable
    {
        private readonly TextWriter _output;
        private Stream _stream;
        private TcpClient _tcpClient; // nur bei TCP-Verbindung
        private JsonRpc _jsonRpc;

        public IPsdzRpcService RpcService { get; private set; }
        public event EventHandler<bool> ClientConnected;
        public PsdzRpcCallbackHandler CallbackHandler { get; } = new PsdzRpcCallbackHandler();

        public PsdzRpcClient(TextWriter output = null)
        {
            _output = output;
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
                _stream = _tcpClient.GetStream();
                _output?.WriteLine("Connected!");

                StartJsonRpc(synchronizationContext);
                return true;
            }
            catch (SocketException ex)
            {
                _output?.WriteLine($"TCP connect failed: {ex.Message}");
                _tcpClient?.Dispose();
                _tcpClient = null;
                return false;
            }
        }

        private void StartJsonRpc(SynchronizationContext synchronizationContext)
        {
            _jsonRpc = new JsonRpc(_stream);
            _jsonRpc.AddLocalRpcTarget(CallbackHandler);
            _jsonRpc.Disconnected += OnJsonRpcDisconnected;

            if (synchronizationContext != null)
                _jsonRpc.SynchronizationContext = synchronizationContext;

            RpcService = _jsonRpc.Attach<IPsdzRpcService>();
            _jsonRpc.StartListening();
            ClientConnected?.Invoke(this, true);
        }

        private void OnJsonRpcDisconnected(object sender, JsonRpcDisconnectedEventArgs e)
        {
            _output?.WriteLine($"RPC disconnected: {e.Reason} – {e.Description}");
            ClientConnected?.Invoke(this, false);
        }

        public void Dispose()
        {
            _jsonRpc?.Dispose();
            _jsonRpc = null;

            _stream?.Dispose();
            _stream = null;

            _tcpClient?.Dispose();
            _tcpClient = null;

            RpcService?.Dispose();
            RpcService = null;
        }
    }
}
