using System;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model
{
    public interface IPsdzIstufe : IComparable<IPsdzIstufe>
    {
        bool IsValid { get; }

        string Value { get; }
    }
}
