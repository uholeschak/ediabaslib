using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace BMW.Rheingold.Psdz.Model.Sfa
{
    [DataContract]
    [KnownType(typeof(PsdzDetailedStatusCto))]
    [KnownType(typeof(PsdzSecurityBackendRequestFailureCto))]
    [KnownType(typeof(PsdzSecureTokenEto))]
    [KnownType(typeof(PsdzSecureTokenForVehicleEto))]
    public class PsdzFetchCalculationSecureTokensResultCto : IPsdzFetchCalculationSecureTokensResultCto
    {
        [DataMember]
        public IList<IPsdzDetailedStatusCto> DetailedStatus { get; set; }

        [DataMember]
        public int DurationOfLastRequest { get; set; }

        [DataMember]
        public IList<IPsdzSecurityBackendRequestFailureCto> Failures { get; set; }

        [DataMember]
        public string FeatureSetReference { get; set; }

        [DataMember]
        public string JsonString { get; set; }

        [DataMember]
        public PsdzSecurityBackendRequestProgressStatusToEnum ProgressStatus { get; set; }

        [DataMember]
        public IList<IPsdzSecureTokenEto> SecureTokens { get; set; }

        [DataMember]
        public IPsdzSecureTokenForVehicleEto SecureTokenForVehicle { get; set; }

        [DataMember]
        public PsdzTokenOverallStatusEtoEnum TokenOverallStatusEto { get; set; }

        [DataMember]
        public string TokenPackageReference { get; set; }
    }
}
