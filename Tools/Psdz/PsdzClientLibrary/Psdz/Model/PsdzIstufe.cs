using PsdzClient;
using System;
using System.Globalization;
using System.Runtime.Serialization;
using RheingoldPsdzWebApi.Adapter.Contracts.Model;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model
{
    [PreserveSource(AttributesModified = true)]
    [DataContract]
    public class PsdzIstufe : IPsdzIstufe, IComparable<IPsdzIstufe>
    {
        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public bool IsValid { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember(IsRequired = true)]
        public string Value { get; set; }

        public int CompareTo(IPsdzIstufe other)
        {
            if (other != null)
            {
                return string.Compare(Value, other.Value, ignoreCase: true, CultureInfo.InvariantCulture);
            }
            return 1;
        }

        public override string ToString()
        {
            return Value;
        }
    }
}
