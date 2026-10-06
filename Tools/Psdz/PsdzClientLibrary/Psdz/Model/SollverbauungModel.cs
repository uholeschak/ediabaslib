using BMW.Rheingold.Psdz;
using Newtonsoft.Json;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public class SollverbauungModel
    {
        [JsonProperty("asXml", NullValueHandling = NullValueHandling.Ignore)]
        public string AsXml { get; set; }

        [JsonProperty("orderList", NullValueHandling = NullValueHandling.Ignore)]
        public OrderListModel OrderList { get; set; }

        [JsonProperty("svt", NullValueHandling = NullValueHandling.Ignore)]
        public SvtModel Svt { get; set; }
    }
}