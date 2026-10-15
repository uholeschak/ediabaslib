using Newtonsoft.Json;
using System.Collections.Generic;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public class OrderListModel
    {
        [JsonProperty("bntnVariantInstances", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<EcuVariantInstanceModel> BntnVariantInstances { get; set; }

        [JsonProperty("numberOfUnits", NullValueHandling = NullValueHandling.Ignore)]
        public int NumberOfUnits { get; set; }
    }
}