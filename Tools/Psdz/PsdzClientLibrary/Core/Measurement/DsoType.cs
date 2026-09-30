using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows.Input;
using System.Xml;
using System.Xml.Serialization;

namespace BMW.Rheingold.Measurement.Common.Data
{
    [Serializable]
    [GeneratedCode("Xsd2Code", "3.4.0.38968")]
    [DesignerCategory("code")]
    [XmlRoot(Namespace = "", IsNullable = true)]
    public class DsoType
    {
        private CursorType cursorField;

        private DisplayType displayField;

        private TimeType timeField;

        private List<ChannelType> channelField;

        private TriggerType triggerField;

        private long recordDelaySelection;

        private long recordDurationSelection;

        private static XmlSerializer serializer;

        [XmlElement(Order = 0)]
        public CursorType Cursor
        {
            get
            {
                return cursorField;
            }
            set
            {
                cursorField = value;
            }
        }

        [XmlElement(Order = 1)]
        public DisplayType Display
        {
            get
            {
                return displayField;
            }
            set
            {
                displayField = value;
            }
        }

        [XmlElement(Order = 2)]
        public TimeType Time
        {
            get
            {
                return timeField;
            }
            set
            {
                timeField = value;
            }
        }

        [XmlElement("Channel", Order = 3)]
        public List<ChannelType> Channel
        {
            get
            {
                return channelField;
            }
            set
            {
                channelField = value;
            }
        }

        [XmlElement(Order = 4)]
        public TriggerType Trigger
        {
            get
            {
                return triggerField;
            }
            set
            {
                triggerField = value;
            }
        }

        [XmlElement(Order = 5)]
        public long RecordDelaySelection
        {
            get
            {
                return recordDelaySelection;
            }
            set
            {
                recordDelaySelection = value;
            }
        }

        [XmlElement(Order = 6)]
        public long RecordDurationSelection
        {
            get
            {
                return recordDurationSelection;
            }
            set
            {
                recordDurationSelection = value;
            }
        }

        private static XmlSerializer Serializer
        {
            get
            {
                if (serializer == null)
                {
                    serializer = new XmlSerializer(typeof(DsoType));
                }
                return serializer;
            }
        }

        public DsoType()
        {
            triggerField = new TriggerType();
            channelField = new List<ChannelType>();
            timeField = new TimeType();
            displayField = new DisplayType();
            cursorField = new CursorType();
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

        public static bool Deserialize(string xml, out DsoType obj, out Exception exception)
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

        public static bool Deserialize(string xml, out DsoType obj)
        {
            Exception exception = null;
            return Deserialize(xml, out obj, out exception);
        }

        public static DsoType Deserialize(string xml)
        {
            StringReader stringReader = null;
            try
            {
                stringReader = new StringReader(xml);
                return (DsoType)Serializer.Deserialize(XmlReader.Create(stringReader));
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

        public static bool LoadFromFile(string fileName, out DsoType obj, out Exception exception)
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

        public static bool LoadFromFile(string fileName, out DsoType obj)
        {
            Exception exception = null;
            return LoadFromFile(fileName, out obj, out exception);
        }

        public static DsoType LoadFromFile(string fileName)
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
