using PsdzClientLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace BMW.Rheingold.Psdz.Model.SecureCoding.SignatureResultCto
{
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
