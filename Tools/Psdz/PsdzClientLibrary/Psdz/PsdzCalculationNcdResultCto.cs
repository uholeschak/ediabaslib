using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz.Model.SecureCoding
{
    [DataContract]
    [KnownType(typeof(PsdzCalculatedNcdsEto))]
    [KnownType(typeof(PsdzScbResultCto))]
    public class PsdzCalculationNcdResultCto : IPsdzCalculationNcdResultCto
    {
        [DataMember]
        public IList<IPsdzCalculatedNcdsEto> CalculatedNcds { get; set; }

        [DataMember]
        public IPsdzScbResultCto ScbResultCto { get; set; }
    }
}
