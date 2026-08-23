using Newtonsoft.Json;
using System.Collections.Generic;

namespace BMW.ISPI.TRIC.ISTA.Contracts.Models.BatteryDemandService
{
    public sealed class CCM
    {
        [JsonProperty("ccmList")]
        public List<CcmListItem> CcmList { get; set; }

        [JsonProperty("totalCcmList")]
        public List<TotalCcmListItem> TotalCcmList { get; set; }
    }
}
