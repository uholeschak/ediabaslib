using System;

namespace BMW.Rheingold.CoreFramework.Contracts.ConnectionManagement
{
    [Flags]
    public enum ConnectionTargetTypes
    {
        VCI = 1,
        MIB = 2,
        ALL = 3
    }
}
