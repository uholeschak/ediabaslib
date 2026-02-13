using System.Collections.Generic;
using System.Threading.Tasks;
using PolyType;
using PsdzClient;
using PsdzClient.Programming;
using StreamJsonRpc;

namespace PsdzRpcServer.Shared;

[JsonRpcContract, GenerateShape(IncludeMethods = MethodShapeFlags.PublicInstance)]
public partial interface IPsdzRpcServiceCallback
{
    Task OnProgressChangedAsync(int percent, string message);
    Task OnOperationCompletedAsync(bool success);
    Task OnUpdateStatus(string message);
    Task OnUpdateProgress(int percent, bool marquee, string message);
    Task OnUpdateOptions(Dictionary<PsdzDatabase.SwiRegisterEnum, List<ProgrammingJobs.OptionsItem>> optionsDict);
    Task OnUpdateOptionSelections(PsdzDatabase.SwiRegisterEnum? swiRegisterEnum);
    Task<bool> OnShowMessage(string message, bool okBtn, bool wait);
}
