using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Programming
{
    internal class AsamJobInputDictionary : IAsamJobInputDictionary
    {
        private readonly IDictionary<string, object> jobParams;
        public ICollection<object> Values => jobParams.Values;

        internal AsamJobInputDictionary()
        {
            jobParams = new Dictionary<string, object>();
        }

        public void Add(string key, int value)
        {
            jobParams.Add(key, value);
        }

        public void Add(string key, string value)
        {
            jobParams.Add(key, value);
        }

        public void Add(string key, long value)
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
            if (!jobParams.ContainsKey(key))
            {
                return null;
            }

            return jobParams[key];
        }
    }
}