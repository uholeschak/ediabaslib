using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace PsdzClient.Core.Container
{
    [Serializable]
    [GeneratedCode("Xsd2Code", "3.4.0.32990")]
    [DesignerCategory("code")]
    [DataContract(Name = "HeaderVersion")]
    public class HeaderVersion : INotifyPropertyChanged
    {
        private long majorField;
        private long minorField;
        [XmlAttribute]
        [DataMember]
        public long Major
        {
            get
            {
                return majorField;
            }

            set
            {
                if (!majorField.Equals(value))
                {
                    majorField = value;
                    OnPropertyChanged("Major");
                }
            }
        }

        [XmlAttribute]
        [DataMember]
        public long Minor
        {
            get
            {
                return minorField;
            }

            set
            {
                if (!minorField.Equals(value))
                {
                    minorField = value;
                    OnPropertyChanged("Minor");
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