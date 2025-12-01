using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Serialization;

namespace PsdzClient.Core.Container
{
    [Serializable]
    [GeneratedCode("Xsd2Code", "3.4.0.32990")]
    [DesignerCategory("code")]
    [DataContract(Name = "Configuration")]
    public class Configuration : INotifyPropertyChanged
    {
        private ABranch parametrizationField;
        private ABranch runField;
        private string nameField;
        private string originField;
        private string relatedConfigurationField;
        private long internalNodeIdGeneratorField;
        private bool internalNodeIdGeneratorFieldSpecified;
        [DataMember]
        public ABranch Parametrization
        {
            get
            {
                return parametrizationField;
            }

            set
            {
                if (parametrizationField != null)
                {
                    if (!parametrizationField.Equals(value))
                    {
                        parametrizationField = value;
                        OnPropertyChanged("Parametrization");
                    }
                }
                else
                {
                    parametrizationField = value;
                    OnPropertyChanged("Parametrization");
                }
            }
        }

        [DataMember]
        public ABranch Run
        {
            get
            {
                return runField;
            }

            set
            {
                if (runField != null)
                {
                    if (!runField.Equals(value))
                    {
                        runField = value;
                        OnPropertyChanged("Run");
                    }
                }
                else
                {
                    runField = value;
                    OnPropertyChanged("Run");
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
        [DataMember]
        public string Origin
        {
            get
            {
                return originField;
            }

            set
            {
                if (originField != null)
                {
                    if (!originField.Equals(value))
                    {
                        originField = value;
                        OnPropertyChanged("Origin");
                    }
                }
                else
                {
                    originField = value;
                    OnPropertyChanged("Origin");
                }
            }
        }

        [XmlAttribute]
        [DataMember]
        public string RelatedConfiguration
        {
            get
            {
                return relatedConfigurationField;
            }

            set
            {
                if (relatedConfigurationField != null)
                {
                    if (!relatedConfigurationField.Equals(value))
                    {
                        relatedConfigurationField = value;
                        OnPropertyChanged("RelatedConfiguration");
                    }
                }
                else
                {
                    relatedConfigurationField = value;
                    OnPropertyChanged("RelatedConfiguration");
                }
            }
        }

        [XmlAttribute]
        [DataMember]
        public long InternalNodeIdGenerator
        {
            get
            {
                return internalNodeIdGeneratorField;
            }

            set
            {
                if (!internalNodeIdGeneratorField.Equals(value))
                {
                    internalNodeIdGeneratorField = value;
                    OnPropertyChanged("InternalNodeIdGenerator");
                }
            }
        }

        [XmlIgnore]
        [DataMember]
        public bool InternalNodeIdGeneratorSpecified
        {
            get
            {
                return internalNodeIdGeneratorFieldSpecified;
            }

            set
            {
                if (!internalNodeIdGeneratorFieldSpecified.Equals(value))
                {
                    internalNodeIdGeneratorFieldSpecified = value;
                    OnPropertyChanged("InternalNodeIdGeneratorSpecified");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public virtual void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public static Configuration Deserialize(string configuration)
        {
            try
            {
                StringReader input = new StringReader(configuration);
                return (Configuration)new XmlSerializer(typeof(Configuration)).Deserialize(XmlReader.Create(input));
            }
            catch (Exception exception)
            {
                Log.WarningException("Configuration.Deserialize()", exception);
            }

            return null;
        }
    }
}