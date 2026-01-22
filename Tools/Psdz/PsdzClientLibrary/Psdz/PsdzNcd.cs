using PsdzClient;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz.Model.SecureCoding
{
    [PreserveSource(AttributesModified = true)]
    [DataContract]
    [KnownType(typeof(PsdzCoding1NcdEntry))]
    public class PsdzNcd : IPsdzNcd
    {
        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public string MotorolaSString { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public byte[] CafId { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public int CodingArea { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public int[] CodingProofStamp { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public string CodingVersion { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public string OBDCRC32 { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public byte[] ObdRelevantBytes { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public byte[] Signature { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public int SignatureBlockAddress { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public int SignatureLength { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public int TlIdBlockAddress { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public IList<IPsdzCoding1NcdEntry> UserDataCoding1 { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public byte[] UserDataCoding2 { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public bool IsSigned { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public bool IsValid { get; set; }
    }
}
