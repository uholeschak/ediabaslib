using PsdzClient;
using System.Runtime.Serialization;
using RheingoldPsdzWebApi.Adapter.Contracts.Model;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.SecureCoding;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.SecureCoding
{
    [PreserveSource(AttributesModified = true)]
    [DataContract]
    [KnownType(typeof(PsdzSgbmId))]
    public class PsdzRequestNcdEto : IPsdzRequestNcdEto
    {
        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public IPsdzSgbmId Btld { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public IPsdzSgbmId Cafd { get; set; }
    }
}
