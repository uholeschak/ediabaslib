using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Programming
{
    public interface IAsamJobInputDictionary
    {
        void Add(string key, int value);

        void Add(string key, long value);

        void Add(string key, string value);

        void Add(string key, byte[] value);

        void Add(string key, float value);

        void Add(string key, double value);

        IDictionary<string, object> GetCopy();

        object GetValue(string key);
    }
}
