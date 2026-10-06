using PsdzClient;
using System.Runtime.Serialization;
using BMW.Rheingold.Psdz.Model.SecureCoding;
using RheingoldPsdzWebApi.Adapter.Contracts.Model;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.SecureCoding;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.SecureCoding
{
    [PreserveSource(AttributesModified = true)]
    [DataContract]
    [KnownType(typeof(PsdzSgbmId))]
    [KnownType(typeof(PsdzNcd))]
    public class PsdzCalculatedNcdsEto : IPsdzCalculatedNcdsEto
    {
        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public string Btld { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public IPsdzSgbmId CafdId { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public IPsdzNcd Ncd { get; set; }
    }
}
