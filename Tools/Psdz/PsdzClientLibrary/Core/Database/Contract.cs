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
    [DataContract(Name = "Contract", Namespace = "http://www.bmw.com/ibase/beans/dealerdata")]
    public class Contract : INotifyPropertyChanged
    {
        private Brand brandField;
        private Product productField;
        private BusinessLine businessLineField;
        private string salesBranchField;
        private string internationalDealerNumberField;
        private string nationalDealerNumberField;
        private DateTime startDateField;
        private DateTime endContractDateField;
        private bool endContractDateFieldSpecified;
        private DateTime endServiceDateField;
        private bool endServiceDateFieldSpecified;
        private bool mobileServiceField;
        private bool mobileServiceFieldSpecified;
        private static XmlSerializer serializer;
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        [DataMember]
        public Brand brand
        {
            get
            {
                return brandField;
            }

            set
            {
                if (!brandField.Equals(value))
                {
                    brandField = value;
                    OnPropertyChanged("brand");
                }
            }
        }

        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        [DataMember]
        public Product product
        {
            get
            {
                return productField;
            }

            set
            {
                if (!productField.Equals(value))
                {
                    productField = value;
                    OnPropertyChanged("product");
                }
            }
        }

        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        [DataMember]
        public BusinessLine businessLine
        {
            get
            {
                return businessLineField;
            }

            set
            {
                if (!businessLineField.Equals(value))
                {
                    businessLineField = value;
                    OnPropertyChanged("businessLine");
                }
            }
        }

        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        [DataMember]
        public string salesBranch
        {
            get
            {
                return salesBranchField;
            }

            set
            {
                if (salesBranchField != null)
                {
                    if (!salesBranchField.Equals(value))
                    {
                        salesBranchField = value;
                        OnPropertyChanged("salesBranch");
                    }
                }
                else
                {
                    salesBranchField = value;
                    OnPropertyChanged("salesBranch");
                }
            }
        }

        [XmlAttribute]
        [DataMember]
        public string internationalDealerNumber
        {
            get
            {
                return internationalDealerNumberField;
            }

            set
            {
                if (internationalDealerNumberField != null)
                {
                    if (!internationalDealerNumberField.Equals(value))
                    {
                        internationalDealerNumberField = value;
                        OnPropertyChanged("internationalDealerNumber");
                    }
                }
                else
                {
                    internationalDealerNumberField = value;
                    OnPropertyChanged("internationalDealerNumber");
                }
            }
        }

        [XmlAttribute]
        [DataMember]
        public string nationalDealerNumber
        {
            get
            {
                return nationalDealerNumberField;
            }

            set
            {
                if (nationalDealerNumberField != null)
                {
                    if (!nationalDealerNumberField.Equals(value))
                    {
                        nationalDealerNumberField = value;
                        OnPropertyChanged("nationalDealerNumber");
                    }
                }
                else
                {
                    nationalDealerNumberField = value;
                    OnPropertyChanged("nationalDealerNumber");
                }
            }
        }

        [XmlAttribute]
        [DataMember]
        public DateTime startDate
        {
            get
            {
                return startDateField;
            }

            set
            {
                if (!startDateField.Equals(value))
                {
                    startDateField = value;
                    OnPropertyChanged("startDate");
                }
            }
        }

        [XmlAttribute]
        [DataMember]
        public DateTime endContractDate
        {
            get
            {
                return endContractDateField;
            }

            set
            {
                if (!endContractDateField.Equals(value))
                {
                    endContractDateField = value;
                    OnPropertyChanged("endContractDate");
                }
            }
        }

        [XmlIgnore]
        [DataMember]
        public bool endContractDateSpecified
        {
            get
            {
                return endContractDateFieldSpecified;
            }

            set
            {
                if (!endContractDateFieldSpecified.Equals(value))
                {
                    endContractDateFieldSpecified = value;
                    OnPropertyChanged("endContractDateSpecified");
                }
            }
        }

        [XmlAttribute]
        [DataMember]
        public DateTime endServiceDate
        {
            get
            {
                return endServiceDateField;
            }

            set
            {
                if (!endServiceDateField.Equals(value))
                {
                    endServiceDateField = value;
                    OnPropertyChanged("endServiceDate");
                }
            }
        }

        [XmlIgnore]
        [DataMember]
        public bool endServiceDateSpecified
        {
            get
            {
                return endServiceDateFieldSpecified;
            }

            set
            {
                if (!endServiceDateFieldSpecified.Equals(value))
                {
                    endServiceDateFieldSpecified = value;
                    OnPropertyChanged("endServiceDateSpecified");
                }
            }
        }

        [XmlAttribute]
        [DataMember]
        public bool mobileService
        {
            get
            {
                return mobileServiceField;
            }

            set
            {
                if (!mobileServiceField.Equals(value))
                {
                    mobileServiceField = value;
                    OnPropertyChanged("mobileService");
                }
            }
        }

        [XmlIgnore]
        [DataMember]
        public bool mobileServiceSpecified
        {
            get
            {
                return mobileServiceFieldSpecified;
            }

            set
            {
                if (!mobileServiceFieldSpecified.Equals(value))
                {
                    mobileServiceFieldSpecified = value;
                    OnPropertyChanged("mobileServiceSpecified");
                }
            }
        }

        private static XmlSerializer Serializer
        {
            get
            {
                if (serializer == null)
                {
                    serializer = new XmlSerializer(typeof(Contract));
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

        public static bool Deserialize(string xml, out Contract obj, out Exception exception)
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

        public static bool Deserialize(string xml, out Contract obj)
        {
            Exception exception = null;
            return Deserialize(xml, out obj, out exception);
        }

        public static Contract Deserialize(string xml)
        {
            StringReader stringReader = null;
            try
            {
                stringReader = new StringReader(xml);
                return (Contract)Serializer.Deserialize(XmlReader.Create(stringReader));
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

        public static bool LoadFromFile(string fileName, out Contract obj, out Exception exception)
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

        public static bool LoadFromFile(string fileName, out Contract obj)
        {
            Exception exception = null;
            return LoadFromFile(fileName, out obj, out exception);
        }

        public static Contract LoadFromFile(string fileName)
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