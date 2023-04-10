// BMW.Rheingold.Module.ISTA.Sequence
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace PsdzClient.Core
{
    [Serializable]
    [DataContract(Name = "Sequence")]
    [DesignerCategory("code")]
    [GeneratedCode("Xsd2Code", "3.4.0.32990")]
    public class Sequence : ABranch, INotifyPropertyChanged
    {
        private int defaultSizeField;

        private bool defaultSizeFieldSpecified;

        [XmlAttribute]
        [DataMember]
        public int DefaultSize
        {
            get
            {
                return defaultSizeField;
            }
            set
            {
                if (!defaultSizeField.Equals(value))
                {
                    defaultSizeField = value;
                    OnPropertyChanged("DefaultSize");
                }
            }
        }

        [XmlIgnore]
        [DataMember]
        public bool DefaultSizeSpecified
        {
            get
            {
                return defaultSizeFieldSpecified;
            }
            set
            {
                if (!defaultSizeFieldSpecified.Equals(value))
                {
                    defaultSizeFieldSpecified = value;
                    OnPropertyChanged("DefaultSizeSpecified");
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
