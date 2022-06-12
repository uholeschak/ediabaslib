using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Programming
{
    public class AsamJobInputDictionary : IAsamJobInputDictionary
    {
        internal AsamJobInputDictionary()
        {
            this.jobParams = new Dictionary<string, object>();
        }

        public ICollection<object> Values
        {
            get
            {
                return this.jobParams.Values;
            }
        }

        public void Add(string key, int value)
        {
            this.jobParams.Add(key, value);
        }

        public void Add(string key, string value)
        {
            this.jobParams.Add(key, value);
        }

        public void Add(string key, long value)
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
            if (!this.jobParams.ContainsKey(key))
            {
                return null;
            }
            return this.jobParams[key];
        }

        private readonly IDictionary<string, object> jobParams;
    }
}
