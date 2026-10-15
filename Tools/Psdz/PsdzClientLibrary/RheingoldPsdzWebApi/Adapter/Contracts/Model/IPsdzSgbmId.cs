using System;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model
{
    public interface IPsdzSgbmId : IComparable<IPsdzSgbmId>
    {
        string HexString { get; }

        string Id { get; }

        long IdAsLong { get; }

        int MainVersion { get; }

        int PatchVersion { get; }

        string ProcessClass { get; }

        int SubVersion { get; }

        string SGBMIDVersion { get; }
    }
}