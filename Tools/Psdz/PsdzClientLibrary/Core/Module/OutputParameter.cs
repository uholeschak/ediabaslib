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
    [DataContract(Name = "OutputParameter")]
    [PreserveSource(Hint = "GenericNamedElement removed", InheritanceModified = true)]
    public class OutputParameter : INotifyPropertyChanged
    {
        private string typeField;
        private string friendlyNameField;
        private bool lockedField;
        private bool lockedFieldSpecified;
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

        public new event PropertyChangedEventHandler PropertyChanged;
        public new virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}