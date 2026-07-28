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
    [DataContract(Name = "Exit")]
    public class Exit : INotifyPropertyChanged
    {
        private int indexField;

        private string nextTestStepNameField;

        [XmlAttribute]
        [DataMember]
        public int Index
        {
            get
            {
                return indexField;
            }
            set
            {
                if (!indexField.Equals(value))
                {
                    indexField = value;
                    OnPropertyChanged("Index");
                }
            }
        }

        [XmlAttribute(DataType = "NCName")]
        [DataMember]
        public string NextTestStepName
        {
            get
            {
                return nextTestStepNameField;
            }
            set
            {
                if (nextTestStepNameField != null)
                {
                    if (!nextTestStepNameField.Equals(value))
                    {
                        nextTestStepNameField = value;
                        OnPropertyChanged("NextTestStepName");
                    }
                }
                else
                {
                    nextTestStepNameField = value;
                    OnPropertyChanged("NextTestStepName");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public virtual void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
