using System.Threading.Tasks;
using PolyType;
using StreamJsonRpc;

namespace PsdzRpcServer.Shared;

[JsonRpcContract, GenerateShape(IncludeMethods = MethodShapeFlags.PublicInstance)]
public partial interface IPsdzRpcServiceCallback
{
    Task OnProgressChangedAsync(int percent, string message);
    Task OnOperationCompletedAsync(bool success);
}
