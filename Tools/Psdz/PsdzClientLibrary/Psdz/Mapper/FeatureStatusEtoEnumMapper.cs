using System.Collections.Generic;
using BMW.Rheingold.Psdz;
using BMW.Rheingold.Psdz.Model.Sfa;
using RheingoldPsdzWebApi.Adapter.Contracts;
using RheingoldPsdzWebApi.Adapter.Mapper;

namespace RheingoldPsdzWebApi.Adapter.Mapper
{
    internal class FeatureStatusEtoEnumMapper : MapperBase<PsdzFeatureStatusEtoEnum, FeatureStatusEto>
    {
        protected override IDictionary<PsdzFeatureStatusEtoEnum, FeatureStatusEto> CreateMap()
        {
            return new Dictionary<PsdzFeatureStatusEtoEnum, FeatureStatusEto>
            {
                {
                    PsdzFeatureStatusEtoEnum.DISABLED,
                    FeatureStatusEto.DISABLED
                },
                {
                    PsdzFeatureStatusEtoEnum.ENABLED,
                    FeatureStatusEto.ENABLED
                },
                {
                    PsdzFeatureStatusEtoEnum.EXPIRED,
                    FeatureStatusEto.EXPIRED
                },
                {
                    PsdzFeatureStatusEtoEnum.INITIAL_DISABLED,
                    FeatureStatusEto.INITIAL_DISABLED
                },
                {
                    PsdzFeatureStatusEtoEnum.INVALID,
                    FeatureStatusEto.INVALID
                }
            };
        }
    }
}