using System;

namespace PsdzClientLibrary
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class PreserveSourceAttribute : Attribute
    {
        public string Hint;
    }
}