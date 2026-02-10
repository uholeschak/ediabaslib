using PolyType;
using StreamJsonRpc;
using System.Threading.Tasks;

namespace PsdzClientServer;

[JsonRpcContract, GenerateShape(IncludeMethods = MethodShapeFlags.PublicInstance)]
public partial interface IPsdzClientServiceCallback
{
    Task OnProgressChangedAsync(int percent, string message);
    Task OnOperationCompletedAsync(bool success);
}
