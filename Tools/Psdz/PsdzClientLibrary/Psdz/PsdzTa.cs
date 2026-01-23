using PsdzClient;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz.Model.Tal
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
