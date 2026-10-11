using System;

namespace BMW.Rheingold.CoreFramework.DatabaseProvider
{
    [Obsolete("Is not used anymore in Testmodules. Will be removed in 4.48!")]
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public enum BNMixed
    {
        HETEROGENEOUS,
        HOMOGENEOUS,
        SEPARATED,
        UNKNOWN
    }
}