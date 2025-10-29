using BMW.Rheingold.Psdz;
using BMW.Rheingold.Psdz.Model.Sfa;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz
{
    [DataContract]
    [KnownType(typeof(PsdzEcuFailureResponseCto))]
    [KnownType(typeof(PsdzProgrammingTokenCto))]
    public class PsdzProgrammingTokensResultCto : IPsdzProgrammingTokensResultCto
    {
        [DataMember]
        public IEnumerable<IPsdzEcuFailureResponseCto> Failures { get; set; }

        [DataMember]
        public IEnumerable<IPsdzProgrammingTokenCto> Tokens { get; set; }
    }
}