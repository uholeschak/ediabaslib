using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Configuration;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace PsdzClient.Core.Container
{
    [Serializable]
    [GeneratedCode("Xsd2Code", "3.4.0.32990")]
    [DesignerCategory("code")]
    [DataContract(Name = "Body")]
    public class Body : INotifyPropertyChanged
    {
        private Database databaseField;
        private Configuration configurationField;
        private string textDictionaryConatinerFileNameField;
        private string textDictionaryNameField;
        [DataMember]
        public Database Database
        {
            get
            {
                return databaseField;
            }

            set
            {
                if (databaseField != null)
                {
                    if (!databaseField.Equals(value))
                    {
                        databaseField = value;
                        OnPropertyChanged("Database");
                    }
                }
                else
                {
                    databaseField = value;
                    OnPropertyChanged("Database");
                }
            }
        }

        [DataMember]
        public Configuration Configuration
        {
            get
            {
                return configurationField;
            }

            set
            {
                if (configurationField != null)
                {
                    if (!configurationField.Equals(value))
                    {
                        configurationField = value;
                        OnPropertyChanged("Configuration");
                    }
                }
                else
                {
                    configurationField = value;
                    OnPropertyChanged("Configuration");
                }
            }
        }

        [XmlAttribute]
        [DataMember]
        public string TextDictionaryConatinerFileName
        {
            get
            {
                return textDictionaryConatinerFileNameField;
            }

            set
            {
                if (textDictionaryConatinerFileNameField != null)
                {
                    if (!textDictionaryConatinerFileNameField.Equals(value))
                    {
                        textDictionaryConatinerFileNameField = value;
                        OnPropertyChanged("TextDictionaryConatinerFileName");
                    }
                }
                else
                {
                    textDictionaryConatinerFileNameField = value;
                    OnPropertyChanged("TextDictionaryConatinerFileName");
                }
            }
        }

        [XmlAttribute]
        [DataMember]
        public string TextDictionaryName
        {
            get
            {
                return textDictionaryNameField;
            }

            set
            {
                if (textDictionaryNameField != null)
                {
                    if (!textDictionaryNameField.Equals(value))
                    {
                        textDictionaryNameField = value;
                        OnPropertyChanged("TextDictionaryName");
                    }
                }
                else
                {
                    textDictionaryNameField = value;
                    OnPropertyChanged("TextDictionaryName");
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