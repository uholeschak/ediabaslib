using PsdzClient;
using System.Runtime.Serialization;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Swt;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Tal
{
    [PreserveSource(AttributesModified = true)]
    [KnownType(typeof(PsdzSwtApplicationId))]
    [DataContract]
    public class PsdzFscDeployTa : PsdzTa
    {
        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public PsdzSwtActionType? Action { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public IPsdzSwtApplicationId ApplicationId { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public byte[] Fsc { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public byte[] FscCert { get; set; }
    }
}
