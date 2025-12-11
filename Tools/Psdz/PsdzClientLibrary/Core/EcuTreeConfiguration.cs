using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

#pragma warning disable CA2022
namespace PsdzClient.Core
{
    [Serializable]
    [XmlRoot("EcuTreeConfiguration", Namespace = "http://bmw.com/Rheingold/EcuTreeConfiguration")]
    public class EcuTreeConfiguration : IEcuTreeConfiguration
    {
        public const double DefaultRootHorizontalBusStep = 0.125;
        private const string RESOURCE_ECUTREECONFIGURATION_XSD = "BMW.Rheingold.Diagnostics.EcuCharacteristics.EcuTreeConfiguration.xsd";
        private static MemoryStream xsdEcuTreeConfigurationStream;
        private static XmlSerializer xmlSerializer;
        [XmlAttribute("SchemaVersion")]
        public string SchemaVersion { get; set; }

        [XmlAttribute("MainSeriesSgbd")]
        public string MainSeriesSgbd { get; set; }

        [XmlIgnore]
        public string CompatibilityInfo { get; private set; }

        [XmlElement("CompatibilityInfo", IsNullable = true)]
        public string XmlCompatibilityInfo
        {
            get
            {
                if (CompatibilityInfo == null)
                {
                    return null;
                }

                return CompressString(CompatibilityInfo);
            }

            set
            {
                if (value == null)
                {
                    CompatibilityInfo = null;
                }
                else
                {
                    CompatibilityInfo = DecompressString(value);
                }
            }
        }

        [XmlIgnore]
        public string SitInfo { get; private set; }

        [XmlElement("SitInfo", IsNullable = true)]
        public string XmlSitInfo
        {
            get
            {
                if (SitInfo == null)
                {
                    return null;
                }

                return CompressString(SitInfo);
            }

            set
            {
                if (value == null)
                {
                    SitInfo = null;
                }
                else
                {
                    SitInfo = DecompressString(value);
                }
            }
        }

        [XmlElement("RootHorizontalBusStep", IsNullable = true)]
        public double? RootHorizontalBusStep { get; set; }

        [XmlElement("EcuLogisticsList", Type = typeof(List<EcuLogisticsEntry>), IsNullable = false)]
        public List<EcuLogisticsEntry> EcuLogisticsList { get; set; }

        [XmlElement("BusLogisticsList", Type = typeof(List<BusLogisticsEntry>), IsNullable = true)]
        public List<BusLogisticsEntry> BusLogisticsList { get; set; }

        [XmlElement("BusInterConnectionList", Type = typeof(List<BusInterConnectionEntry>), IsNullable = true)]
        public List<BusInterConnectionEntry> BusInterConnectionList { get; set; }

        [XmlElement("CombinedEcuHousingList", Type = typeof(List<CombinedEcuHousingEntry>), IsNullable = true)]
        public List<CombinedEcuHousingEntry> CombinedEcuHousingList { get; set; }

        [XmlElement("SGBDBusLogisticsList", Type = typeof(List<SGBDBusLogisticsEntry>), IsNullable = true)]
        public List<SGBDBusLogisticsEntry> SGBDBusLogisticsList { get; set; }

        [XmlElement("XGBMBusLogisticsList", Type = typeof(List<XGBMBusLogisticsEntry>), IsNullable = true)]
        public List<XGBMBusLogisticsEntry> XGBMBusLogisticsList { get; set; }

        [XmlElement("BusNameList", Type = typeof(List<BusNameEntry>), IsNullable = true)]
        public List<BusNameEntry> BusNameList { get; set; }

        [XmlArray("MinimalConfigurationList", IsNullable = true)]
        [XmlArrayItem("MinimalConfigurationEntry")]
        public List<int> MinimalConfigurationList { get; set; }

        [XmlArray("ExcludedConfigurationList", IsNullable = true)]
        [XmlArrayItem("ExcludedConfigurationEntry")]
        public List<int> ExcludedConfigurationList { get; set; }

        [XmlArray("OptionalConfigurationList", IsNullable = true)]
        [XmlArrayItem("OptionalConfigurationEntry")]
        public List<int> OptionalConfigurationList { get; set; }

