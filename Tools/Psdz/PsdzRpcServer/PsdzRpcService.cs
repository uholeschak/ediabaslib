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

    public async Task<bool> Connect(string parameter)
    {
        // Implement connection logic here
        for (int i = 0; i <= 100; i += 20)
        {
            await _callback.OnProgressChangedAsync(i, $"Connecting... {i}%");
            await Task.Delay(500);
        }
        await _callback.OnOperationCompletedAsync(true);
        return true;
    }

    public async Task<bool> Disconnect(string parameter)
    {
        // Implement disconnection logic here
        for (int i = 0; i <= 100; i += 20)
        {
            await _callback.OnProgressChangedAsync(i, $"Disconnecting... {i}%");
            await Task.Delay(500);
        }
        await _callback.OnOperationCompletedAsync(true);
        return true;
    }

    public void Dispose()
    {
        // StreamJsonRpc recommends implementing Dispose to encourage developers
        // to dispose of the client RPC proxies generated from the interface.
        // // https://github.com/microsoft/vs-streamjsonrpc/blob/v2.19.27/doc/dynamicproxy.md#dispose-patterns
    }

}