using System.Collections.Generic;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Tal.TalFilter
{
    public class PsdzSweTalFilterOptions : IPsdzSweTalFilterOptions
    {
        public IPsdzTa Ta { get; set; }

        public string ProcessClass { get; set; }

        public IDictionary<string, PsdzTalFilterAction> SweFilter { get; set; }
    }
}