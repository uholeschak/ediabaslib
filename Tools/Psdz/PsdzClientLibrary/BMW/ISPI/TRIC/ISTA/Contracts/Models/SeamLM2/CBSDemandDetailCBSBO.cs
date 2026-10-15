using BMW.ISPI.TRIC.ISTA.Contracts.Models.SeamLM2.Enums;

namespace BMW.ISPI.TRIC.ISTA.Contracts.Models.SeamLM2
{
    public class CBSDemandDetailCBSBO
    {
        public StatusColor? statusColor { get; set; }

        public string advisoryText { get; set; }

        public int? fruUnitValue { get; set; }

        public string fruUnitNumber { get; set; }

        public bool? isRecommended { get; set; }

        public int? remainingTime { get; set; }

        public bool? isUnselectable { get; set; }

        public BundlingEligible? bundlingEligible { get; set; }

        public int? includeRangeTime { get; set; }

        public string serviceDemandCallId { get; set; }

        public int? includeRangeDistance { get; set; }

        public int? forecastRemainingTime { get; set; }

        public int? forecastRemainingDistance { get; set; }

        public string serviceDemandParentIdentifier { get; set; }

        public bool? isDayAccurateDueDate { get; set; }

        public bool? isDueAndIncludedInSiContract { get; set; }

        public bool? isIncludedInSiContract { get; set; }

        public bool? isPreSelected { get; set; }

        public int? averageWeeklyDistance { get; set; }

        public bool? associatedLabourOperation { get; set; }

        public bool? linkedLevelTwo { get; set; }

        public bool? isOverwrittenByBackendDueDate { get; set; }

        public bool? isVehicleDueDateDifferentToBackend { get; set; }

        public int? originalServiceCounter { get; set; }
    }
}
