using BMW.Rheingold.Psdz.Model.SecureCoding;
using BMW.Rheingold.Psdz.Model.Sfa;
using PsdzClient;
using System.Runtime.Serialization;
using BMW.Rheingold.Psdz;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.SecureCoding;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Sfa;

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