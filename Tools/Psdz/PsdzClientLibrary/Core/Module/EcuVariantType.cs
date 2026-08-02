using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace BMW.Rheingold.InfoProvider.HDD.HDDLookup
{
    [Serializable]
    [GeneratedCode("Xsd2Code", "3.3.0.20460")]
    [DebuggerStepThrough]
    [DesignerCategory("code")]
    [XmlRoot(Namespace = "", IsNullable = true)]
    [DataContract(Name = "EcuVariantType")]
    public class EcuVariantType : INotifyPropertyChanged
    {
        private string compatibilityIdentifierField;

        private string formatField;

        [XmlAttribute]
        [DataMember]
        public string CompatibilityIdentifier
        {
            get
            {
                return compatibilityIdentifierField;
            }
            set
            {
                if (compatibilityIdentifierField != null)
                {
                    if (!compatibilityIdentifierField.Equals(value))
                    {
                        compatibilityIdentifierField = value;
                        OnPropertyChanged("CompatibilityIdentifier");
                    }
                }
                else
                {
                    compatibilityIdentifierField = value;
                    OnPropertyChanged("CompatibilityIdentifier");
                }
            }
        }

        [XmlAttribute]
        [DataMember]
        public string Format
        {
            get
            {
                return formatField;
            }
            set
            {
                if (formatField != null)
                {
                    if (!formatField.Equals(value))
                    {
                        formatField = value;
                        OnPropertyChanged("Format");
                    }
                }
                else
                {
                    formatField = value;
                    OnPropertyChanged("Format");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public virtual void OnPropertyChanged(string info)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }
    }
}
