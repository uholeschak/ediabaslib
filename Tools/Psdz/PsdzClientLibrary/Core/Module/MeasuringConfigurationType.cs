using Microsoft.Win32;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;

namespace BMW.Rheingold.Measurement.Common.Data
{
    [Serializable]
    [GeneratedCode("Xsd2Code", "3.4.0.38968")]
    [DesignerCategory("code")]
    [XmlRoot("MeasuringConfiguration", Namespace = "", IsNullable = false)]
    public class MeasuringConfigurationType
    {
        private DmmType dmmField;
        private DsoType dsoField;
        private StgType stgField;
        private ImibCounterConfigData cntField;
        private static XmlSerializer serializer;
        [XmlElement(Order = 0)]
        public DmmType Dmm
        {
            get
            {
                return dmmField;
            }

            set
            {
                dmmField = value;
            }
        }

        [XmlElement(Order = 1)]
        public DsoType Dso
        {
            get
            {
                return dsoField;
            }

            set
            {
                dsoField = value;
            }
        }

        [XmlElement(Order = 2)]
        public StgType Stg
        {
            get
            {
                return stgField;
            }

            set
            {
                stgField = value;
            }
        }

        [XmlElement(Order = 3)]
        public ImibCounterConfigData CntField
        {
            get
            {
                return cntField;
            }

            set
            {
                cntField = value;
            }
        }

        [XmlElement(Order = 4)]
        public string SelectedMeasurement { get; set; }

        private static XmlSerializer Serializer
        {
            get
            {
                if (serializer == null)
                {
                    serializer = new XmlSerializer(typeof(MeasuringConfigurationType));
                }

                return serializer;
            }
        }

        public MeasuringConfigurationType()
        {
            stgField = new StgType();
            dsoField = new DsoType();
            dmmField = new DmmType();
            cntField = default;
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

        public static bool Deserialize(string xml, out MeasuringConfigurationType obj, out Exception exception)
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

        public static bool Deserialize(string xml, out MeasuringConfigurationType obj)
        {
            Exception exception = null;
            return Deserialize(xml, out obj, out exception);
        }

        public static MeasuringConfigurationType Deserialize(string xml)
        {
            StringReader stringReader = null;
            try
            {
                stringReader = new StringReader(xml);
                return (MeasuringConfigurationType)Serializer.Deserialize(XmlReader.Create(stringReader));
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

        public virtual void SaveToRegistry(string path, string name)
        {
            RegistryKey registryKey = null;
            try
            {
                string value = Serialize();
                registryKey = Registry.CurrentUser.CreateSubKey(path);
                registryKey.SetValue(name, value, RegistryValueKind.String);
            }
            finally
            {
                registryKey?.Close();
            }
        }

        public bool DoesKeyExist(string path, string keyName)
        {
            using (RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(path))
            {
                if (registryKey != null)
                {
                    return registryKey.GetValue(keyName) != null;
                }

                return false;
            }
        }

        public virtual IEnumerable<string> GetAvailableEntries(string path)
        {
            List<string> result = new List<string>();
            using (RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(path))
            {
                if (registryKey != null)
                {
                    result = registryKey.GetValueNames().ToList();
                }
            }

            return result;
        }

        public static bool LoadFromFile(string fileName, out MeasuringConfigurationType obj, out Exception exception)
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

        public static bool LoadFromFile(string fileName, out MeasuringConfigurationType obj)
        {
            Exception exception = null;
            return LoadFromFile(fileName, out obj, out exception);
        }

        public static MeasuringConfigurationType LoadFromFile(string fileName)
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

        public static MeasuringConfigurationType LoadFromRegistry(string path, string key)
        {
            RegistryKey registryKey = null;
            try
            {
                registryKey = Registry.CurrentUser.OpenSubKey(path);
                return Deserialize(registryKey.GetValue(key, string.Empty, RegistryValueOptions.None) as string);
            }
            finally
            {
                registryKey?.Close();
            }
        }
    }
}