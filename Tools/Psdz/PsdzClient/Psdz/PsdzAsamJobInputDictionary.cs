using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace BMW.Rheingold.Psdz.Model
{
    [DataContract]
    public class PsdzAsamJobInputDictionary : IPsdzAsamJobInputDictionary
    {
        public void Add(string key, int value)
        {
            this.jobParams.Add(key, value);
        }

        public void Add(string key, long value)
        {
            this.jobParams.Add(key, value);
        }

        public void Add(string key, string value)
        {
            this.jobParams.Add(key, value);
        }

        public void Add(string key, byte[] value)
        {
            this.jobParams.Add(key, value);
        }

        public void Add(string key, float value)
        {
            this.jobParams.Add(key, value);
        }

        public void Add(string key, double value)
        {
            this.jobParams.Add(key, value);
        }

        public IDictionary<string, object> GetCopy()
        {
            return new Dictionary<string, object>(this.jobParams);
        }

        public object GetValue(string key)
        {
            return this.jobParams[key];
        }

        [DataMember]
        private IDictionary<string, object> jobParams = new Dictionary<string, object>();
    }
}
