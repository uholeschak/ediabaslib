using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace BMW.Rheingold.CoreFramework.DatabaseProvider
{
    [Serializable]
    [GeneratedCode("Xsd2Code", "3.4.0.38968")]
    [DesignerCategory("code")]
    [XmlType(Namespace = "http://www.bmw.com/ibase/beans/dealerdata")]
    [XmlRoot(Namespace = "http://www.bmw.com/ibase/beans/dealerdata", IsNullable = true)]
    [DataContract(Name = "Communication", Namespace = "http://www.bmw.com/ibase/beans/dealerdata")]
    public class Communication : INotifyPropertyChanged
    {
        private Phone voiceField;

        private Phone faxField;

        private string emailField;

        private string urlField;

        private static XmlSerializer serializer;

        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        [DataMember]
        public Phone voice
        {
            get
            {
                return voiceField;
            }
            set
            {
                if (voiceField != null)
                {
                    if (!voiceField.Equals(value))
                    {
                        voiceField = value;
                        OnPropertyChanged("voice");
                    }
                }
                else
                {
                    voiceField = value;
                    OnPropertyChanged("voice");
                }
            }
        }

        [XmlElement(Form = XmlSchemaForm.Unqualified, IsNullable = true)]
        [DataMember]
        public Phone fax
        {
            get
            {
                return faxField;
            }
            set
            {
                if (faxField != null)
                {
                    if (!faxField.Equals(value))
                    {
                        faxField = value;
                        OnPropertyChanged("fax");
                    }
                }
                else
                {
                    faxField = value;
                    OnPropertyChanged("fax");
                }
            }
        }

        [XmlAttribute]
        [DataMember]
        public string email
        {
            get
            {
                return emailField;
            }
            set
            {
                if (emailField != null)
                {
                    if (!emailField.Equals(value))
                    {
                        emailField = value;
                        OnPropertyChanged("email");
                    }
                }
                else
                {
                    emailField = value;
                    OnPropertyChanged("email");
                }
            }
        }

        [XmlAttribute(DataType = "anyURI")]
        [DataMember]
        public string url
        {
            get
            {
                return urlField;
            }
            set
            {
                if (urlField != null)
                {
                    if (!urlField.Equals(value))
                    {
                        urlField = value;
                        OnPropertyChanged("url");
                    }
                }
                else
                {
                    urlField = value;
                    OnPropertyChanged("url");
                }
            }
        }

        private static XmlSerializer Serializer
        {
            get
            {
                if (serializer == null)
                {
                    serializer = new XmlSerializer(typeof(Communication));
                }
                return serializer;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public Communication()
        {
            faxField = new Phone();
            voiceField = new Phone();
        }

        public virtual void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public virtual string Serialize()
        {
            StreamReader streamReader = null;
            MemoryStream memoryStream = null;
            try
            {
                memoryStream = new MemoryStream();
                Serializer.Serialize(memoryStream, this);
                memoryStream.Seek(0L, SeekOrigin.Begin);
                streamReader = new StreamReader(memoryStream);
                return streamReader.ReadToEnd();
            }
            finally
            {
                streamReader?.Dispose();
                memoryStream?.Dispose();
            }
        }

        public static bool Deserialize(string xml, out Communication obj, out Exception exception)
        {
            exception = null;
            obj = null;
            try
            {
                obj = Deserialize(xml);
                return true;
            }
            catch (Exception ex)
            {
                exception = ex;
                return false;
            }
        }

        public static bool Deserialize(string xml, out Communication obj)
        {
            Exception exception = null;
            return Deserialize(xml, out obj, out exception);
        }

        public static Communication Deserialize(string xml)
        {
            StringReader stringReader = null;
            try
            {
                stringReader = new StringReader(xml);
                return (Communication)Serializer.Deserialize(XmlReader.Create(stringReader));
            }
            finally
            {
                stringReader?.Dispose();
            }
        }

        public virtual bool SaveToFile(string fileName, out Exception exception)
        {
            exception = null;
            try
            {
                SaveToFile(fileName);
                return true;
            }
            catch (Exception ex)
            {
                exception = ex;
                return false;
            }
        }

        public virtual void SaveToFile(string fileName)
        {
            StreamWriter streamWriter = null;
            try
            {
                string value = Serialize();
                streamWriter = new FileInfo(fileName).CreateText();
                streamWriter.WriteLine(value);
                streamWriter.Close();
            }
            finally
            {
                streamWriter?.Dispose();
            }
        }

        public static bool LoadFromFile(string fileName, out Communication obj, out Exception exception)
        {
            exception = null;
            obj = null;
            try
            {
                obj = LoadFromFile(fileName);
                return true;
            }
            catch (Exception ex)
            {
                exception = ex;
                return false;
            }
        }

        public static bool LoadFromFile(string fileName, out Communication obj)
        {
            Exception exception = null;
            return LoadFromFile(fileName, out obj, out exception);
        }

        public static Communication LoadFromFile(string fileName)
        {
            FileStream fileStream = null;
            StreamReader streamReader = null;
            try
            {
                fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                streamReader = new StreamReader(fileStream);
                string xml = streamReader.ReadToEnd();
                streamReader.Close();
                fileStream.Close();
                return Deserialize(xml);
            }
            finally
            {
                fileStream?.Dispose();
                streamReader?.Dispose();
            }
        }
    }
}
