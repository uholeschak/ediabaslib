using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.IO;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Serialization;

namespace PsdzClient.Core.Container
{
    [Serializable]
    [GeneratedCode("Xsd2Code", "3.4.0.32990")]
    [DesignerCategory("code")]
    [XmlRoot(Namespace = "", IsNullable = false)]
    [DataContract(Name = "ConfigurationContainer")]
    public class ConfigurationContainer : INotifyPropertyChanged
    {
        private Header headerField;
        private Body bodyField;
        private string nameField;
        private CompressionMethod compressionField;
        private long majorVersionField;
        private long minorVersionField;
        private ParameterContainer parametrizationOverrides = new ParameterContainer();
        private ParameterContainer runOverrides = new ParameterContainer();
        [DataMember]
        public Header Header
        {
            get
            {
                return headerField;
            }

            set
            {
                if (headerField != null)
                {
                    if (!headerField.Equals(value))
                    {
                        headerField = value;
                        OnPropertyChanged("Header");
                    }
                }
                else
                {
                    headerField = value;
                    OnPropertyChanged("Header");
                }
            }
        }

        [DataMember]
        public Body Body
        {
            get
            {
                return bodyField;
            }

            set
            {
                if (bodyField != null)
                {
                    if (!bodyField.Equals(value))
                    {
                        bodyField = value;
                        OnPropertyChanged("Body");
                    }
                }
                else
                {
                    bodyField = value;
                    OnPropertyChanged("Body");
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
        [DefaultValue(CompressionMethod.Undefined)]
        [DataMember]
        public CompressionMethod Compression
        {
            get
            {
                return compressionField;
            }

            set
            {
                if (!compressionField.Equals(value))
                {
                    compressionField = value;
                    OnPropertyChanged("Compression");
                }
            }
        }

        [XmlAttribute]
        [DataMember]
        public long MajorVersion
        {
            get
            {
                return majorVersionField;
            }

            set
            {
                if (!majorVersionField.Equals(value))
                {
                    majorVersionField = value;
                    OnPropertyChanged("MajorVersion");
                }
            }
        }

        [XmlAttribute]
        [DataMember]
        public long MinorVersion
        {
            get
            {
                return minorVersionField;
            }

            set
            {
                if (!minorVersionField.Equals(value))
                {
                    minorVersionField = value;
                    OnPropertyChanged("MinorVersion");
                }
            }
        }

        public ParameterContainer ParametrizationOverrides => parametrizationOverrides;
        public ParameterContainer RunOverrides => runOverrides;

        public event PropertyChangedEventHandler PropertyChanged;
        public ConfigurationContainer()
        {
            compressionField = CompressionMethod.Undefined;
            majorVersionField = 1L;
            minorVersionField = 0L;
        }

        public virtual void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public static ConfigurationContainer Deserialize(string configurationContainer)
        {
            try
            {
                StringReader input = new StringReader(configurationContainer);
                return (ConfigurationContainer)new XmlSerializer(typeof(ConfigurationContainer)).Deserialize(XmlReader.Create(input));
            }
            catch (Exception exception)
            {
                Log.WarningException("ConfigurationContainer.Deserialize()", exception);
            }

            return null;
        }

        public void AddParametrizationOverride(string path, object value)
        {
            parametrizationOverrides.setParameter(path, value);
        }

        public void AddRunOverride(string path, object value)
        {
            runOverrides.setParameter(path, value);
        }
    }
}