using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace BMW.Rheingold.Measurement.Common.Data
{
    [Serializable]
    [GeneratedCode("Xsd2Code", "3.4.0.38968")]
    [DesignerCategory("code")]
    [XmlRoot(Namespace = "", IsNullable = true)]
    public class DisplayType
    {
        private bool logField;

        private bool logFieldSpecified;

        private bool recordField;

        private bool recordFieldSpecified;

        private bool comperssField;

        private bool comperssFieldSpecified;

        private bool stimuliField;

        private bool stimuliFieldSpecified;

        private bool holdField;

        private bool holdFieldSpecified;

        private static XmlSerializer serializer;

        [XmlElement(Order = 0)]
        public bool Log
        {
            get
            {
                return logField;
            }
            set
            {
                logField = value;
            }
        }

        [XmlIgnore]
        public bool LogSpecified
        {
            get
            {
                return logFieldSpecified;
            }
            set
            {
                logFieldSpecified = value;
            }
        }

        [XmlElement(Order = 1)]
        public bool Record
        {
            get
            {
                return recordField;
            }
            set
            {
                recordField = value;
            }
        }

        [XmlIgnore]
        public bool RecordSpecified
        {
            get
            {
                return recordFieldSpecified;
            }
            set
            {
                recordFieldSpecified = value;
            }
        }

        [XmlElement(Order = 2)]
        public bool Comperss
        {
            get
            {
                return comperssField;
            }
            set
            {
                comperssField = value;
            }
        }

        [XmlIgnore]
        public bool ComperssSpecified
        {
            get
            {
                return comperssFieldSpecified;
            }
            set
            {
                comperssFieldSpecified = value;
            }
        }

        [XmlElement(Order = 3)]
        public bool Stimuli
        {
            get
            {
                return stimuliField;
            }
            set
            {
                stimuliField = value;
            }
        }

        [XmlIgnore]
        public bool StimuliSpecified
        {
            get
            {
                return stimuliFieldSpecified;
            }
            set
            {
                stimuliFieldSpecified = value;
            }
        }

        [XmlElement(Order = 4)]
        public bool Hold
        {
            get
            {
                return holdField;
            }
            set
            {
                holdField = value;
            }
        }

        [XmlIgnore]
        public bool HoldSpecified
        {
            get
            {
                return holdFieldSpecified;
            }
            set
            {
                holdFieldSpecified = value;
            }
        }

        private static XmlSerializer Serializer
        {
            get
            {
                if (serializer == null)
                {
                    serializer = new XmlSerializer(typeof(DisplayType));
                }
                return serializer;
            }
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

        public static bool Deserialize(string xml, out DisplayType obj, out Exception exception)
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

        public static bool Deserialize(string xml, out DisplayType obj)
        {
            Exception exception = null;
            return Deserialize(xml, out obj, out exception);
        }

        public static DisplayType Deserialize(string xml)
        {
            StringReader stringReader = null;
            try
            {
                stringReader = new StringReader(xml);
                return (DisplayType)Serializer.Deserialize(XmlReader.Create(stringReader));
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

        public static bool LoadFromFile(string fileName, out DisplayType obj, out Exception exception)
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

        public static bool LoadFromFile(string fileName, out DisplayType obj)
        {
            Exception exception = null;
            return LoadFromFile(fileName, out obj, out exception);
        }

        public static DisplayType LoadFromFile(string fileName)
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
