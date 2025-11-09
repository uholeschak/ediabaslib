using BMW.Rheingold.Psdz.Model;
using PsdzClientLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace BMW.Rheingold.Psdz
{
    [PreserveSource(Removed = true)]
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
