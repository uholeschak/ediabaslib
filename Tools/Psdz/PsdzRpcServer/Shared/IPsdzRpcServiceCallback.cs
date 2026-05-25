using PolyType;
using StreamJsonRpc;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PsdzRpcServer.Shared
{
    [JsonRpcContract, GenerateShape(IncludeMethods = MethodShapeFlags.PublicInstance)]
    public partial interface IPsdzRpcServiceCallback : IPsdzRpcVehicleCallback
    {
        Task<DateTime> OnPing();
        Task OnStartProgrammingCompleted(bool success);
        Task OnStopProgrammingCompleted(bool success);
        Task OnConnectVehicleCompleted(bool success, string vin, bool licenseValid);
        Task OnDisconnectVehicleCompleted(bool success);
        Task OnVehicleFunctionsCompleted(bool success, PsdzOperationType operationType);
        Task OnUpdateStatus(string message);
        Task OnUpdateProgress(int percent, bool marquee, string message);
        Task OnUpdateOptions();
        Task OnUpdateOptionSelections(PsdzRpcSwiRegisterEnum? swiRegisterEnum);
        Task<bool> OnShowMessage(string message, bool okBtn, bool wait);
        Task<int> OnTelSendQueueSize();
        Task OnServiceInitialized(string hostLogDir, bool loggingInitialized);
        Task<PsdzRpcAppInfo> OnGetAppInfo();
    }
}
