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
    [DataContract(Name = "QuantityChoice")]
    public class QuantityChoice : AChoice, INotifyPropertyChanged
    {
        private uint minimumField;
        private uint maximumField;
        [XmlAttribute]
        [DefaultValue(typeof(uint), "1")]
        [DataMember]
        public uint Minimum
        {
            get
            {
                return minimumField;
            }

            set
            {
                _ = minimumField;
                if (!minimumField.Equals(value))
                {
                    minimumField = value;
                    OnPropertyChanged("Minimum");
                }
            }
        }

        [XmlAttribute]
        [DataMember]
        public uint Maximum
        {
            get
            {
                return maximumField;
            }

            set
            {
                _ = maximumField;
                if (!maximumField.Equals(value))
                {
                    maximumField = value;
                    OnPropertyChanged("Maximum");
                }
            }
        }

        public new event PropertyChangedEventHandler PropertyChanged;
        public QuantityChoice()
        {
            minimumField = 1u;
        }

        public new virtual void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}