using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace BMW.Rheingold.Psdz.Model.SecureCoding
{
    [DataContract]
    [KnownType(typeof(PsdzCoding1NcdEntry))]
    public class PsdzNcd : IPsdzNcd
    {
        [DataMember]
        public string MotorolaSString { get; set; }

        [DataMember]
        public byte[] CafId { get; set; }

        [DataMember]
        public int CodingArea { get; set; }

        [DataMember]
        public int[] CodingProofStamp { get; set; }

        [DataMember]
        public string CodingVersion { get; set; }

        [DataMember]
        public string OBDCRC32 { get; set; }

        [DataMember]
        public byte[] ObdRelevantBytes { get; set; }

        [DataMember]
        public byte[] Signature { get; set; }

        [DataMember]
        public int SignatureBlockAddress { get; set; }

        [DataMember]
        public int SignatureLength { get; set; }

        [DataMember]
        public int TlIdBlockAddress { get; set; }

        [DataMember]
        public IList<IPsdzCoding1NcdEntry> UserDataCoding1 { get; set; }

        [DataMember]
        public byte[] UserDataCoding2 { get; set; }

        [DataMember]
        public bool IsSigned { get; set; }

        [DataMember]
        public bool IsValid { get; set; }
    }
}
