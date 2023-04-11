using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace PsdzClient.Core
{
    [Serializable]
    [XmlType(AnonymousType = true)]
    [DataContract(Name = "SubDeviceCollectionSubDevice")]
    [DesignerCategory("code")]
    [GeneratedCode("Xsd2Code", "3.4.0.32990")]
    public class SubDeviceCollectionSubDevice : SubDevice, INotifyPropertyChanged
    {
        public new event PropertyChangedEventHandler PropertyChanged;

        public new virtual void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
