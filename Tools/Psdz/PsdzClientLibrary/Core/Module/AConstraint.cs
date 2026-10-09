using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace BMW.Rheingold.Module.ISTA
{
    [Serializable]
    [XmlInclude(typeof(StepConstraint))]
    [XmlInclude(typeof(RangeConstraint))]
    [XmlInclude(typeof(ValueConstraint))]
    [GeneratedCode("Xsd2Code", "3.4.0.32990")]
    [DesignerCategory("code")]
    [DataContract(Name = "AConstraint")]
    public abstract class AConstraint : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
