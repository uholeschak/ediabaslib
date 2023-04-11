// BMW.Rheingold.Module.ISTA.AConstraint
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace PsdzClient.Core
{
    [Serializable]
    [DataContract(Name = "AConstraint")]
    [GeneratedCode("Xsd2Code", "3.4.0.32990")]
    [XmlInclude(typeof(ValueConstraint))]
    [DesignerCategory("code")]
    [XmlInclude(typeof(StepConstraint))]
    [XmlInclude(typeof(RangeConstraint))]
    public abstract class AConstraint : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public virtual void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
