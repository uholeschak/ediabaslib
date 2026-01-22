using BMW.Rheingold.Psdz.Model.Sfa;
using BMW.Rheingold.Psdz.Model.Sfa.LocalizableMessageTo;
using PsdzClient;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz.Model.Kds
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
