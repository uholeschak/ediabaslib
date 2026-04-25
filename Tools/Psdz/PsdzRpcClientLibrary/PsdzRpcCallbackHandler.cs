using PsdzRpcServer.Shared;
using System;
using System.Threading.Tasks;

namespace PsdzRpcClient
{
    public class PsdzRpcCallbackHandler : IPsdzRpcServiceCallback
    {
        public event EventHandler<bool> StartProgrammingCompleted;
        public event EventHandler<bool> StopProgrammingCompleted;
        public event EventHandler<ConnectVehicleEventArgs> ConnectVehicleCompleted;
        public event EventHandler<bool> DisconnectVehicleCompleted;
        public event EventHandler<VehicleFunctionsEventArgs> VehicleFunctionsCompleted;
        public event EventHandler<string> UpdateStatus;
        public event EventHandler<ProgressEventArgs> UpdateProgress;
        public event EventHandler UpdateOptions;
        public event EventHandler<PsdzRpcSwiRegisterEnum?> UpdateOptionSelections;
        public event EventHandler<ShowMessageEventArgs> ShowMessage;
        public event EventHandler<ShowMessageEventArgs> ShowMessageWait;
        public event EventHandler<TelSendQueueSizeEventArgs> TelSendQueueSize;
        public event EventHandler<ServiceInitializedEventArgs> ServiceInitialized;
        public event EventHandler<ulong> VehicleConnect;
        public event EventHandler<ulong> VehicleDisconnect;

        public Task OnStartProgrammingCompleted(bool success)
        {
            StartProgrammingCompleted?.Invoke(this, success);
            return Task.CompletedTask;
        }

        public Task OnStopProgrammingCompleted(bool success)
        {
            StopProgrammingCompleted?.Invoke(this, success);
            return Task.CompletedTask;
        }

        public Task OnConnectVehicleCompleted(bool success, string vin)
        {
            ConnectVehicleCompleted?.Invoke(this, new ConnectVehicleEventArgs(success, vin));
            return Task.CompletedTask;
        }

        public Task OnDisconnectVehicleCompleted(bool success)
        {
            DisconnectVehicleCompleted?.Invoke(this, success);
            return Task.CompletedTask;
        }

        public Task OnVehicleFunctionsCompleted(bool success, PsdzOperationType operationType)
        {
            VehicleFunctionsCompleted?.Invoke(this, new VehicleFunctionsEventArgs(success, operationType));
            return Task.CompletedTask;
        }

        public Task OnUpdateStatus(string message)
        {
            UpdateStatus?.Invoke(this, message);
            return Task.CompletedTask;
        }

        public Task OnUpdateProgress(int percent, bool marquee, string message)
        {
            ProgressEventArgs args = new ProgressEventArgs(percent, marquee, message);
            UpdateProgress?.Invoke(this, args);
            return Task.CompletedTask;
        }

        public Task OnUpdateOptions()
        {
            UpdateOptions?.Invoke(this, EventArgs.Empty);
            return Task.CompletedTask;
        }

        public Task OnUpdateOptionSelections(PsdzRpcSwiRegisterEnum? swiRegisterEnum)
        {
            UpdateOptionSelections?.Invoke(this, swiRegisterEnum);
            return Task.CompletedTask;
        }

        public async Task<bool> OnShowMessage(string message, bool okBtn, bool wait)
        {
            if (wait)
            {
                ShowMessageEventArgs args = new ShowMessageEventArgs(message, okBtn, waitForResult: true);
                return await Task.Run(() =>
                {
                    try
                    {
                        ShowMessageWait?.Invoke(this, args);
                    }
                    catch (Exception)
                    {
                        args.SetResult(false);
                    }

                    bool result = args.WaitForResult();
                    args.Dispose();
                    return result;
                }).ConfigureAwait(false);
            }
            else
            {
                using ShowMessageEventArgs args = new ShowMessageEventArgs(message, okBtn);
                ShowMessage?.Invoke(this, args);
                return args.Result;
            }
        }

        public Task<int> OnTelSendQueueSize()
        {
            TelSendQueueSizeEventArgs args = new TelSendQueueSizeEventArgs();
            TelSendQueueSize?.Invoke(this, args);
            return Task.FromResult(args.Result);
        }

        public Task OnServiceInitialized(string hostLogDir, bool loggingInitialized)
        {
            ServiceInitializedEventArgs args = new ServiceInitializedEventArgs(hostLogDir, loggingInitialized);
            ServiceInitialized?.Invoke(this, args);
            return Task.CompletedTask;
        }

        public Task OnVehicleConnect(ulong id)
        {
            VehicleConnect?.Invoke(this, id);
            return Task.CompletedTask;
        }

        public Task OnVehicleDisconnect(ulong id)
        {
            VehicleDisconnect?.Invoke(this, id);
            return Task.CompletedTask;
        }
    }
}
