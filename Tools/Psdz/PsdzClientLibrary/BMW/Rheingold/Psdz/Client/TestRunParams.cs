using PsdzClient;
using System.Runtime.Serialization;
using RheingoldPsdzWebApi.Adapter.Contracts.Model;

namespace BMW.Rheingold.Psdz.Client
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
