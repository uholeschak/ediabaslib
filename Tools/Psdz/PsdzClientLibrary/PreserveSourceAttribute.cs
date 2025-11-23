using System;

namespace PsdzClient
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Enum | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Event, AllowMultiple = false, Inherited = false)]
    public class PreserveSourceAttribute : Attribute
    {
        public string Hint;

        public bool Removed;

        public bool Placeholder;

        public bool KeepAttribute;

        public bool AccessModified;

        public bool InheritanceModified;

        public bool AttributesModified;
    }

    public struct PlaceholderType
    {
        public static readonly PlaceholderType Value = default;
    }
}