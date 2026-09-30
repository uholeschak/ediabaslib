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
    public class TriggerType
    {
        private int positionField;

        private string levelField;

        private string slopeField;

        private string modeField;

        private string sourceNameField;

        private static XmlSerializer serializer;

        [XmlElement(Order = 0)]
        public int Position
        {
            get
            {
                return positionField;
            }
            set
            {
                positionField = value;
            }
        }

        [XmlElement(Order = 1)]
        public string Level
        {
            get
            {
                return levelField;
            }
            set
            {
                levelField = value;
            }
        }

        [XmlElement(Order = 2)]
        public string Slope
        {
            get
            {
                return slopeField;
            }
            set
            {
                slopeField = value;
            }
        }

        [XmlElement(Order = 3)]
        public string Mode
        {
            get
            {
                return modeField;
            }
            set
            {
                modeField = value;
            }
        }

        [XmlAttribute]
        public string SourceName
        {
            get
            {
                return sourceNameField;
            }
            set
            {
                sourceNameField = value;
            }
        }

        private static XmlSerializer Serializer
        {
            get
            {
                if (serializer == null)
                {
                    serializer = new XmlSerializer(typeof(TriggerType));
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

        public static bool Deserialize(string xml, out TriggerType obj, out Exception exception)
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

        public static bool Deserialize(string xml, out TriggerType obj)
        {
            Exception exception = null;
            return Deserialize(xml, out obj, out exception);
        }

        public static TriggerType Deserialize(string xml)
        {
            StringReader stringReader = null;
            try
            {
                stringReader = new StringReader(xml);
                return (TriggerType)Serializer.Deserialize(XmlReader.Create(stringReader));
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

        public static bool LoadFromFile(string fileName, out TriggerType obj, out Exception exception)
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

        public static bool LoadFromFile(string fileName, out TriggerType obj)
        {
            Exception exception = null;
            return LoadFromFile(fileName, out obj, out exception);
        }

        public static TriggerType LoadFromFile(string fileName)
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
