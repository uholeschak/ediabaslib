using BMW.Rheingold.Psdz;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace BMW.Rheingold.Psdz
{
    public class DeleteSwtTransactionsRequestModel
    {
        [JsonProperty("swtApplicationIdBlackList", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<SwtApplicationIdModel> SwtApplicationIdBlackList { get; set; }

        [JsonProperty("swtApplicationIdWhiteList", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<SwtApplicationIdModel> SwtApplicationIdWhiteList { get; set; }

        [JsonProperty("tal", NullValueHandling = NullValueHandling.Ignore)]
        public TalModel Tal { get; set; }
    }
}