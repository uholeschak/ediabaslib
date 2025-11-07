using System;

namespace PsdzClient.Core
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Event, AllowMultiple = false, Inherited = true)]
    public class AuthorAPIHiddenAttribute : Attribute
    {
    }
}