using BMW.Rheingold.Psdz;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace BMW.Rheingold.Psdz
{
    public class CheckSwesRequestModel
    {
        [JsonProperty("sgbmIdList", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<SgbmIdModel> SgbmIdList { get; set; }
    }
}