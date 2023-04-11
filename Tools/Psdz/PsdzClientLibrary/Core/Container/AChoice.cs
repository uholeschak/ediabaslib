// BMW.Rheingold.Module.ISTA.AChoice
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace PsdzClient.Core.Container
{
    [Serializable]
    [GeneratedCode("Xsd2Code", "3.4.0.32990")]
    [XmlInclude(typeof(MultipleChoice))]
    [XmlInclude(typeof(SingleChoice))]
    [XmlInclude(typeof(QuantityChoice))]
    [DataContract(Name = "AChoice")]
    [DesignerCategory("code")]
    public class AChoice : ABranch, INotifyPropertyChanged
    {
        private string defaultChildField;

        [XmlAttribute]
        [DataMember]
        public string DefaultChild
        {
            get
            {
                return defaultChildField;
            }
            set
            {
                if (defaultChildField != null)
                {
                    if (!defaultChildField.Equals(value))
                    {
                        defaultChildField = value;
                        OnPropertyChanged("DefaultChild");
                    }
                }
                else
                {
                    defaultChildField = value;
                    OnPropertyChanged("DefaultChild");
                }
            }
        }

        public new event PropertyChangedEventHandler PropertyChanged;

        public new virtual void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
