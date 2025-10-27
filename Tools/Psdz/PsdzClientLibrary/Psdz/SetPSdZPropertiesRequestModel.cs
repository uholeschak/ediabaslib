using Newtonsoft.Json;

namespace BMW.Rheingold.Psdz
{
    public class SetPSdZPropertiesRequestModel
    {
        [JsonProperty("dealerId", NullValueHandling = NullValueHandling.Ignore)]
        public string DealerId { get; set; }

        [JsonProperty("plantId", NullValueHandling = NullValueHandling.Ignore)]
        public string PlantId { get; set; }

        [JsonProperty("programmierGeraeteSeriennummer", NullValueHandling = NullValueHandling.Ignore)]
        public string ProgrammierGeraeteSeriennummer { get; set; }

        [JsonProperty("testerEinsatzKennung", NullValueHandling = NullValueHandling.Ignore)]
        public string TesterEinsatzKennung { get; set; }
    }
}