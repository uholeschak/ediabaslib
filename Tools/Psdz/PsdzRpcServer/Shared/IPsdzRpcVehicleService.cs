using PolyType;
using StreamJsonRpc;
using System.Threading.Tasks;

namespace PsdzRpcServer.Shared
{
    [JsonRpcContract, GenerateShape(IncludeMethods = MethodShapeFlags.PublicInstance)]
    public partial interface IPsdzRpcVehicleService
    {
        Task<bool> EnableVehicleProxy();
        Task<bool> SetVehicleResponse(PsdzVehicleResponse response);
    }
}
