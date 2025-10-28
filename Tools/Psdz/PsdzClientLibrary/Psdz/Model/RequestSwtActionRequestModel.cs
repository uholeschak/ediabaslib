using Newtonsoft.Json;

namespace BMW.Rheingold.Psdz
{
    public class RequestSwtActionRequestModel
    {
        [JsonProperty("periodicalCheck", NullValueHandling = NullValueHandling.Ignore)]
        public bool PeriodicalCheck { get; set; }
    }
}