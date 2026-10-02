using PsdzClient.Core;
using System;

namespace BMW.Rheingold.CoreFramework.Contracts.Vehicle
{
    [Obsolete("Legacy Property, the ChassisType is retrieved from the Database and not used in test modules, thus it can be deleted.")]
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public enum ChassisType
    {
        LIM,
        TOU,
        COU,
        SAV,
        ROA,
        SH,
        CAB,
        SAT,
        HC,
        NONE,
        SAC,
        COM,
        CLU,
        HAT,
        SHA,
        UNKNOWN
    }
}