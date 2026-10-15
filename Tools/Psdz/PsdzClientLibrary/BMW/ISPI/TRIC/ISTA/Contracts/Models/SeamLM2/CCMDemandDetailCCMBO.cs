using System.Collections.Generic;

namespace BMW.ISPI.TRIC.ISTA.Contracts.Models.SeamLM2
{
    public class CCMDemandDetailCCMBO
    {
        public List<CCMDemandDetailCCMCcmListInnerBO> ccmList { get; set; }

        public List<CCMDemandDetailCCMTotalCcmListInnerBO> totalCcmList { get; set; }
    }
}
