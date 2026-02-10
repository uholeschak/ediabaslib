using PolyType;
using StreamJsonRpc;
using System;
using System.Threading.Tasks;

namespace PsdzClientServer;

[JsonRpcContract, GenerateShape(IncludeMethods = MethodShapeFlags.PublicInstance)]
public partial interface IPsdzClientService : IDisposable
{
    Task<bool> Connect(string parameter);
    Task<bool> Disconnect(string parameter);
}