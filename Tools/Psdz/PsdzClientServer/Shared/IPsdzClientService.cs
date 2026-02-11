using System;
using System.Threading.Tasks;
using PolyType;
using StreamJsonRpc;

namespace PsdzClientServer.Shared;

[JsonRpcContract, GenerateShape(IncludeMethods = MethodShapeFlags.PublicInstance)]
public partial interface IPsdzClientService : IDisposable
{
    Task<bool> Connect(string parameter);
    Task<bool> Disconnect(string parameter);
}