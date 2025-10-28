using Newtonsoft.Json;
using System.Collections.Generic;

namespace BMW.Rheingold.Psdz
{
    public class NcdModel
    {
        [JsonProperty("cafId", NullValueHandling = NullValueHandling.Ignore)]
        public byte[] CafId { get; set; }

        [JsonProperty("codingArea", NullValueHandling = NullValueHandling.Ignore)]
        public int CodingArea { get; set; }

        [JsonProperty("codingProofStamp", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<int> CodingProofStamp { get; set; }

        [JsonProperty("codingVersion", NullValueHandling = NullValueHandling.Ignore)]
        public string CodingVersion { get; set; }

        [JsonProperty("motorolaSString", NullValueHandling = NullValueHandling.Ignore)]
        public string MotorolaSString { get; set; }

        [JsonProperty("obdRelevantBytes", NullValueHandling = NullValueHandling.Ignore)]
        public byte[] ObdRelevantBytes { get; set; }

        [JsonProperty("obdcrc32", NullValueHandling = NullValueHandling.Ignore)]
        public string Obdcrc32 { get; set; }

        [JsonProperty("signature", NullValueHandling = NullValueHandling.Ignore)]
        public byte[] Signature { get; set; }

        [JsonProperty("signatureBlockAddress", NullValueHandling = NullValueHandling.Ignore)]
        public int SignatureBlockAddress { get; set; }

        [JsonProperty("signatureLength", NullValueHandling = NullValueHandling.Ignore)]
        public int SignatureLength { get; set; }

        [JsonProperty("signed", NullValueHandling = NullValueHandling.Ignore)]
        public bool Signed { get; set; }

        [JsonProperty("tlIdBlockAddress", NullValueHandling = NullValueHandling.Ignore)]
        public int TlIdBlockAddress { get; set; }

        [JsonProperty("userDataCoding1", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<Coding1NcdEntryModel> UserDataCoding1 { get; set; }

        [JsonProperty("userDataCoding2", NullValueHandling = NullValueHandling.Ignore)]
        public byte[] UserDataCoding2 { get; set; }

        [JsonProperty("valid", NullValueHandling = NullValueHandling.Ignore)]
        public bool Valid { get; set; }
    }
}