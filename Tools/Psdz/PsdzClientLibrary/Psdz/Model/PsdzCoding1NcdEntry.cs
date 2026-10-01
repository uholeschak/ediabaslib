using PsdzClient;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz.Model.SecureCoding
{
    [PreserveSource(AttributesModified = true)]
    [DataContract]
    public class PsdzCoding1NcdEntry : IPsdzCoding1NcdEntry
    {
        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public int BlockAdress { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public byte[] UserData { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public bool IsWriteable { get; set; }
    }
}
