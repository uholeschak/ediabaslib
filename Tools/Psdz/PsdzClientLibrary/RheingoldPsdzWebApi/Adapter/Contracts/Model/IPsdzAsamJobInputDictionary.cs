using System.Collections.Generic;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model
{
    public interface IPsdzAsamJobInputDictionary
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
