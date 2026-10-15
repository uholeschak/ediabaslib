using System;
using System.Collections.Generic;
using PsdzClient;

namespace BMW.ISPI.TRIC.ISTA.Contracts.Models.SeamLM2
{
    public class ServiceDemandBO
    {
        public string category { get; set; }

        public string key { get; set; }

        public string state { get; set; }

        public DateTimeOffset? dueDate { get; set; }

        public int? remainingDistance { get; set; }

        public int? remainingDistanceMiles { get; set; }

        public int? urgency { get; set; }

        public string purpose { get; set; }

        public DateTimeOffset? createdAt { get; set; }

        public DateTimeOffset? updatedAt { get; set; }

        public ServiceDemandDetails details { get; set; }

        public int? counter { get; set; }

        public int? travelledDistanceReference { get; set; }

        public bool? isLive { get; set; }

        public string eventIdentifier { get; set; }

        public bool? isInWorkshop { get; set; }

        [PreserveSource(Hint = "DemandContentsBO", Placeholder = true)]
        public List<PlaceholderType> demandContents { get; set; }

        public DateTimeOffset? openedAt { get; set; }

        public bool? isUnreliable { get; set; }

        public string appointmentKey { get; set; }
    }
}
