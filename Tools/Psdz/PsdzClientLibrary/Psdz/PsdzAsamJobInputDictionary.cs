using PsdzClient;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz.Model
{
    [PreserveSource(AttributesModified = true)]
    [DataContract]
    public class PsdzAsamJobInputDictionary : IPsdzAsamJobInputDictionary
    {
        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        private IDictionary<string, object> jobParams = new Dictionary<string, object>();

        public void Add(string key, int value)
        {
            jobParams.Add(key, value);
        }

        public void Add(string key, long value)
        {
            jobParams.Add(key, value);
        }

        public void Add(string key, string value)
        {
            jobParams.Add(key, value);
        }

        public void Add(string key, byte[] value)
        {
            jobParams.Add(key, value);
        }

        public void Add(string key, float value)
        {
            jobParams.Add(key, value);
        }

        public void Add(string key, double value)
        {
            jobParams.Add(key, value);
        }

        public IDictionary<string, object> GetCopy()
        {
            return new Dictionary<string, object>(jobParams);
        }

        public object GetValue(string key)
        {
            return jobParams[key];
        }
    }
}
