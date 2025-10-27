using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace BMW.Rheingold.Psdz
{
    [JsonConverter(typeof(JsonInheritanceConverter), new object[] { "discriminatorType" })]
    [JsonInheritance("SvtModel", typeof(SvtModel))]
    public class StandardSvtModel
    {
        [JsonProperty("asString", NullValueHandling = NullValueHandling.Ignore)]
        public string AsString { get; set; }

        [JsonProperty("ecus", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<EcuModel> Ecus { get; set; }

        [JsonProperty("hoSignature", NullValueHandling = NullValueHandling.Ignore)]
        public byte[] HoSignature { get; set; }

        [JsonProperty("hoSignatureDate", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime HoSignatureDate { get; set; }

        [JsonProperty("version", NullValueHandling = NullValueHandling.Ignore)]
        public int Version { get; set; }
    }
}