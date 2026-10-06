using BMW.Rheingold.Psdz.Model.Tal;
using BMW.Rheingold.Psdz;
using System.Collections.Generic;
using BMW.Rheingold.Psdz.Model.Tal.TalFilter;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Tal.TalFilter
{
    public interface IPsdzSweTalFilterOptions
    {
        IPsdzTa Ta { get; }

        string ProcessClass { get; }

        IDictionary<string, PsdzTalFilterAction> SweFilter { get; }
    }
}