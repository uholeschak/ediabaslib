using System;
using BMW.Authoring.Programming.API.Models.Interfaces;

namespace BMW.Authoring.Programming.API.Models.Implementation
{
    public class CombinedEcuHousingEntryResponse : ICombinedEcuHousingEntryResponse
    {
        public int[] RequiredEcuAddresses { get; set; }

        public CombinedEcuHousingEntryResponse()
        {
            RequiredEcuAddresses = Array.Empty<int>();
        }
    }
}
