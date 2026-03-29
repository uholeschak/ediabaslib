using System.Threading.Tasks;
using PolyType;
using StreamJsonRpc;

namespace PsdzRpcServer.Shared
{
    [JsonRpcContract, GenerateShape(IncludeMethods = MethodShapeFlags.PublicInstance)]
    public partial interface IPsdzRpcServiceCallback
    {
        Task OnStartProgrammingCompleted(bool success);
        Task OnStopProgrammingCompleted(bool success);
        Task OnConnectVehicleCompleted(bool success, string vin);
        Task OnDisconnectVehicleCompleted(bool success);
        Task OnVehicleFunctionsCompleted(bool success, PsdzOperationType operationType);
        Task OnUpdateStatus(string message);
        Task OnUpdateProgress(int percent, bool marquee, string message);
        Task OnUpdateOptions();
        Task OnUpdateOptionSelections(PsdzRpcSwiRegisterEnum? swiRegisterEnum);
        Task<bool> OnShowMessage(string message, bool okBtn, bool wait);
        Task<int> OnTelSendQueueSize();
        Task OnServiceInitialized(string hostLogDir);
    }
}
