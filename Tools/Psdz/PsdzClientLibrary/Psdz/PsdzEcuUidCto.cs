using PsdzClient;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz.Model.SecurityManagement
{
    [PreserveSource(AttributesModified = true)]
    [DataContract]
    public class PsdzEcuUidCto : IPsdzEcuUidCto
    {
        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public string EcuUid { get; set; }
    }
}
