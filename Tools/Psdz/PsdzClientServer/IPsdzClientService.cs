using System.Threading.Tasks;

namespace PsdzClientServer;

public interface IPsdzClientService
{
    Task<bool> Connect(string parameter);
    Task<bool> Disconnect(string parameter);
}