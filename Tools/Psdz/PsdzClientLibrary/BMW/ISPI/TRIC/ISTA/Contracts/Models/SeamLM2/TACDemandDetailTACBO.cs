using BMW.ISPI.TRIC.ISTA.Contracts.Models.SeamLM2.Enums;

namespace BMW.ISPI.TRIC.ISTA.Contracts.Models.SeamLM2
{
    public sealed class TACDemandDetailTACBO
    {
        public SalesChannel? salesChannel { get; set; }

        public CampaignType? campaignType { get; set; }

        public LocalIndicator? localIndicator { get; set; }
    }
}
