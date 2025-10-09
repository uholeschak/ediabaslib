using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PsdzClient.Core
{
    public class DataHolder
    {
        [JsonProperty(PropertyName = "DataHolder")]
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

        public string ToJson(ILogger log)
        {
            try
            {
                return JsonConvert.SerializeObject(this, Formatting.Indented);
            }
            catch (Exception ex)
            {
                log.Error("DataHolder.ToJson", "Failed to serialize the Fusion Reactor data.", ex);
                return string.Empty;
            }
        }
    }
}
