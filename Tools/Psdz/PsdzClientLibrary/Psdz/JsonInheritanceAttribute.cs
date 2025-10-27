using System;

namespace BMW.Rheingold.Psdz
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true)]
    public class JsonInheritanceAttribute : Attribute
    {
        public string Key { get; }

        public Type Type { get; }

        public JsonInheritanceAttribute(string key, Type type)
        {
            Key = key;
            Type = type;
        }
    }
}