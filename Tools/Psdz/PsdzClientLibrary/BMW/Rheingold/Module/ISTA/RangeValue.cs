using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace BMW.Rheingold.Module.ISTA
{
    [Serializable]
    [GeneratedCode("Xsd2Code", "3.4.0.26539")]
    [DesignerCategory("code")]
    [XmlRoot(Namespace = "", IsNullable = true)]
    [DataContract(Name = "RangeValue")]
    public class RangeValue : INotifyPropertyChanged
    {
        private string functionalValueField;
        private string technicalValueField;
        [XmlAttribute]
        [DataMember]
        public string FunctionalValue
        {
            get
            {
                return functionalValueField;
            }

            set
            {
                if (functionalValueField != null)
                {
                    if (!functionalValueField.Equals(value))
                    {
                        functionalValueField = value;
                        OnPropertyChanged("FunctionalValue");
                    }
                }
                else
                {
                    functionalValueField = value;
                    OnPropertyChanged("FunctionalValue");
                }
            }
        }

        [XmlAttribute]
        [DataMember]
        public string TechnicalValue
        {
            get
            {
                return technicalValueField;
            }

            set
            {
                if (technicalValueField != null)
                {
                    if (!technicalValueField.Equals(value))
                    {
                        technicalValueField = value;
                        OnPropertyChanged("TechnicalValue");
                    }
                }
                else
                {
                    technicalValueField = value;
                    OnPropertyChanged("TechnicalValue");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}