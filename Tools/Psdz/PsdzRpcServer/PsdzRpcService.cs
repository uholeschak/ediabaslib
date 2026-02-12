using PsdzClient;
using PsdzClient.Programming;
using PsdzRpcServer.Shared;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PsdzRpcServer;

public class PsdzRpcService : IPsdzRpcService
{
    private readonly IPsdzRpcServiceCallback _callback;
    private  readonly ProgrammingJobs _programmingJobs;

    public PsdzRpcService(IPsdzRpcServiceCallback callback)
    {
        _callback = callback;
        _programmingJobs = new ProgrammingJobs(PsdzRpcServiceConstants.DealerId);
        _programmingJobs.UpdateStatusEvent += UpdateStatus;
        _programmingJobs.ProgressEvent += UpdateProgress;
        _programmingJobs.UpdateOptionsEvent += UpdateOptions;
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

    private void UpdateStatus(string message = null)
    {
        _callback.OnUpdateStatus(message);
    }

    private void UpdateProgress(int percent, bool marquee, string message = null)
    {
        _callback.OnUpdateProgress(percent, marquee, message);
    }

    private void UpdateOptions(Dictionary<PsdzDatabase.SwiRegisterEnum, List<ProgrammingJobs.OptionsItem>> optionsDict)
    {
        _callback.OnUpdateOptionsAsync(optionsDict);
    }

    public void Dispose()
    {
        // StreamJsonRpc recommends implementing Dispose to encourage developers
        // to dispose of the client RPC proxies generated from the interface.
        // // https://github.com/microsoft/vs-streamjsonrpc/blob/v2.19.27/doc/dynamicproxy.md#dispose-patterns
        _programmingJobs.UpdateStatusEvent -= UpdateStatus;
        _programmingJobs.ProgressEvent -= UpdateProgress;
        _programmingJobs.UpdateOptionsEvent -= UpdateOptions;
        _programmingJobs.Dispose();
    }
}
