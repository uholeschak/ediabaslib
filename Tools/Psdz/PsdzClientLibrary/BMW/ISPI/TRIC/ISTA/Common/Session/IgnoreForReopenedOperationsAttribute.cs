using System;

namespace BMW.ISPI.TRIC.ISTA.Common.Session
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public sealed class IgnoreForReopenedOperationsAttribute : Attribute
    {
    }
}
