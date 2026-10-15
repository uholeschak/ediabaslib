using System.Collections.Generic;
using BMW.Rheingold.CoreFramework.Contracts.Programming;

namespace BMW.Rheingold.Programming.ProgrammingEngine
{
    public class EcuFilterOnSweLevel : IEcuFilterOnSweLevel
    {
        public int DiagAddress { get; set; }

        public TaCategories TaCategory { get; set; }

        public TalFilterOptions TalFilterOptions { get; set; }

        public List<ISweTalFilterOptions> SweTalFilterOptions { get; set; } = new List<ISweTalFilterOptions>();
    }
}