using PsdzRpcServer.Shared;
using StreamJsonRpc;
using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;

namespace PsdzRpcClient
{
    public class PsdzRpcClient : IDisposable
    {
        private readonly TextWriter _output;
        private NamedPipeClientStream _pipeClient;
        private JsonRpc _jsonRpc;

        public IPsdzRpcService RpcService { get; private set; }
        public event EventHandler<bool> ClientConnected;
        public PsdzRpcCallbackHandler CallbackHandler { get; } = new PsdzRpcCallbackHandler();

        public PsdzRpcClient(TextWriter output = null)
        {
            _output = output;
        }

        public async Task ConnectAsync(SynchronizationContext synchronizationContext, CancellationToken ct)
        {
            _pipeClient = new NamedPipeClientStream(
                ".",
                PsdzRpcServiceConstants.PipeName,
                PipeDirection.InOut,
                PipeOptions.Asynchronous);

            _output?.WriteLine("Connecting with server...");
            await _pipeClient.ConnectAsync(ct).ConfigureAwait(false);
            _output?.WriteLine("Connected!");

            _jsonRpc = new JsonRpc(_pipeClient);
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

            _pipeClient?.Dispose();
            _pipeClient = null;

            RpcService?.Dispose();
            RpcService = null;
        }
    }
}
