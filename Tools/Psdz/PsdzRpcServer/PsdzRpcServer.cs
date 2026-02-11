using PsdzRpcServer.Shared;
using StreamJsonRpc;
using System;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;

namespace PsdzRpcServer
{
    public class PsdzRpcServer
    {
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
                    Console.WriteLine("Wait for client connection...");
                    await pipeServer.WaitForConnectionAsync(ct);
                    Console.WriteLine("Client connected");

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

                Console.WriteLine("Client disconnected.");
            }
            finally
            {
                pipeServer.Dispose();
            }
        }
    }
}
