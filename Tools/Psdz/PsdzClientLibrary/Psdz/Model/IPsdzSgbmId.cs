using System;

namespace BMW.Rheingold.Psdz.Model
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