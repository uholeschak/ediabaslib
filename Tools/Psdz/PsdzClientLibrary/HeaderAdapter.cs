using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Xml.Serialization;
namespace PsdzClient.Core
{
    [Serializable]
    [DesignerCategory("code")]
    [DataContract(Name = "HeaderAdapter")]
    [XmlType(AnonymousType = true)]
    [GeneratedCode("Xsd2Code", "3.4.0.32990")]
    public class HeaderAdapter : Adapter, INotifyPropertyChanged
    {
        public new event PropertyChangedEventHandler PropertyChanged;

        public new virtual void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
