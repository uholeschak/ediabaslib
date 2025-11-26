using BMW.Rheingold.Psdz;
using System.Collections.Generic;

namespace BMW.Rheingold.Psdz
{
    public class EcuFilterOnSweLevel : IEcuFilterOnSweLevel
    {
        public int DiagAddress { get; set; }

        public TaCategories TaCategory { get; set; }

        public TalFilterOptions TalFilterOptions { get; set; }

        public List<ISweTalFilterOptions> SweTalFilterOptions { get; set; } = new List<ISweTalFilterOptions>();
    }
}