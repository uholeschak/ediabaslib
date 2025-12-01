using System;
using System.CodeDom.Compiler;

namespace PsdzClient.Core.Container
{
    [Serializable]
    [GeneratedCode("Xsd2Code", "3.4.0.32990")]
    public enum TranslationMode
    {
        All,
        RuntimeOnly,
        DesigntimeOnly,
        None
    }
}