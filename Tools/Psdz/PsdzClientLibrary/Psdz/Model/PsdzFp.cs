using PsdzClient;
using System.Runtime.Serialization;
using BMW.Rheingold.Psdz.Model;
using RheingoldPsdzWebApi.Adapter.Contracts.Model;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model
{
    [PreserveSource(AttributesModified = true)]
    [DataContract]
    public class PsdzFp : PsdzStandardFp, IPsdzFp, IPsdzStandardFp
    {
        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public string Baureihenverbund { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public string Entwicklungsbaureihe { get; set; }
    }
}
