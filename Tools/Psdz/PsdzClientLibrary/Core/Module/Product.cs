using System;
using System.CodeDom.Compiler;
using System.Xml.Serialization;

namespace BMW.Rheingold.CoreFramework.DatabaseProvider
{
    [Serializable]
    [GeneratedCode("Xsd2Code", "3.4.0.38968")]
    [XmlType(Namespace = "http://www.bmw.com/ibase/beans/dealerdata")]
    public enum Product
    {
        Motorcycle,
        Vehicle
    }
}
