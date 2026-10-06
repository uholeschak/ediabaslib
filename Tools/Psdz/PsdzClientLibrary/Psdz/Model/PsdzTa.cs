using PsdzClient;
using System.Runtime.Serialization;
using BMW.Rheingold.Psdz.Model;
using BMW.Rheingold.Psdz.Model.Tal;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Tal
{
    [PreserveSource(AttributesModified = true)]
    [DataContract]
    [KnownType(typeof(PsdzSgbmId))]
    public class PsdzTa : PsdzTalElement, IPsdzTa, IPsdzTalElement
    {
        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public IPsdzSgbmId SgbmId { get; set; }
    }
}
