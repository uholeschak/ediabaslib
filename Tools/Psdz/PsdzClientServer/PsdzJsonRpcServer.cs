using PsdzClientServer.Shared;
using StreamJsonRpc;
using System;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;

namespace PsdzClientServer;

public class PsdzJsonRpcServer
{
    public async Task StartAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            NamedPipeServerStream pipeServer = new NamedPipeServerStream(
                PsdzServiceConstants.PipeName,
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

            var callback = jsonRpc.Attach<IPsdzClientServiceCallback>();

            var service = new PsdzClientService(callback);
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
