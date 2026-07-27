using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Serialization;

namespace BMW.Rheingold.CoreFramework.DatabaseProvider
{
    [Serializable]
    [GeneratedCode("Xsd2Code", "3.4.0.38968")]
    [DesignerCategory("code")]
    [XmlType(Namespace = "http://www.bmw.com/ibase/beans/dealerdata")]
    [XmlRoot(Namespace = "http://www.bmw.com/ibase/beans/dealerdata", IsNullable = true)]
    [DataContract(Name = "Phone", Namespace = "http://www.bmw.com/ibase/beans/dealerdata")]
    public class Phone : INotifyPropertyChanged
    {
        private string countryCodeField;

        private string areaCodeField;

        private string localNumberField;

        private static XmlSerializer serializer;

        [XmlAttribute]
        [DataMember]
        public string countryCode
        {
            get
            {
                return countryCodeField;
            }
            set
            {
                if (countryCodeField != null)
                {
                    if (!countryCodeField.Equals(value))
                    {
                        countryCodeField = value;
                        OnPropertyChanged("countryCode");
                    }
                }
                else
                {
                    countryCodeField = value;
                    OnPropertyChanged("countryCode");
                }
            }
        }

        [XmlAttribute]
        [DataMember]
        public string areaCode
        {
            get
            {
                return areaCodeField;
            }
            set
            {
                if (areaCodeField != null)
                {
                    if (!areaCodeField.Equals(value))
                    {
                        areaCodeField = value;
                        OnPropertyChanged("areaCode");
                    }
                }
                else
                {
                    areaCodeField = value;
                    OnPropertyChanged("areaCode");
                }
            }
        }

        [XmlAttribute]
        [DataMember]
        public string localNumber
        {
            get
            {
                return localNumberField;
            }
            set
            {
                if (localNumberField != null)
                {
                    if (!localNumberField.Equals(value))
                    {
                        localNumberField = value;
                        OnPropertyChanged("localNumber");
                    }
                }
                else
                {
                    localNumberField = value;
                    OnPropertyChanged("localNumber");
                }
            }
        }

        private static XmlSerializer Serializer
        {
            get
            {
                if (serializer == null)
                {
                    serializer = new XmlSerializer(typeof(Phone));
                }
                return serializer;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

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

        public static bool Deserialize(string xml, out Phone obj, out Exception exception)
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

        public static bool Deserialize(string xml, out Phone obj)
        {
            Exception exception = null;
            return Deserialize(xml, out obj, out exception);
        }

        public static Phone Deserialize(string xml)
        {
            StringReader stringReader = null;
            try
            {
                stringReader = new StringReader(xml);
                return (Phone)Serializer.Deserialize(XmlReader.Create(stringReader));
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

        public static bool LoadFromFile(string fileName, out Phone obj, out Exception exception)
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

        public static bool LoadFromFile(string fileName, out Phone obj)
        {
            Exception exception = null;
            return LoadFromFile(fileName, out obj, out exception);
        }

        public static Phone LoadFromFile(string fileName)
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
