using StreamJsonRpc;
using System;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;

namespace PsdzClientServer;

public class PsdzJsonRpcServer
{
    private const string PipeName = "PsdzJsonRpcPipe";

    public async Task StartAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            NamedPipeServerStream pipeServer = new NamedPipeServerStream(
                PipeName,
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
                Task task = HandleClientAsync(pipeServer);
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
            using JsonRpc jsonRpc = JsonRpc.Attach(pipeServer, new PsdzClientService());
            await jsonRpc.Completion;
            Console.WriteLine("Client disconnected.");
        }
        finally
        {
            pipeServer.Dispose();
        }
    }
}
