using System.Runtime.Serialization;
using BMW.Rheingold.Psdz.Model.Sfa.LocalizableMessageTo;

namespace BMW.Rheingold.Psdz.Model.Sfa
{
    [DataContract]
    [KnownType(typeof(PsdzLocalizableMessageTo))]
    public class PsdzSecurityBackendRequestFailureCto : IPsdzSecurityBackendRequestFailureCto
    {
        [DataMember]
        public ILocalizableMessageTo Cause { get; set; }

        [DataMember]
        public int Retry { get; set; }

        [DataMember]
        public string Url { get; set; }
    }
}