        [XmlArray("UnsureConfigurationList", IsNullable = true)]
        [XmlArrayItem("UnsureConfigurationEntry")]
        public List<int> UnsureConfigurationList { get; set; }

        [XmlArray("XorConfigurationList", IsNullable = true)]
        [XmlArrayItem("XorConfigurationEntry")]
        public List<int[]> XorConfigurationList { get; set; }

        ReadOnlyCollection<IEcuLogisticsEntry> IEcuTreeConfiguration.EcuLogisticsList => ((IEnumerable<IEcuLogisticsEntry>)EcuLogisticsList).ToList().AsReadOnly();

        ReadOnlyCollection<IBusLogisticsEntry> IEcuTreeConfiguration.BusLogisticsList
        {
            get
            {
                if (BusLogisticsList.Count == 0)
                {
                    return null;
                }

                return ((IEnumerable<IBusLogisticsEntry>)BusLogisticsList).ToList().AsReadOnly();
            }
        }

        ReadOnlyCollection<IBusInterConnectionEntry> IEcuTreeConfiguration.BusInterConnectionList
        {
            get
            {
                if (BusInterConnectionList.Count == 0)
                {
                    return null;
                }

                return ((IEnumerable<IBusInterConnectionEntry>)BusInterConnectionList).ToList().AsReadOnly();
            }
        }

        ReadOnlyCollection<ICombinedEcuHousingEntry> IEcuTreeConfiguration.CombinedEcuHousingList
        {
            get
            {
                if (CombinedEcuHousingList.Count == 0)
                {
                    return null;
                }

                return ((IEnumerable<ICombinedEcuHousingEntry>)CombinedEcuHousingList).ToList().AsReadOnly();
            }
        }

        ReadOnlyCollection<ISGBDBusLogisticsEntry> IEcuTreeConfiguration.SGBDBusLogisticsList
        {
            get
            {
                if (SGBDBusLogisticsList.Count == 0)
                {
                    return null;
                }

                return ((IEnumerable<ISGBDBusLogisticsEntry>)SGBDBusLogisticsList).ToList().AsReadOnly();
            }
        }

        ReadOnlyCollection<IBusNameEntry> IEcuTreeConfiguration.BusNameList
        {
            get
            {
                if (BusNameList.Count == 0)
                {
                    return null;
                }

                return ((IEnumerable<IBusNameEntry>)BusNameList).ToList().AsReadOnly();
            }
        }

        ReadOnlyCollection<IXGBMBusLogisticsEntry> IEcuTreeConfiguration.XGBMBusLogisticsList
        {
            get
            {
                if (XGBMBusLogisticsList.Count == 0)
                {
                    return null;
                }

                return ((IEnumerable<IXGBMBusLogisticsEntry>)XGBMBusLogisticsList).ToList().AsReadOnly();
            }
        }

        private static MemoryStream XsdEcuTreeConfigurationStream
        {
            get
            {
                if (xsdEcuTreeConfigurationStream == null)
                {
                    try
                    {
                        using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("BMW.Rheingold.Diagnostics.EcuCharacteristics.EcuTreeConfiguration.xsd"))
                        {
                            if (stream == null)
                            {
                                throw new IOException(string.Format("('{0}') could not be found!", "BMW.Rheingold.Diagnostics.EcuCharacteristics.EcuTreeConfiguration.xsd"));
                            }

                            xsdEcuTreeConfigurationStream = new MemoryStream();
                            stream.CopyTo(xsdEcuTreeConfigurationStream);
                        }
                    }
                    catch (Exception exception)
                    {
                        Log.ErrorException("EcuTreeConfiguration.XsdEcuTreeConfigurationStream", exception);
                    }
                }

                xsdEcuTreeConfigurationStream.Position = 0L;
                return xsdEcuTreeConfigurationStream;
            }
        }

        private static XmlSerializer XmlSerializer
        {
            get
            {
                if (xmlSerializer == null)
                {
                    xmlSerializer = new XmlSerializer(typeof(EcuTreeConfiguration));
                }

                return xmlSerializer;
            }
        }

        private static string CompressString(string text)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            MemoryStream memoryStream = new MemoryStream();
            using (GZipStream gZipStream = new GZipStream(memoryStream, CompressionMode.Compress, leaveOpen: true))
            {
                gZipStream.Write(bytes, 0, bytes.Length);
            }

