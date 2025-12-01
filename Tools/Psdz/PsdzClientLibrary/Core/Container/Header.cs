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
    [DataContract(Name = "Header")]
    public class Header : INotifyPropertyChanged
    {
        private byte[] tagField;
        private HeaderVersion versionField;
        private HeaderAdapter adapterField;
        [XmlElement(DataType = "base64Binary")]
        [DataMember]
        public byte[] Tag
        {
            get
            {
                return tagField;
            }

            set
            {
                if (tagField != null)
                {
                    if (!tagField.Equals(value))
                    {
                        tagField = value;
                        OnPropertyChanged("Tag");
                    }
                }
                else
                {
                    tagField = value;
                    OnPropertyChanged("Tag");
                }
            }
        }

        [DataMember]
        public HeaderVersion Version
        {
            get
            {
                return versionField;
            }

            set
            {
                if (versionField != null)
                {
                    if (!versionField.Equals(value))
                    {
                        versionField = value;
                        OnPropertyChanged("Version");
                    }
                }
                else
                {
                    versionField = value;
                    OnPropertyChanged("Version");
                }
            }
        }

        [DataMember]
        public HeaderAdapter Adapter
        {
            get
            {
                return adapterField;
            }

            set
            {
                if (adapterField != null)
                {
                    if (!adapterField.Equals(value))
                    {
                        adapterField = value;
                        OnPropertyChanged("Adapter");
                    }
                }
                else
                {
                    adapterField = value;
                    OnPropertyChanged("Adapter");
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