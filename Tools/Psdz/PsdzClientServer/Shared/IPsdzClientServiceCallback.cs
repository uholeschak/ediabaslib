using System.Threading.Tasks;
using PolyType;
using StreamJsonRpc;

namespace PsdzClientServer.Shared;

[JsonRpcContract, GenerateShape(IncludeMethods = MethodShapeFlags.PublicInstance)]
public partial interface IPsdzClientServiceCallback
{
    Task OnProgressChangedAsync(int percent, string message);
    Task OnOperationCompletedAsync(bool success);
}
