using PsdzClient.Core;
using System;

namespace PsdzClient.Programming
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface IVehicleProfileChecksum : ICloneable
    {
        byte[] VpcCrc { get; }

        long VpcVersion { get; }

        string VpcCrcAsHex { get; }
    }
}
