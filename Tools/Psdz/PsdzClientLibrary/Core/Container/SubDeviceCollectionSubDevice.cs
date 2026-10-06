using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using PsdzClient.Core.Container;

namespace BMW.Rheingold.Module.ISTA
{
    [Serializable]
    [GeneratedCode("Xsd2Code", "3.4.0.32990")]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    [DataContract(Name = "SubDeviceCollectionSubDevice")]
    public class SubDeviceCollectionSubDevice : SubDevice, INotifyPropertyChanged
    {
        public new event PropertyChangedEventHandler PropertyChanged;
        public new virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}