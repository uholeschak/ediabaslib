using System;

namespace BMW.Rheingold.CoreFramework
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Event, AllowMultiple = false, Inherited = true)]
    public class AuthorAPIHiddenAttribute : Attribute
    {
    }
}