using System.Collections.Generic;
using PsdzClient.Psdz;

namespace BMW.Rheingold.Psdz
{
    internal class TalFilterActionMapper : MapperBase<PsdzTalFilterAction, ActionValues>
    {
        protected override IDictionary<PsdzTalFilterAction, ActionValues> CreateMap()
        {
            return new Dictionary<PsdzTalFilterAction, ActionValues>
            {
                {
                    PsdzTalFilterAction.Empty,
                    ActionValues.EMPTY
                },
                {
                    PsdzTalFilterAction.AllowedToBeTreated,
                    ActionValues.ALLOWED_TO_BE_TREATED
                },
                {
                    PsdzTalFilterAction.MustBeTreated,
                    ActionValues.MUST_BE_TREATED
                },
                {
                    PsdzTalFilterAction.MustNotBeTreated,
                    ActionValues.MUST_NOT_BE_TREATED
                },
                {
                    PsdzTalFilterAction.OnlyToBeTreatedAndBlockCategoryInAllEcu,
                    ActionValues.ONLY_TO_BE_TREATED_AND_BLOCK_CATEGORY_IN_ALL_ECU
                }
            };
        }
    }
}