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
        public async Task ConnectTcpAsync(string host, int port, SynchronizationContext synchronizationContext, CancellationToken ct)
        {
            _tcpClient = new TcpClient { NoDelay = true };

            _output?.WriteLine($"Connecting via TCP to {host}:{port}...");
            await _tcpClient.ConnectAsync(host, port).ConfigureAwait(false);
            _stream = _tcpClient.GetStream();
            _output?.WriteLine("Connected!");

            StartJsonRpc(synchronizationContext);
        }

        private void StartJsonRpc(SynchronizationContext synchronizationContext)
        {
            _jsonRpc = new JsonRpc(_stream);
            _jsonRpc.AddLocalRpcTarget(CallbackHandler);

            if (synchronizationContext != null)
            {
                _jsonRpc.SynchronizationContext = synchronizationContext;
            }

            RpcService = _jsonRpc.Attach<IPsdzRpcService>();
            _jsonRpc.StartListening();
            ClientConnected?.Invoke(this, true);
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
