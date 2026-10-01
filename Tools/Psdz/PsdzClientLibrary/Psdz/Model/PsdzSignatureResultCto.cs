using PsdzClient;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz.Model.SecureCoding.SignatureResultCto
{
    [PreserveSource(AttributesModified = true)]
    [DataContract]
    [KnownType(typeof(PsdzSgbmId))]
    public class PsdzSignatureResultCto : IPsdzSignatureResultCto
    {
        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public IPsdzSgbmId Cafd { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public string BootloaderSgbmNumber { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public byte[] Signature { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public int[] CodingProofStamp { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public string KeyAlgorithm { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public string Digest { get; set; }
    }
}
