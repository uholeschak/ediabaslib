using System.Collections.Generic;
using BMW.Rheingold.Psdz;
using BMW.Rheingold.Psdz.Model.Tal.TalFilter;
using PsdzClient.Psdz;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Tal.TalFilter;
using RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects;
using RheingoldPsdzWebApi.Adapter.Mapper;

namespace RheingoldPsdzWebApi.Adapter.Mapper
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