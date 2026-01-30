using BMW.Rheingold.Psdz.Model.Tal;
using BMW.Rheingold.Psdz;
using System.Collections.Generic;
using BMW.Rheingold.Psdz.Model.Tal.TalFilter;

namespace BMW.Rheingold.Psdz
{
    public class PsdzSweTalFilterOptions : IPsdzSweTalFilterOptions
    {
        public IPsdzTa Ta { get; set; }

        public string ProcessClass { get; set; }

        public IDictionary<string, PsdzTalFilterAction> SweFilter { get; set; }
    }
}