using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Psdz
{
    public enum PsdzBindingCalculationProgessStatus
    {
        Error,
        Running,
        Success,
        UnknownRequestId
    }

    [DataContract]
    public class PsdzFetchBindingCalculationResult
    {
        [DataMember]
        public int DurationOfLastRequest { get; set; }

        [DataMember]
        public PsdzSecurityCalculatedObjectCto[] CalculatedBindings { get; set; }

        [DataMember]
        public PsdzSecurityCalculatedObjectCto[] CalculatedCertificates { get; set; }

        [DataMember]
        public PsdzSecurityCalculatedObjectCto[] CalculatedKeypacks { get; set; }

        [DataMember]
        public PsdzBindingCalculationFailure[] Failures { get; set; }

        [DataMember]
        public PsdzBindingCalculationProgessStatus ProgressStatus { get; set; }
    }
}
