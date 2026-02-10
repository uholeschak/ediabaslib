using System;
using System.Threading.Tasks;

namespace PsdzClientServer;

public interface IPsdzClientService : IDisposable
{
    Task<bool> Connect(string parameter);
    Task<bool> Disconnect(string parameter);
}