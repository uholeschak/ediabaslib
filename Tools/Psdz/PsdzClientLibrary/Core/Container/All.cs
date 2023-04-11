// BMW.Rheingold.Module.ISTA.All
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace PsdzClient.Core.Container
{
    [Serializable]
    [DesignerCategory("code")]
    [XmlInclude(typeof(Executable))]
    [DataContract(Name = "All")]
    [GeneratedCode("Xsd2Code", "3.4.0.32990")]
    public class All : ABranch, INotifyPropertyChanged
    {
        public new event PropertyChangedEventHandler PropertyChanged;

        public new virtual void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
