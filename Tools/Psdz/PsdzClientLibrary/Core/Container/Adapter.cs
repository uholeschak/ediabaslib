using System;
using System.CodeDom.Compiler;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace PsdzClient.Core.Container
{
    [Serializable]
    [GeneratedCode("Xsd2Code", "3.4.0.32990")]
    public enum NoDeviceBehaviorForHeader
    {
        SystemDefault,
        SubstitutionValueInput
    }

    [Serializable]
    [GeneratedCode("Xsd2Code", "3.4.0.32990")]
    [DesignerCategory("code")]
    [DataContract(Name = "Adapter")]
    public class Adapter : INotifyPropertyChanged
    {
        private ClassReference classReferenceField;
        private ObservableCollection<SubDeviceCollectionSubDevice> subDeviceCollectionField;
        private string nameField;
        private NoDeviceBehaviorForHeader noDeviceBehaviorField;
        [DataMember]
        public ClassReference ClassReference
        {
            get
            {
                return classReferenceField;
            }

            set
            {
                if (classReferenceField != null)
                {
                    if (!classReferenceField.Equals(value))
                    {
                        classReferenceField = value;
                        OnPropertyChanged("ClassReference");
                    }
                }
                else
                {
                    classReferenceField = value;
                    OnPropertyChanged("ClassReference");
                }
            }
        }

        [XmlArrayItem("SubDevice", IsNullable = false)]
        [DataMember]
        public ObservableCollection<SubDeviceCollectionSubDevice> SubDeviceCollection
        {
            get
            {
                return subDeviceCollectionField;
            }

            set
            {
                if (subDeviceCollectionField != null)
                {
                    if (!subDeviceCollectionField.Equals(value))
                    {
                        subDeviceCollectionField = value;
                        OnPropertyChanged("SubDeviceCollection");
                    }
                }
                else
                {
                    subDeviceCollectionField = value;
                    OnPropertyChanged("SubDeviceCollection");
                }
            }
        }

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
        public Adapter()
        {
            noDeviceBehaviorField = NoDeviceBehaviorForHeader.SystemDefault;
        }

        public virtual void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}