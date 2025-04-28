using BMW.Rheingold.CoreFramework.Contracts.Programming;
using System.Collections.Generic;

namespace BMW.Rheingold.Psdz
{
    public interface IEcuFilterOnSweLevel
    {
        int DiagAddress { get; }

        TaCategories TaCategory { get; }

        TalFilterOptions TalFilterOptions { get; }

        List<ISweTalFilterOptions> SweTalFilterOptions { get; }
    }
}