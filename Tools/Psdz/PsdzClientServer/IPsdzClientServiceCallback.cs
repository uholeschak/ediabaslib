using System.Threading.Tasks;

namespace PsdzClientServer;

public interface IPsdzClientServiceCallback
{
    Task OnProgressChangedAsync(int percent, string message);
    Task OnOperationCompletedAsync(bool success);
}
