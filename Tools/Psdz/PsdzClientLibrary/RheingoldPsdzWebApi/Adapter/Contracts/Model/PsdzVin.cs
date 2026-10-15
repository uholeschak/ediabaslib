using PsdzClient;
using System.Runtime.Serialization;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model
{
    [PreserveSource(AttributesModified = true)]
    [DataContract]
    public class PsdzVin : IPsdzVin
    {
        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public string Value { get; set; }
    }
}
