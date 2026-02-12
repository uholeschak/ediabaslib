using PsdzRpcServer.Shared;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PsdzClient;
using PsdzClient.Programming;

namespace PsdzRpcClient;

public class PsdzRpcCallbackHandler : IPsdzRpcServiceCallback
{
    public event EventHandler<ProgressEventArgs> ProgressChanged;
    public event EventHandler<bool> OperationCompleted;
    public event EventHandler<string> UpdatedStatus;
    public event EventHandler<(int percent, bool marquee, string message)> UpdateProgress;
    public event EventHandler<Dictionary<PsdzDatabase.SwiRegisterEnum, List<ProgrammingJobs.OptionsItem>>> UpdateOptions;

    public Task OnProgressChangedAsync(int percent, string message)
    {
        ProgressChanged?.Invoke(this, new ProgressEventArgs(percent, message));
        return Task.CompletedTask;
    }

    public Task OnOperationCompletedAsync(bool success)
    {
        OperationCompleted?.Invoke(this, success);
        return Task.CompletedTask;
    }

    public Task OnUpdateStatus(string message)
    {
        UpdatedStatus?.Invoke(this, message);
        return Task.CompletedTask;
    }

    public Task OnUpdateProgress(int percent, bool marquee, string message)
    {
        UpdateProgress?.Invoke(this, (percent, marquee, message));
        return Task.CompletedTask;
    }

    public Task OnUpdateOptionsAsync(Dictionary<PsdzDatabase.SwiRegisterEnum, List<ProgrammingJobs.OptionsItem>> optionsDict)
    {
        UpdateOptions?.Invoke(this, optionsDict);
        return Task.CompletedTask;
    }
}