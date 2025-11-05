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
    [DataContract(Name = "ClassReference")]
    public class ClassReference : INotifyPropertyChanged
    {
        private string fullClassNameField;
        private string locationField;
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
        [DataMember]
        public string Location
        {
            get
            {
                return locationField;
            }

            set
            {
                if (locationField != null)
                {
                    if (!locationField.Equals(value))
                    {
                        locationField = value;
                        OnPropertyChanged("Location");
                    }
                }
                else
                {
                    locationField = value;
                    OnPropertyChanged("Location");
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