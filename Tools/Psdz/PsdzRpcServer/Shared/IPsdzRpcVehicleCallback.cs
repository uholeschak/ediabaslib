using System.Threading.Tasks;
using PolyType;
using StreamJsonRpc;

namespace PsdzRpcServer.Shared
{
    [JsonRpcContract, GenerateShape(IncludeMethods = MethodShapeFlags.PublicInstance)]
    public partial interface IPsdzRpcVehicleCallback
    {
        Task OnVehicleConnect(ulong id);
        Task OnVehicleDisconnect(ulong id);
        Task OnVehicleSend(ulong id, byte[] data);
    }
}
