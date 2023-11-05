using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;

namespace PsdzClientLibrary.Core
{
    public class DataHolder
    {
        public List<DataHolderItem> Data { get; set; } = new List<DataHolderItem>();


        public IList<PropertyData<T>> GetPropertyCollection<T>(string propertyName)
        {
            List<PropertyData<T>> list = Data.Where((DataHolderItem i) => i.PropertyName == propertyName).FirstOrDefault()?.Values as List<PropertyData<T>>;
            if (list == null)
            {
                DataHolderItem dataHolderItem = new DataHolderItem
                {
                    PropertyName = propertyName,
                    Values = new List<PropertyData<T>>()
                };
                Data.Add(dataHolderItem);
                list = dataHolderItem.Values as List<PropertyData<T>>;
            }
            return list;
        }

        public string SerializeToXml(IMultisourceLogger log)
        {
            try
            {
                Type[] extraTypes = Data.Select((DataHolderItem d) => d.Values.GetType()).Distinct().ToArray();
                using (StringWriter stringWriter = new StringWriter())
                {
                    using (XmlWriter xmlWriter = XmlWriter.Create(stringWriter, new XmlWriterSettings
                           {
                               Indent = true
                           }))
                    {
                        XmlSerializer xmlSerializer = new XmlSerializer(typeof(DataHolder), null, extraTypes, new XmlRootAttribute("DataHolder"), "");
                        xmlSerializer.Serialize(xmlWriter, this);
                        return stringWriter.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(log.CurrentMethod(), "Failed to serialize the Fusion Reactor data.", ex);
                return string.Empty;
            }
        }
    }
}
