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
    [DataContract(Name = "Address", Namespace = "http://www.bmw.com/ibase/beans/dealerdata")]
    public class Address : INotifyPropertyChanged, IAddress
    {
        private string street1Field;
        private string street2Field;
        private string postalCodeField;
        private string town1Field;
        private string town2Field;
        private string countryField;
        private static XmlSerializer serializer;
        [XmlAttribute]
        [DataMember]
        public string street1
        {
            get
            {
                return street1Field;
            }

            set
            {
                if (street1Field != null)
                {
                    if (!street1Field.Equals(value))
                    {
                        street1Field = value;
                        OnPropertyChanged("street1");
                    }
                }
                else
                {
                    street1Field = value;
                    OnPropertyChanged("street1");
                }
            }
        }

        [XmlAttribute]
        [DataMember]
        public string street2
        {
            get
            {
                return street2Field;
            }

            set
            {
                if (street2Field != null)
                {
                    if (!street2Field.Equals(value))
                    {
                        street2Field = value;
                        OnPropertyChanged("street2");
                    }
                }
                else
                {
                    street2Field = value;
                    OnPropertyChanged("street2");
                }
            }
        }

        [XmlAttribute]
        [DataMember]
        public string postalCode
        {
            get
            {
                return postalCodeField;
            }

            set
            {
                if (postalCodeField != null)
                {
                    if (!postalCodeField.Equals(value))
                    {
                        postalCodeField = value;
                        OnPropertyChanged("postalCode");
                    }
                }
                else
                {
                    postalCodeField = value;
                    OnPropertyChanged("postalCode");
                }
            }
        }

        [XmlAttribute]
        [DataMember]
        public string town1
        {
            get
            {
                return town1Field;
            }

            set
            {
                if (town1Field != null)
                {
                    if (!town1Field.Equals(value))
                    {
                        town1Field = value;
                        OnPropertyChanged("town1");
                    }
                }
                else
                {
                    town1Field = value;
                    OnPropertyChanged("town1");
                }
            }
        }

        [XmlAttribute]
        [DataMember]
        public string town2
        {
            get
            {
                return town2Field;
            }

            set
            {
                if (town2Field != null)
                {
                    if (!town2Field.Equals(value))
                    {
                        town2Field = value;
                        OnPropertyChanged("town2");
                    }
                }
                else
                {
                    town2Field = value;
                    OnPropertyChanged("town2");
                }
            }
        }

        [XmlAttribute]
        [DataMember]
        public string country
        {
            get
            {
                return countryField;
            }

            set
            {
                if (countryField != null)
                {
                    if (!countryField.Equals(value))
                    {
                        countryField = value;
                        OnPropertyChanged("country");
                    }
                }
                else
                {
                    countryField = value;
                    OnPropertyChanged("country");
                }
            }
        }

        private static XmlSerializer Serializer
        {
            get
            {
                if (serializer == null)
                {
                    serializer = new XmlSerializer(typeof(Address));
                }

                return serializer;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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

        public static bool Deserialize(string xml, out Address obj, out Exception exception)
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

        public static bool Deserialize(string xml, out Address obj)
        {
            Exception exception = null;
            return Deserialize(xml, out obj, out exception);
        }

        public static Address Deserialize(string xml)
        {
            StringReader stringReader = null;
            try
            {
                stringReader = new StringReader(xml);
                return (Address)Serializer.Deserialize(XmlReader.Create(stringReader));
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

        public static bool LoadFromFile(string fileName, out Address obj, out Exception exception)
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

        public static bool LoadFromFile(string fileName, out Address obj)
        {
            Exception exception = null;
            return LoadFromFile(fileName, out obj, out exception);
        }

        public static Address LoadFromFile(string fileName)
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