using System;

namespace BMW.ISPI.TRIC.ISTA.Contracts.Models.SeamLM2
{
    public sealed class CCMDemandDetailCCMCcmListInnerBO
    {
        public string ccmId { get; set; }

        public int? mileage { get; set; }

        public int? priority { get; set; }

        public DateTimeOffset? timestamp { get; set; }
    }
}
