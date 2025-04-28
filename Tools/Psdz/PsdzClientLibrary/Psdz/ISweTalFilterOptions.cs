using BMW.Rheingold.CoreFramework.Contracts.Programming;
using BMW.Rheingold.Psdz.Model.Tal;
using System.Collections.Generic;

namespace BMW.Rheingold.Psdz
{
    public interface ISweTalFilterOptions
    {
        string ProcessClass { get; }

        List<string> SgbmIds { get; }

        List<TalFilterOptions> SweFilter { get; }

        IPsdzTa Ta { get; set; }
    }
}