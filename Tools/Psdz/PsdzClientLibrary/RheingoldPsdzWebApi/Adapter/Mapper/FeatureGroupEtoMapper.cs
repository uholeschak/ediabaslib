using System.Collections.Generic;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Sfa;
using RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects;

namespace RheingoldPsdzWebApi.Adapter.Mapper
{
    internal class FeatureGroupEtoMapper : MapperBase<PsdzFeatureGroupEtoEnum, FeatureGroupEto>
    {
        protected override IDictionary<PsdzFeatureGroupEtoEnum, FeatureGroupEto> CreateMap()
        {
            return new Dictionary<PsdzFeatureGroupEtoEnum, FeatureGroupEto>
            {
                {
                    PsdzFeatureGroupEtoEnum.CUSTOMER_FEATURES,
                    FeatureGroupEto.CUSTOMER_FEATURES
                },
                {
                    PsdzFeatureGroupEtoEnum.PLANT_SYSTEM_FEATURES,
                    FeatureGroupEto.PLANT_SYSTEM_FEATURES
                },
                {
                    PsdzFeatureGroupEtoEnum.VEHICLE_SYSTEM_FEATURES,
                    FeatureGroupEto.VEHICLE_SYSTEM_FEATURES
                }
            };
        }
    }
}