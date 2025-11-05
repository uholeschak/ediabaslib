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
    [DataContract(Name = "SubDevice")]
    public class SubDevice : INotifyPropertyChanged
    {
        private string nameField;
        private string fullClassNameField;
        private NoDeviceBehaviorForHeader noDeviceBehaviorField;
        [XmlAttribute]
        [DataMember]
        public string Name
        {
            get
            {
                return nameField;
            }

            set
            {
                if (nameField != null)
                {
                    if (!nameField.Equals(value))
                    {
                        nameField = value;
                        OnPropertyChanged("Name");
                    }
                }
                else
                {
                    nameField = value;
                    OnPropertyChanged("Name");
                }
            }
        }

        [XmlAttribute]
        [DataMember]
        public string FullClassName
        {
            get
            {
                return fullClassNameField;
            }

            set
            {
                if (fullClassNameField != null)
                {
                    if (!fullClassNameField.Equals(value))
                    {
                        fullClassNameField = value;
                        OnPropertyChanged("FullClassName");
                    }
                }
                else
                {
                    fullClassNameField = value;
                    OnPropertyChanged("FullClassName");
                }
            }
        }

        [XmlAttribute]
        [DefaultValue(NoDeviceBehaviorForHeader.SystemDefault)]
        [DataMember]
        public NoDeviceBehaviorForHeader NoDeviceBehavior
        {
            get
            {
                return noDeviceBehaviorField;
            }

            set
            {
                if (!noDeviceBehaviorField.Equals(value))
                {
                    noDeviceBehaviorField = value;
                    OnPropertyChanged("NoDeviceBehavior");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public SubDevice()
        {
            noDeviceBehaviorField = NoDeviceBehaviorForHeader.SystemDefault;
        }

        public virtual void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}