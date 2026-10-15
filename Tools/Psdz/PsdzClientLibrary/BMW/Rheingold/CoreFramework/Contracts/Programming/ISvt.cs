using System;
using System.Collections.Generic;

namespace BMW.Rheingold.CoreFramework.Contracts.Programming
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface ISvt
    {
        IEnumerable<IEcuObj> Ecus { get; }

        byte[] HoSignature { get; }

        DateTime HoSignatureDate { get; }

        int Version { get; }

        bool RemoveEcu(IEcuObj ecu);
    }
}
