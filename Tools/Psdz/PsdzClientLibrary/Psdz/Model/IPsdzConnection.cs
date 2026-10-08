using System;

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