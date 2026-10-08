using System;
using BMW.Rheingold.Psdz.Model;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model
{
    public interface IPsdzConnection
    {
        Guid Id { get; }

        IPsdzTargetSelector TargetSelector { get; }

        int Port { get; }

        bool IsTlsAllowed { get; }
    }
}