using PsdzClient;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz.Model.Swt
{
    [PreserveSource(AttributesModified = true)]
    [DataContract]
    [KnownType(typeof(PsdzSwtEcu))]
    public class PsdzSwtAction : IPsdzSwtAction
    {
        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public IEnumerable<IPsdzSwtEcu> SwtEcus { get; set; }
    }
}
