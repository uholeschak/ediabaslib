using BMW.Rheingold.Psdz;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace BMW.Rheingold.Psdz
{
    public class ProgrammingTokenCtoModel
    {
        [JsonProperty("tokenVersion", NullValueHandling = NullValueHandling.Ignore)]
        public int TokenVersion { get; set; }

        [JsonProperty("vin", NullValueHandling = NullValueHandling.Ignore)]
        public VinModel Vin { get; set; }

        [JsonProperty("ecuIdentifierCto", NullValueHandling = NullValueHandling.Ignore)]
        public EcuIdentifierCtoModel EcuIdentifier { get; set; }

        [JsonProperty("ecuUidCto", NullValueHandling = NullValueHandling.Ignore)]
        public EcuUidCtoModel EcuUidCto { get; set; }

        [JsonProperty("activeSGBMIDs", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<SgbmIdModel> ActiveSGBMIDs { get; set; }

        [JsonProperty("newSGBMIDs", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<SgbmIdModel> NewSGBMIDs { get; set; }

        [JsonProperty("activeSGBMIDsHash", NullValueHandling = NullValueHandling.Ignore)]
        public byte[] ActiveSGBMIDsHash { get; set; }

        [JsonProperty("validityStartTime", NullValueHandling = NullValueHandling.Ignore)]
        public byte[] ValidityStartTime { get; set; }

        [JsonProperty("validityEndTime", NullValueHandling = NullValueHandling.Ignore)]
        public byte[] ValidityEndTime { get; set; }

        [JsonProperty("signed", NullValueHandling = NullValueHandling.Ignore)]
        public bool IsSigned { get; set; }

        [JsonProperty("programmingTokenAsBytes", NullValueHandling = NullValueHandling.Ignore)]
        public byte[] ProgrammingTokenAsBytes { get; set; }

        public override string ToString()
        {
            return $"ECU {EcuIdentifier.BaseVariant} ({EcuIdentifier.DiagAddress.OffsetAsInt}) - Token as Bytes (V {TokenVersion}): {ProgrammingTokenAsBytes}";
        }
    }
}