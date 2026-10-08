using System.Collections.Generic;

namespace BMW.Rheingold.CoreFramework.Contracts.Programming
{
    public interface IEcuFilterOnSweLevel
    {
        int DiagAddress { get; }

        TaCategories TaCategory { get; }

        TalFilterOptions TalFilterOptions { get; }

        List<ISweTalFilterOptions> SweTalFilterOptions { get; }
    }
}