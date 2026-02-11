using System.Threading.Tasks;
using PsdzRpcServer.Shared;

namespace PsdzRpcServer;

public class PsdzRpcService : IPsdzRpcService
{
    private readonly IPsdzRpcServiceCallback _callback;

    public PsdzRpcService(IPsdzRpcServiceCallback callback)
    {
        _callback = callback;
    }

    public Task<bool> Connect(string parameter)
    {
        // Implement connection logic here
        return Task.FromResult(true);
    }

    public Task<bool> Disconnect(string parameter)
    {
        // Implement disconnection logic here
        return Task.FromResult(true);
    }

    public void Dispose()
    {
        // StreamJsonRpc recommends implementing Dispose to encourage developers
        // to dispose of the client RPC proxies generated from the interface.
        // // https://github.com/microsoft/vs-streamjsonrpc/blob/v2.19.27/doc/dynamicproxy.md#dispose-patterns
    }

}