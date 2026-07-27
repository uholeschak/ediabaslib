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
    [XmlRoot(Namespace = "http://www.bmw.com/ibase/beans/dealerdata", IsNullable = false)]
    [DataContract(Name = "DistributionPartner", Namespace = "http://www.bmw.com/ibase/beans/dealerdata")]
    public class DistributionPartner : INotifyPropertyChanged
    {
        private ObservableCollection<Outlet> outletField;

        private string distributionPartnerNumberField;

        private string nameField;

        private static XmlSerializer serializer;

        [XmlElement("outlet", Form = XmlSchemaForm.Unqualified, Order = 0)]
        [DataMember]
        public ObservableCollection<Outlet> outlet
        {
            get
            {
                return outletField;
            }
            set
            {
                if (outletField != null)
                {
                    if (!outletField.Equals(value))
                    {
                        outletField = value;
                        OnPropertyChanged("outlet");
                    }
                }
                else
                {
                    outletField = value;
                    OnPropertyChanged("outlet");
                }
            }
        }

        [XmlAttribute]
        [DataMember]
        public string distributionPartnerNumber
        {
            get
            {
                return distributionPartnerNumberField;
            }
            set
            {
                if (distributionPartnerNumberField != null)
                {
                    if (!distributionPartnerNumberField.Equals(value))
                    {
                        distributionPartnerNumberField = value;
                        OnPropertyChanged("distributionPartnerNumber");
                    }
                }
                else
                {
                    distributionPartnerNumberField = value;
                    OnPropertyChanged("distributionPartnerNumber");
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

        private static XmlSerializer Serializer
        {
            get
            {
                if (serializer == null)
                {
                    serializer = new XmlSerializer(typeof(DistributionPartner));
                }
                return serializer;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public DistributionPartner()
        {
            outletField = new ObservableCollection<Outlet>();
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

        public static bool Deserialize(string xml, out DistributionPartner obj, out Exception exception)
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

        public static bool Deserialize(string xml, out DistributionPartner obj)
        {
            Exception exception = null;
            return Deserialize(xml, out obj, out exception);
        }

        public static DistributionPartner Deserialize(string xml)
        {
            StringReader stringReader = null;
            try
            {
                stringReader = new StringReader(xml);
                return (DistributionPartner)Serializer.Deserialize(XmlReader.Create(stringReader));
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

        public static bool LoadFromFile(string fileName, out DistributionPartner obj, out Exception exception)
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

        public static bool LoadFromFile(string fileName, out DistributionPartner obj)
        {
            Exception exception = null;
            return LoadFromFile(fileName, out obj, out exception);
        }

        public static DistributionPartner LoadFromFile(string fileName)
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
