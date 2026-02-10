using StreamJsonRpc;
using System;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;

namespace PsdzClientServer;

public class PsdzJsonRpcServer
{
    private const string PipeName = "PsdzJsonRpcPipe";
    private readonly CancellationTokenSource _cts = new();

    public async Task StartAsync()
    {
        while (!_cts.Token.IsCancellationRequested)
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
                await pipeServer.WaitForConnectionAsync(_cts.Token);
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

    public void Stop() => _cts.Cancel();
}
