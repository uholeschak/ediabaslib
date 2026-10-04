using PsdzClient.Core;
using System;

namespace BMW.Rheingold.CoreFramework.Contracts.Programming
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface IVehicleProfileChecksum : ICloneable
    {
        byte[] VpcCrc { get; }

        long VpcVersion { get; }

        string VpcCrcAsHex { get; }
    }
}
