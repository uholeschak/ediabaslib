using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace BMW.Rheingold.Psdz.Model.SecureCoding
{
    [KnownType(typeof(PsdzCalculatedNcdsEto))]
    [KnownType(typeof(PsdzScbResultCto))]
    [DataContract]
    public class PsdzCalculationNcdResultCto : IPsdzCalculationNcdResultCto
    {
        [DataMember]
        public string CalculatedNcdAsString { get; set; }

        [DataMember]
        public IList<IPsdzCalculatedNcdsEto> CalculatedNcds { get; set; }

        [DataMember]
        public IPsdzScbResultCto ScbResultCto { get; set; }
    }
}
