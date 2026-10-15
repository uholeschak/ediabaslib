using System.Collections.Generic;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Tal.TalFilter
{
    public interface IPsdzSweTalFilterOptions
    {
        IPsdzTa Ta { get; }

        string ProcessClass { get; }

        IDictionary<string, PsdzTalFilterAction> SweFilter { get; }
    }
}