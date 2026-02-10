using System.Threading.Tasks;

namespace PsdzClientServer;

public class PsdzClientService : IPsdzClientService
{
    public Task<bool> Connect(string parameter)
    {
        // Implement connection logic here
        return Task.FromResult(true);
    }

    public Task<bool> Disconnect(string parameter)
    {
        // Implement disconnection logic here
        return Task.FromResult(true);
    }
}