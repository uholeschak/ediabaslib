using System;
using System.CodeDom.Compiler;
using System.Collections.ObjectModel;
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
    [DataContract(Name = "Outlet", Namespace = "http://www.bmw.com/ibase/beans/dealerdata")]
    public class Outlet : INotifyPropertyChanged
    {
        private Address addressField;
        private Communication contactField;
        private BusinessRelationship businessRelationshipField;
        private ObservableCollection<string> marketLanguageField;
        private ObservableCollection<Contract> contractField;
        private string outletNumberField;
        private string nameField;
        private bool protectionVehicleServiceField;
        private static XmlSerializer serializer;
        [XmlElement(Form = XmlSchemaForm.Unqualified, Order = 0)]
        [DataMember]
        public Address address
        {
            get
            {
                return addressField;
            }

            set
            {
                if (addressField != null)
                {
                    if (!addressField.Equals(value))
                    {
                        addressField = value;
                        OnPropertyChanged("address");
                    }
                }
                else
                {
                    addressField = value;
                    OnPropertyChanged("address");
                }
            }
        }

        [XmlElement(Form = XmlSchemaForm.Unqualified, Order = 1)]
        [DataMember]
        public Communication contact
        {
            get
            {
                return contactField;
            }

            set
            {
                if (contactField != null)
                {
                    if (!contactField.Equals(value))
                    {
                        contactField = value;
                        OnPropertyChanged("contact");
                    }
                }
                else
                {
                    contactField = value;
                    OnPropertyChanged("contact");
                }
            }
        }

        [XmlElement(Form = XmlSchemaForm.Unqualified, Order = 2)]
        [DataMember]
        public BusinessRelationship businessRelationship
        {
            get
            {
                return businessRelationshipField;
            }

            set
            {
                if (!businessRelationshipField.Equals(value))
                {
                    businessRelationshipField = value;
                    OnPropertyChanged("businessRelationship");
                }
            }
        }

        [XmlElement("marketLanguage", Form = XmlSchemaForm.Unqualified, DataType = "language", Order = 3)]
        [DataMember]
        public ObservableCollection<string> marketLanguage
        {
            get
            {
                return marketLanguageField;
            }

            set
            {
                if (marketLanguageField != null)
                {
                    if (!marketLanguageField.Equals(value))
                    {
                        marketLanguageField = value;
                        OnPropertyChanged("marketLanguage");
                    }
                }
                else
                {
                    marketLanguageField = value;
                    OnPropertyChanged("marketLanguage");
                }
            }
        }

        [XmlElement("contract", Form = XmlSchemaForm.Unqualified, Order = 4)]
        [DataMember]
        public ObservableCollection<Contract> contract
        {
            get
            {
                return contractField;
            }

            set
            {
                if (contractField != null)
                {
                    if (!contractField.Equals(value))
                    {
                        contractField = value;
                        OnPropertyChanged("contract");
                    }
                }
                else
                {
                    contractField = value;
                    OnPropertyChanged("contract");
                }
            }
        }

        [XmlAttribute(DataType = "integer")]
        [DataMember]
        public string outletNumber
        {
            get
            {
                return outletNumberField;
            }

            set
            {
                if (outletNumberField != null)
                {
                    if (!outletNumberField.Equals(value))
                    {
                        outletNumberField = value;
                        OnPropertyChanged("outletNumber");
                    }
                }
                else
                {
                    outletNumberField = value;
                    OnPropertyChanged("outletNumber");
                }
            }
        }

        [XmlAttribute]
        [DataMember]
        public string name
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
                        OnPropertyChanged("name");
                    }
                }
                else
                {
                    nameField = value;
                    OnPropertyChanged("name");
                }
            }
        }

        [XmlAttribute]
        [DataMember]
        public bool protectionVehicleService
        {
            get
            {
                return protectionVehicleServiceField;
            }

            set
            {
                if (!protectionVehicleServiceField.Equals(value))
                {
                    protectionVehicleServiceField = value;
                    OnPropertyChanged("protectionVehicleService");
                }
            }
        }

        private static XmlSerializer Serializer
        {
            get
            {
                if (serializer == null)
                {
                    serializer = new XmlSerializer(typeof(Outlet));
                }

                return serializer;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public Outlet()
        {
            contractField = new ObservableCollection<Contract>();
            marketLanguageField = new ObservableCollection<string>();
            contactField = new Communication();
            addressField = new Address();
        }

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

        public static bool Deserialize(string xml, out Outlet obj, out Exception exception)
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

        public static bool Deserialize(string xml, out Outlet obj)
        {
            Exception exception = null;
            return Deserialize(xml, out obj, out exception);
        }

        public static Outlet Deserialize(string xml)
        {
            StringReader stringReader = null;
            try
            {
                stringReader = new StringReader(xml);
                return (Outlet)Serializer.Deserialize(XmlReader.Create(stringReader));
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

        public static bool LoadFromFile(string fileName, out Outlet obj, out Exception exception)
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

        public static bool LoadFromFile(string fileName, out Outlet obj)
        {
            Exception exception = null;
            return LoadFromFile(fileName, out obj, out exception);
        }

        public static Outlet LoadFromFile(string fileName)
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