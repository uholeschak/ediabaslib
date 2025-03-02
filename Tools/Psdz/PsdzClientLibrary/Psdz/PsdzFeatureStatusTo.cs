using BMW.Rheingold.Psdz.Model.SecureCoding;
using BMW.Rheingold.Psdz.Model.Sfa;
using BMW.Rheingold.Psdz;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz
{
    [DataContract]
    [KnownType(typeof(PsdzFeatureIdCto))]
    [KnownType(typeof(PsdzDiagAddressCto))]
    public class PsdzFeatureStatusTo : IPsdzFeatureStatusTo
    {
        [DataMember]
        public PsdzFeatureStatusEtoEnum FeatureStatus { get; set; }

        [DataMember]
        public IPsdzFeatureIdCto FeatureId { get; set; }

        [DataMember]
        public IPsdzDiagAddressCto DiagAddress { get; set; }

        [DataMember]
        public PsdzValidationStatusEtoEnum ValidationStatus { get; set; }
    }
}