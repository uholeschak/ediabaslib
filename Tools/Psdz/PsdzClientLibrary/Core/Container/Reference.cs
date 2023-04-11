// BMW.Rheingold.Module.ISTA.Reference
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace PsdzClient.Core
{
    [Serializable]
    [DataContract(Name = "Reference")]
    [GeneratedCode("Xsd2Code", "3.4.0.32990")]
    [DesignerCategory("code")]
    public class Reference : ANode, INotifyPropertyChanged
    {
        private string referencedField;

        [DataMember]
        [XmlElement(DataType = "IDREF")]
        public string Referenced
        {
            get
            {
                return referencedField;
            }
            set
            {
                if (referencedField != null)
                {
                    if (!referencedField.Equals(value))
                    {
                        referencedField = value;
                        OnPropertyChanged("Referenced");
                    }
                }
                else
                {
                    referencedField = value;
                    OnPropertyChanged("Referenced");
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
