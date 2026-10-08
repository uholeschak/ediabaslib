using System.Collections.Generic;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Tal;

namespace BMW.Rheingold.CoreFramework.Contracts.Programming
{
    public interface ISweTalFilterOptions
    {
        string ProcessClass { get; }

        List<string> SgbmIds { get; }

        List<TalFilterOptions> SweFilter { get; }

        IPsdzTa Ta { get; set; }
    }
}