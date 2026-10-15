using System.Runtime.Serialization;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Sfa.LocalizableMessageTo;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Sfa
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
