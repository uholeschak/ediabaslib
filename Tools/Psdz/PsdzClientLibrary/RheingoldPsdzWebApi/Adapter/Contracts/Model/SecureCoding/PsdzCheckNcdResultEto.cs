using PsdzClient;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.SecureCoding
{
    [PreserveSource(AttributesModified = true)]
    [DataContract]
    [KnownType(typeof(PsdzDetailedNcdInfoEto))]
    public class PsdzCheckNcdResultEto : IPsdzCheckNcdResultEto
    {
        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public IList<IPsdzDetailedNcdInfoEto> DetailedNcdStatus { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public bool isEachNcdSigned { get; set; }
    }
}
