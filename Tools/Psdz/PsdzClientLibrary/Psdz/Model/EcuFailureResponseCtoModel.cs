using BMW.Rheingold.Psdz;
using Newtonsoft.Json;

namespace BMW.Rheingold.Psdz
{
    public class EcuFailureResponseCtoModel
    {
        [JsonProperty("cause", NullValueHandling = NullValueHandling.Ignore)]
        public LocalizableMessageToModel Cause { get; set; }

        [JsonProperty("ecuIdentifierCto", NullValueHandling = NullValueHandling.Ignore)]
        public EcuIdentifierModel EcuIdentifierCto { get; set; }

        public override string ToString()
        {
            return $"{EcuIdentifierCto?.BaseVariant} ({EcuIdentifierCto?.DiagAddrAsInt}) - Cause: ({Cause?.MessageId}) {Cause?.Description}";
        }
    }
}