using PsdzRpcServer.Shared;
using StreamJsonRpc;
using System;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;

namespace PsdzRpcClient;

public class PsdzRpcClient : IAsyncDisposable
{
    private NamedPipeClientStream _pipeClient;
    private JsonRpc _jsonRpc;

    public IPsdzRpcService RpcService { get; private set; }
    public PsdzRpcCallbackHandler CallbackHandler { get; } = new PsdzRpcCallbackHandler();

    public async Task ConnectAsync(CancellationToken ct = default)
    {
        _pipeClient = new NamedPipeClientStream(
            ".",
            PsdzRpcServiceConstants.PipeName,
            PipeDirection.InOut,
            PipeOptions.Asynchronous);

        Console.WriteLine($"Connecting with server...");
        await _pipeClient.ConnectAsync(ct);
        Console.WriteLine("Connected!");

        _jsonRpc = JsonRpc.Attach(_pipeClient);
        _jsonRpc.AddLocalRpcTarget(CallbackHandler);

        RpcService = _jsonRpc.Attach<IPsdzRpcService>();

        _jsonRpc.StartListening();
    }

    public async ValueTask DisposeAsync()
    {
        if (_jsonRpc != null)
        {
            _jsonRpc.Dispose();
        }
        if (_pipeClient != null)
        {
            await _pipeClient.DisposeAsync();
        }
    }
}