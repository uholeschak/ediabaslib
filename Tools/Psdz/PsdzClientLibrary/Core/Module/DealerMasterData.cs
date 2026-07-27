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
    [XmlRoot(Namespace = "http://www.bmw.com/ibase/beans/dealerdata", IsNullable = false)]
    [DataContract(Name = "DealerMasterData", Namespace = "http://www.bmw.com/ibase/beans/dealerdata")]
    public class DealerMasterData : INotifyPropertyChanged
    {
        private DistributionPartner distributionPartnerField;

        private DateTime expirationDateField;

        private string verificationCodeField;

        private string hardwareIdField;

        private static XmlSerializer serializer;

        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        [DataMember]
        public DistributionPartner distributionPartner
        {
            get
            {
                return distributionPartnerField;
            }
            set
            {
                if (distributionPartnerField != null)
                {
                    if (!distributionPartnerField.Equals(value))
                    {
                        distributionPartnerField = value;
                        OnPropertyChanged("distributionPartner");
                    }
                }
                else
                {
                    distributionPartnerField = value;
                    OnPropertyChanged("distributionPartner");
                }
            }
        }

        [XmlAttribute]
        [DataMember]
        public DateTime expirationDate
        {
            get
            {
                return expirationDateField;
            }
            set
            {
                if (!expirationDateField.Equals(value))
                {
                    expirationDateField = value;
                    OnPropertyChanged("expirationDate");
                }
            }
        }

        [XmlAttribute]
        [DataMember]
        public string verificationCode
        {
            get
            {
                return verificationCodeField;
            }
            set
            {
                if (verificationCodeField != null)
                {
                    if (!verificationCodeField.Equals(value))
                    {
                        verificationCodeField = value;
                        OnPropertyChanged("verificationCode");
                    }
                }
                else
                {
                    verificationCodeField = value;
                    OnPropertyChanged("verificationCode");
                }
            }
        }

        [XmlAttribute]
        [DataMember]
        public string hardwareId
        {
            get
            {
                return hardwareIdField;
            }
            set
            {
                if (hardwareIdField != null)
                {
                    if (!hardwareIdField.Equals(value))
                    {
                        hardwareIdField = value;
                        OnPropertyChanged("hardwareId");
                    }
                }
                else
                {
                    hardwareIdField = value;
                    OnPropertyChanged("hardwareId");
                }
            }
        }

        private static XmlSerializer Serializer
        {
            get
            {
                if (serializer == null)
                {
                    serializer = new XmlSerializer(typeof(DealerMasterData));
                }
                return serializer;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public DealerMasterData()
        {
            distributionPartnerField = new DistributionPartner();
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

        public static bool Deserialize(string xml, out DealerMasterData obj, out Exception exception)
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

        public static bool Deserialize(string xml, out DealerMasterData obj)
        {
            Exception exception = null;
            return Deserialize(xml, out obj, out exception);
        }

        public static DealerMasterData Deserialize(string xml)
        {
            StringReader stringReader = null;
            try
            {
                stringReader = new StringReader(xml);
                return (DealerMasterData)Serializer.Deserialize(XmlReader.Create(stringReader));
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

        public static bool LoadFromFile(string fileName, out DealerMasterData obj, out Exception exception)
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

        public static bool LoadFromFile(string fileName, out DealerMasterData obj)
        {
            Exception exception = null;
            return LoadFromFile(fileName, out obj, out exception);
        }

        public static DealerMasterData LoadFromFile(string fileName)
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
