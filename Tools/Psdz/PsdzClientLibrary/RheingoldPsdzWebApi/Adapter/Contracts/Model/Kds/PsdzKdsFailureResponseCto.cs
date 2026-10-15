using PsdzClient;
using System.Runtime.Serialization;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Sfa;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Sfa.LocalizableMessageTo;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Kds
{
    [PreserveSource(AttributesModified = true)]
    [KnownType(typeof(PsdzLocalizableMessageTo))]
    [DataContract]
    [KnownType(typeof(PsdzKdsIdCto))]
    public class PsdzKdsFailureResponseCto : IPsdzKdsFailureResponseCto
    {
        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public ILocalizableMessageTo Cause { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public IPsdzKdsIdCto KdsId { get; set; }
    }
}
