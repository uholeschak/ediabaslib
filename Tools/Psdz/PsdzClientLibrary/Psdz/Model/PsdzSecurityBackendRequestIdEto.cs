using System.Runtime.Serialization;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Sfa
{
    [DataContract]
    public class PsdzSecurityBackendRequestIdEto : IPsdzSecurityBackendRequestIdEto
    {
        [DataMember]
        public int Value { get; set; }
    }
}
