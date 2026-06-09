using System;

namespace PsdzClient.Core
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public sealed class IgnoreForReopenedOperationsAttribute : Attribute
    {
    }
}
