using PsdzClient;
using System.Runtime.Serialization;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Communications;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Tal
{
    [PreserveSource(AttributesModified = true)]
    [DataContract]
    [KnownType(typeof(PsdzProtocol))]
    public class PsdzIbaDeployTa : PsdzTa
    {
        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public PsdzProtocol? ActualProtocol { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public PsdzProtocol? PreferredProtocol { get; set; }
    }
}
