using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.Psdz.Model;

namespace BMW.Rheingold.Psdz
{
    // [UH] Only for compatibility with older version
    [KnownType(typeof(PsdzIstufe))]
    [DataContract]
    [KnownType(typeof(PsdzStandardFa))]
    [KnownType(typeof(PsdzStandardSvt))]
    public class TestRunParams
    {
        [DataMember]
        public int DurationTalLineExecution { get; set; }

        [DataMember]
        public int IncNoGeneratedTal { get; set; }

        [DataMember]
        public int InitNoGeneratedTal { get; set; }

        [DataMember]
        public IPsdzIstufe IstufeCurrent { get; set; }

        [DataMember]
        public IPsdzStandardFa StandardFa { get; set; }

        [DataMember]
        public IPsdzStandardSvt SvtCurrent { get; set; }
    }
}
