using BMW.Rheingold.Psdz.Model.SecureCoding;
using BMW.Rheingold.Psdz.Model.Sfa;
using PsdzClient;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz
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