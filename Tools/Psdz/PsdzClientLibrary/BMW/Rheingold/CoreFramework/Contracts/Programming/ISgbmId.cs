using System;

namespace BMW.Rheingold.CoreFramework.Contracts.Programming
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface ISgbmId : IComparable<ISgbmId>, IEquatable<ISgbmId>
    {
        long Id { get; }

        int MainVersion { get; }

        int PatchVersion { get; }

        string ProcessClass { get; }

        int SubVersion { get; }

        string HexString { get; }
    }
}
