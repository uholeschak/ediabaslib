using PsdzClient;
using System.Collections.Generic;
using System.Runtime.Serialization;
using BMW.Rheingold.Psdz.Model.SecureCoding;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.SecureCoding;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.SecureCoding
{
    [PreserveSource(AttributesModified = true)]
    [DataContract]
    [KnownType(typeof(PsdzCalculatedNcdsEto))]
    [KnownType(typeof(PsdzScbResultCto))]
    public class PsdzCalculationNcdResultCto : IPsdzCalculationNcdResultCto
    {
        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public IList<IPsdzCalculatedNcdsEto> CalculatedNcds { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public IPsdzScbResultCto ScbResultCto { get; set; }
    }
}
