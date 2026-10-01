using PsdzClient;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz.Model.Tal
{
    [PreserveSource(AttributesModified = true)]
    [DataContract]
    public class PsdzIdLightBasisTa : PsdzTa
    {
        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public IEnumerable<string> Ids { get; set; }
    }
}
