using System;

namespace PsdzClientLibrary
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Enum | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class PreserveSourceAttribute : Attribute
    {
        public string Hint;

        public bool Removed;

        public bool KeepAttribute;
    }
}