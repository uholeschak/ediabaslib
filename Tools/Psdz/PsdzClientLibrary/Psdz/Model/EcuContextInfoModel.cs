using BMW.Rheingold.Psdz;
using Newtonsoft.Json;
using System;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public class EcuContextInfoModel
    {
        [JsonProperty("ecuId", NullValueHandling = NullValueHandling.Ignore)]
        public EcuIdentifierModel EcuId { get; set; }

        [JsonProperty("lastProgrammingDate", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime LastProgrammingDate { get; set; }

        [JsonProperty("manufacturingDate", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime ManufacturingDate { get; set; }

        [JsonProperty("performedFlashCycles", NullValueHandling = NullValueHandling.Ignore)]
        public int PerformedFlashCycles { get; set; }

        [JsonProperty("programCounter", NullValueHandling = NullValueHandling.Ignore)]
        public int ProgramCounter { get; set; }

        [JsonProperty("remainingFlashCycles", NullValueHandling = NullValueHandling.Ignore)]
        public int RemainingFlashCycles { get; set; }
    }
}