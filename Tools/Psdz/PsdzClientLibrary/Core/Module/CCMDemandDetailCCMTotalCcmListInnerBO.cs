using System;

namespace BMW.ISPI.TRIC.ISTA.Contracts.Models.SeamLM2
{
    public class CCMDemandDetailCCMTotalCcmListInnerBO
    {
        public int? ccmId { get; set; }

        public int? lastMileage { get; set; }

        public DateTimeOffset? lastOccurenceTimestamp { get; set; }

        public int? occurences { get; set; }
    }
}
