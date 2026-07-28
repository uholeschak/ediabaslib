using PsdzClient;
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Xml.Serialization;

#pragma warning disable CS0109
namespace BMW.Rheingold.Module.ISTA
{
    [Serializable]
    [GeneratedCode("Xsd2Code", "3.4.0.26539")]
    [DesignerCategory("code")]
    [XmlRoot(Namespace = "", IsNullable = true)]
    [DataContract(Name = "Statement")]
    [PreserveSource(Hint = "GenericElement removed", InheritanceModified = true)]
    public abstract class Statement : INotifyPropertyChanged
    {
        private bool activeField;
        private bool protocolField;
        [XmlAttribute]
        [DefaultValue(true)]
        [DataMember]
        public bool Active
        {
            get
            {
                return activeField;
            }

            set
            {
                if (!activeField.Equals(value))
                {
                    activeField = value;
                    OnPropertyChanged("Active");
                }
            }
        }

        [XmlAttribute]
        [DataMember]
        public bool Protocol
        {
            get
            {
                return protocolField;
            }

            set
            {
                if (!protocolField.Equals(value))
                {
                    protocolField = value;
                    OnPropertyChanged("Protocol");
                }
            }
        }

        public new event PropertyChangedEventHandler PropertyChanged;
        public Statement()
        {
            activeField = true;
            protocolField = true;
        }

        public new virtual void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}