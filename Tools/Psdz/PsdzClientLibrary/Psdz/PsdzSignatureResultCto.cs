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
        // [UH] Keep data members for backward compatibility
        [DataMember]
        public IPsdzSgbmId Cafd { get; set; }

        [DataMember]
        public string BootloaderSgbmNumber { get; set; }

        [DataMember]
        public byte[] Signature { get; set; }

        [DataMember]
        public int[] CodingProofStamp { get; set; }

        [DataMember]
        public string KeyAlgorithm { get; set; }

        [DataMember]
        public string Digest { get; set; }
    }
}
