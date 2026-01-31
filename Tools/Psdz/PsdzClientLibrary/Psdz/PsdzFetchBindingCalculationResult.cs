using PsdzClient;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz.Model.Certificate
{
    [PreserveSource(AttributesModified = true)]
    [DataContract]
    public class PsdzFetchBindingCalculationResult
    {
        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public int DurationOfLastRequest { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public PsdzSecurityCalculatedObjectCto[] CalculatedBindings { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public PsdzSecurityCalculatedObjectCto[] CalculatedCertificates { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public PsdzSecurityCalculatedObjectCto[] CalculatedKeypacks { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public PsdzBindingCalculationFailure[] Failures { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public PsdzBindingCalculationProgessStatus ProgressStatus { get; set; }
    }
}
