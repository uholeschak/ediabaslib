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
    public event EventHandler<string> UpdateStatus;
    public event EventHandler<(int percent, bool marquee, string message)> UpdateProgress;
    public event EventHandler UpdateOptions;
    public event EventHandler<PsdzDatabase.SwiRegisterEnum?> UpdateOptionSelections;
    public event EventHandler<ShowMessageEventArgs> ShowMessage;
    public event EventHandler<ShowMessageEventArgs> ShowMessageWait;
    public event EventHandler<TelSendQueueSizeEventArgs> TelSendQueueSize;
    public event EventHandler<string> ServiceInitialized;

    public Task OnProgressChanged(int percent, string message)
    {
        ProgressChanged?.Invoke(this, new ProgressEventArgs(percent, message));
        return Task.CompletedTask;
    }

    public Task OnOperationCompleted(bool success)
    {
        OperationCompleted?.Invoke(this, success);
        return Task.CompletedTask;
    }

    public Task OnUpdateStatus(string message)
    {
        UpdateStatus?.Invoke(this, message);
        return Task.CompletedTask;
    }

    public Task OnUpdateProgress(int percent, bool marquee, string message)
    {
        UpdateProgress?.Invoke(this, (percent, marquee, message));
        return Task.CompletedTask;
    }

    public Task OnUpdateOptions()
    {
        UpdateOptions?.Invoke(this, EventArgs.Empty);
        return Task.CompletedTask;
    }

    public Task OnUpdateOptionSelections(PsdzDatabase.SwiRegisterEnum? swiRegisterEnum)
    {
        UpdateOptionSelections?.Invoke(this, swiRegisterEnum);
        return Task.CompletedTask;
    }

    public Task<bool> OnShowMessage(string message, bool okBtn, bool wait)
    {
        ShowMessageEventArgs args = new ShowMessageEventArgs(message, okBtn);
        if (wait)
        {
            ShowMessageWait?.Invoke(this, args);
        }
        else
        {
            ShowMessage?.Invoke(this, args);
        }
        return Task.FromResult(args.Result);
    }

    public Task<int> OnTelSendQueueSize()
    {
        TelSendQueueSizeEventArgs args = new TelSendQueueSizeEventArgs();
        TelSendQueueSize?.Invoke(this, args);
        return Task.FromResult(args.Result);
    }

    public Task OnServiceInitialized(string hostLogDir)
    {
        ServiceInitialized?.Invoke(this, hostLogDir);
        return Task.CompletedTask;
    }
}
