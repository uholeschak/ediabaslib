using Newtonsoft.Json;

namespace BMW.Rheingold.Psdz
{
    public class SmartActuatorFlashStatusModel
    {
        [JsonProperty("debugInformation", NullValueHandling = NullValueHandling.Ignore)]
        public int DebugInformation;

        [JsonProperty("smartActuatorId", NullValueHandling = NullValueHandling.Ignore)]
        public string SmartActuatorID { get; set; }

        [JsonProperty("programmingStatus", NullValueHandling = NullValueHandling.Ignore)]
        public string ProgrammingStatus { get; set; }
    }
}
