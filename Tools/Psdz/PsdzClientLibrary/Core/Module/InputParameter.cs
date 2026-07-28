using PsdzClient;
using System;
using System.CodeDom.Compiler;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Xml.Schema;
using System.Xml.Serialization;

#pragma warning disable CS0109
namespace BMW.Rheingold.Module.ISTA
{
    [Serializable]
    [GeneratedCode("Xsd2Code", "3.4.0.26539")]
    [DesignerCategory("code")]
    [XmlRoot(Namespace = "", IsNullable = true)]
    [DataContract(Name = "InputParameter")]
    [PreserveSource(Hint = "GenericNamedElement removed", InheritanceModified = true)]
    public class InputParameter : INotifyPropertyChanged
    {
        private string defaultValueField;
        private ObservableCollection<RangeValue> valueRangeListField;
        private string typeField;
        private string friendlyNameField;
        private bool lockedField;
        private bool lockedFieldSpecified;
        private bool mandatoryField;
        private bool mandatoryFieldSpecified;
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        [DataMember]
        public string DefaultValue
        {
            get
            {
                return defaultValueField;
            }

            set
            {
                if (defaultValueField != null)
                {
                    if (!defaultValueField.Equals(value))
                    {
                        defaultValueField = value;
                        OnPropertyChanged("DefaultValue");
                    }
                }
                else
                {
                    defaultValueField = value;
                    OnPropertyChanged("DefaultValue");
                }
            }
        }

        [XmlArray(Form = XmlSchemaForm.Unqualified)]
        [XmlArrayItem(Form = XmlSchemaForm.Unqualified, IsNullable = false)]
        [DataMember]
        public ObservableCollection<RangeValue> ValueRangeList
        {
            get
            {
                return valueRangeListField;
            }

            set
            {
                if (valueRangeListField != null)
                {
                    if (!valueRangeListField.Equals(value))
                    {
                        valueRangeListField = value;
                        OnPropertyChanged("ValueRangeList");
                    }
                }
                else
                {
                    valueRangeListField = value;
                    OnPropertyChanged("ValueRangeList");
                }
            }
        }

        [XmlAttribute]
        [DataMember]
        public string Type
        {
            get
            {
                return typeField;
            }

            set
            {
                if (typeField != null)
                {
                    if (!typeField.Equals(value))
                    {
                        typeField = value;
                        OnPropertyChanged("Type");
                    }
                }
                else
                {
                    typeField = value;
                    OnPropertyChanged("Type");
                }
            }
        }

        [XmlAttribute]
        [DataMember]
        public string FriendlyName
        {
            get
            {
                return friendlyNameField;
            }

            set
            {
                if (friendlyNameField != null)
                {
                    if (!friendlyNameField.Equals(value))
                    {
                        friendlyNameField = value;
                        OnPropertyChanged("FriendlyName");
                    }
                }
                else
                {
                    friendlyNameField = value;
                    OnPropertyChanged("FriendlyName");
                }
            }
        }

        [XmlAttribute]
        [DataMember]
        public bool Locked
        {
            get
            {
                return lockedField;
            }

            set
            {
                if (!lockedField.Equals(value))
                {
                    lockedField = value;
                    OnPropertyChanged("Locked");
                }
            }
        }

        [XmlIgnore]
        [DataMember]
        public bool LockedSpecified
        {
            get
            {
                return lockedFieldSpecified;
            }

            set
            {
                if (!lockedFieldSpecified.Equals(value))
                {
                    lockedFieldSpecified = value;
                    OnPropertyChanged("LockedSpecified");
                }
            }
        }

        [XmlAttribute]
        [DataMember]
        public bool Mandatory
        {
            get
            {
                return mandatoryField;
            }

            set
            {
                if (!mandatoryField.Equals(value))
                {
                    mandatoryField = value;
                    OnPropertyChanged("Mandatory");
                }
            }
        }

        [XmlIgnore]
        [DataMember]
        public bool MandatorySpecified
        {
            get
            {
                return mandatoryFieldSpecified;
            }

            set
            {
                if (!mandatoryFieldSpecified.Equals(value))
                {
                    mandatoryFieldSpecified = value;
                    OnPropertyChanged("MandatorySpecified");
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