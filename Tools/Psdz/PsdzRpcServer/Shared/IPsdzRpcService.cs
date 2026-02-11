using System;
using System.Threading.Tasks;
using PolyType;
using StreamJsonRpc;

namespace PsdzRpcServer.Shared;

[JsonRpcContract, GenerateShape(IncludeMethods = MethodShapeFlags.PublicInstance)]
public partial interface IPsdzRpcService : IDisposable
{
    Task<bool> Connect(string parameter);
    Task<bool> Disconnect(string parameter);
}