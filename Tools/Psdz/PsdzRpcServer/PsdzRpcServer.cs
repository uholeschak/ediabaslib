using PsdzRpcServer.Shared;
using StreamJsonRpc;
using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;

namespace PsdzRpcServer
{
    public class PsdzRpcServer
    {
        private readonly TextWriter _output;
        private int _clientCount;
        private bool _hadClients;
        private readonly TaskCompletionSource<bool> _allClientsDisconnected = new TaskCompletionSource<bool>();

        /// <summary>
        /// Wird ausgelöst wenn der letzte Client die Verbindung trennt.
        /// </summary>
        public Task AllClientsDisconnected => _allClientsDisconnected.Task;

        public PsdzRpcServer(TextWriter output = null)
        {
            _output = output;
            _clientCount = 0;
            _hadClients = false;
        }

        public async Task StartAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                NamedPipeServerStream pipeServer = new NamedPipeServerStream(
                    PsdzRpcServiceConstants.PipeName,
                    PipeDirection.InOut,
                    NamedPipeServerStream.MaxAllowedServerInstances,
                    PipeTransmissionMode.Byte,
                    PipeOptions.Asynchronous);

                try
                {
                    _output?.WriteLine("Wait for client connection...");
                    await pipeServer.WaitForConnectionAsync(ct);

                    int count = Interlocked.Increment(ref _clientCount);
                    _hadClients = true;
                    _output?.WriteLine($"Client connected. Active clients: {count}");

                    // JsonRpc for this connection
                    _ = HandleClientAsync(pipeServer);
                }
                catch (OperationCanceledException)
                {
                    pipeServer.Dispose();
                    break;
                }
            }
        }

        private async Task HandleClientAsync(NamedPipeServerStream pipeServer)
        {
            try
            {
                using var jsonRpc = new JsonRpc(pipeServer);

                var callback = jsonRpc.Attach<IPsdzRpcServiceCallback>();

                var service = new PsdzRpcService(callback);
                jsonRpc.AddLocalRpcTarget(service);

                jsonRpc.StartListening();
                await jsonRpc.Completion;

                _output?.WriteLine("Client disconnected.");
            }
            finally
            {
                pipeServer.Dispose();

                int count = Interlocked.Decrement(ref _clientCount);
                _output?.WriteLine($"Active clients: {count}");

                if (count == 0 && _hadClients)
                {
                    _allClientsDisconnected.TrySetResult(true);
                }
            }
        }
    }
}
