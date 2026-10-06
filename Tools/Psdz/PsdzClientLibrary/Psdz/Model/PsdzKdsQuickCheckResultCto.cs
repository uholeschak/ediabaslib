using PsdzClient;
using System.Runtime.Serialization;
using BMW.Rheingold.Psdz.Model.Kds;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Kds;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Kds
{
    [PreserveSource(AttributesModified = true)]
    [KnownType(typeof(PsdzKdsIdCto))]
    [DataContract]
    public class PsdzKdsQuickCheckResultCto : IPsdzKdsQuickCheckResultCto
    {
        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public IPsdzKdsIdCto KdsId { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public PsdzQuickCheckResultEto QuickCheckResult { get; set; }
    }
}
