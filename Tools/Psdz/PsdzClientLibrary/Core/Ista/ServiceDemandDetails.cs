namespace BMW.ISPI.TRIC.ISTA.Contracts.Models.SeamLM2
{
    public class ServiceDemandDetails
    {
        public TYRDemandDetailTYRBO TYR { get; set; }

        public CCMDemandDetailCCMBO CCM { get; set; }

        public BATDemandDetailBATBO BAT { get; set; }

        public RSUDemandDetailRSUBO RSU { get; set; }

        public TACDemandDetailTACBO TAC { get; set; }

        public CBSDemandDetailCBSBO CBS { get; set; }

        public SCDDemandDetailSCDBO SCD { get; set; }

        public SIADemandDetailSIABO SIA { get; set; }

        public EPMDemandDetailEPMBO EPM { get; set; }

        public HVSDemandDetailSHVSBO HVS { get; set; }
    }
}