            memoryStream.Position = 0L;
            byte[] array = new byte[memoryStream.Length];
            memoryStream.Read(array, 0, array.Length);
            byte[] array2 = new byte[array.Length + 4];
            Buffer.BlockCopy(array, 0, array2, 4, array.Length);
            Buffer.BlockCopy(BitConverter.GetBytes(bytes.Length), 0, array2, 0, 4);
            return Convert.ToBase64String(array2);
        }

        private static string DecompressString(string compressedText)
        {
            byte[] array = Convert.FromBase64String(compressedText);
            using (MemoryStream memoryStream = new MemoryStream())
            {
                int num = BitConverter.ToInt32(array, 0);
                memoryStream.Write(array, 4, array.Length - 4);
                byte[] array2 = new byte[num];
                memoryStream.Position = 0L;
                using (GZipStream gZipStream = new GZipStream(memoryStream, CompressionMode.Decompress))
                {
                    gZipStream.Read(array2, 0, array2.Length);
                }

                return Encoding.UTF8.GetString(array2);
            }
        }

        public EcuTreeConfiguration()
        {
        }

        internal EcuTreeConfiguration(BaseEcuCharacteristics EcuCharacteristic)
        {
            SchemaVersion = "0.1";
            MainSeriesSgbd = EcuCharacteristic.brSgbd;
            CompatibilityInfo = EcuCharacteristic.compatibilityInfo;
            SitInfo = EcuCharacteristic.sitInfo;
            RootHorizontalBusStep = EcuCharacteristic.rootHorizontalBusStep;
            if (EcuCharacteristic.ecuTable != null)
            {
                EcuLogisticsList = EcuCharacteristic.ecuTable.Cast<EcuLogisticsEntry>().ToList();
            }

            if (EcuCharacteristic.busTable != null)
            {
                BusLogisticsList = EcuCharacteristic.busTable.Cast<BusLogisticsEntry>().ToList();
            }

            if (EcuCharacteristic.interConnectionTable != null)
            {
                BusInterConnectionList = EcuCharacteristic.interConnectionTable.Cast<BusInterConnectionEntry>().ToList();
            }

            if (EcuCharacteristic.combinedEcuHousingTable != null)
            {
                CombinedEcuHousingList = EcuCharacteristic.combinedEcuHousingTable.Cast<CombinedEcuHousingEntry>().ToList();
            }

            if (EcuCharacteristic.variantTable != null)
            {
                SGBDBusLogisticsList = EcuCharacteristic.variantTable.Cast<SGBDBusLogisticsEntry>().ToList();
            }

            if (EcuCharacteristic.busNameTable != null)
            {
                BusNameList = EcuCharacteristic.busNameTable.Cast<BusNameEntry>().ToList();
            }

            if (EcuCharacteristic.xgbdTable != null)
            {
                XGBMBusLogisticsList = EcuCharacteristic.xgbdTable.Cast<XGBMBusLogisticsEntry>().ToList();
            }

            if (EcuCharacteristic.minimalConfiguration != null)
            {
                MinimalConfigurationList = EcuCharacteristic.minimalConfiguration.ToList();
            }

            if (EcuCharacteristic.excludedConfiguration != null)
            {
                ExcludedConfigurationList = EcuCharacteristic.excludedConfiguration.ToList();
            }

            if (EcuCharacteristic.optionalConfiguration != null)
            {
                OptionalConfigurationList = EcuCharacteristic.optionalConfiguration.ToList();
            }

            if (EcuCharacteristic.unsureConfiguration != null)
            {
                UnsureConfigurationList = EcuCharacteristic.unsureConfiguration.ToList();
            }

            if (EcuCharacteristic.xorConfiguration != null)
            {
                XorConfigurationList = EcuCharacteristic.xorConfiguration.ToList();
            }
        }

        public static bool ValidateFile(Stream xmlFileStream, ValidationEventHandler validationEventHandler = null)
        {
            XmlReader xmlReader = null;
            XmlReader xmlReader2 = null;
            bool successfull = true;
            try
            {
                xmlFileStream.Position = 0L;
                XmlReaderSettings xmlReaderSettings = new XmlReaderSettings();
                xmlReaderSettings.ValidationEventHandler += delegate
                {
                    successfull = false;
                };
                if (validationEventHandler != null)
                {
                    xmlReaderSettings.ValidationEventHandler += validationEventHandler;
                }

                xmlReaderSettings.ValidationEventHandler += xmlSettings_ValidationEventHandler;
                xmlReader2 = XmlReader.Create(XsdEcuTreeConfigurationStream);
                xmlReaderSettings.Schemas.Add("http://bmw.com/Rheingold/EcuTreeConfiguration", xmlReader2);
                xmlReaderSettings.ValidationType = ValidationType.Schema;
                xmlReaderSettings.ValidationFlags |= XmlSchemaValidationFlags.ProcessInlineSchema;
                xmlReaderSettings.ValidationFlags |= XmlSchemaValidationFlags.ProcessSchemaLocation;
                xmlReaderSettings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;
                xmlReaderSettings.Schemas.Compile();
                xmlReader = XmlReader.Create(xmlFileStream, xmlReaderSettings);
                while (xmlReader.Read())
                {
                }

                return successfull;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                xmlReader?.Close();
                xmlReader2?.Close();
            }
        }

        private static void xmlSettings_ValidationEventHandler(object sender, ValidationEventArgs e)
        {
            _ = e.Message;
        }

        public virtual string Serialize()
        {
            StreamReader streamReader = null;
            MemoryStream memoryStream = null;
            try
            {
                memoryStream = new MemoryStream();
                XmlSerializer.Serialize(memoryStream, this);
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

        public static bool Deserialize(string xml, out EcuTreeConfiguration obj, out Exception exception)
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

        private static bool Deserialize(string xml, out EcuTreeConfiguration obj)
        {
            Exception exception;
            return Deserialize(xml, out obj, out exception);
        }

        private static EcuTreeConfiguration Deserialize(string xml)
        {
            StringReader stringReader = null;
            try
            {
                stringReader = new StringReader(xml);
                return (EcuTreeConfiguration)XmlSerializer.Deserialize(XmlReader.Create(stringReader));
            }
            finally
            {
                stringReader?.Dispose();
            }
        }

        public static bool WriteToStream(out Stream xmlFileStream, EcuTreeConfiguration obj, out Exception exception)
        {
            xmlFileStream = null;
            exception = null;
            StreamWriter streamWriter = null;
            try
            {
                xmlFileStream = new MemoryStream();
                string value = obj.Serialize();
                streamWriter = new StreamWriter(xmlFileStream);
                streamWriter.Write(value);
                streamWriter.Flush();
                xmlFileStream.Position = 0L;
                return true;
            }
            catch (Exception ex)
            {
                exception = ex;
                streamWriter?.Dispose();
                return false;
            }
        }

        public static bool WriteToStream(out Stream xmlFileStream, EcuTreeConfiguration obj)
        {
            Exception exception;
            return WriteToStream(out xmlFileStream, obj, out exception);
        }

        public static Stream WriteToStream(EcuTreeConfiguration obj)
        {
            if (WriteToStream(out var xmlFileStream, obj, out var exception))
            {
                if (xmlFileStream == null)
                {
                    throw new NullReferenceException();
                }

                return xmlFileStream;
            }

            throw exception;
        }

        public static bool ReadFromStream(Stream xmlFileStream, out EcuTreeConfiguration obj, out Exception exception)
        {
            obj = null;
            exception = null;
            StreamReader streamReader = null;
            try
            {
                xmlFileStream.Position = 0L;
                streamReader = new StreamReader(xmlFileStream);
                string xml = streamReader.ReadToEnd();
                streamReader.Close();
                obj = Deserialize(xml);
                return true;
            }
            catch (Exception ex)
            {
                exception = ex;
                return false;
            }
            finally
            {
                streamReader?.Dispose();
            }
        }

        public static bool ReadFromStream(Stream xmlFileStream, out EcuTreeConfiguration obj)
        {
            Exception exception;
            return ReadFromStream(xmlFileStream, out obj, out exception);
        }

        public static EcuTreeConfiguration ReadFromStream(Stream xmlFileStream)
        {
            if (ReadFromStream(xmlFileStream, out var obj, out var exception))
            {
                if (obj == null)
                {
                    throw new NullReferenceException();
                }

                return obj;
            }

            throw exception;
        }
    }
}