using System;
using System.CodeDom.Compiler;
using System.Xml.Serialization;

namespace BMW.Rheingold.Module.ISTA
{
    [Serializable]
    [GeneratedCode("Xsd2Code", "3.4.0.26539")]
    [XmlType(AnonymousType = true)]
    public enum DocumentStatementAction
    {
        Add,
        Remove,
        RemoveAll
    }
}
