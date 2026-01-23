using PsdzClient;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz.Model.Tal.TalFilter
{
    [PreserveSource(AttributesModified = true)]
    [DataContract]
    public class PsdzTalFilter : IPsdzTalFilter
    {
        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public string AsXml { get; set; }
    }
}
