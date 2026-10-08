using BMW.Rheingold.Psdz.Model.Sfa;
using PsdzClient;
using System.Runtime.Serialization;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.SecureCoding;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Sfa.FeatureStatusTo
{
    [PreserveSource(AttributesModified = true)]
    [DataContract]
    [KnownType(typeof(PsdzFeatureIdCto))]
    [KnownType(typeof(PsdzDiagAddressCto))]
    public class PsdzFeatureStatusTo : IPsdzFeatureStatusTo
    {
        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public PsdzFeatureStatusEtoEnum FeatureStatus { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public IPsdzFeatureIdCto FeatureId { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public IPsdzDiagAddressCto DiagAddress { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public PsdzValidationStatusEtoEnum ValidationStatus { get; set; }
    }
}