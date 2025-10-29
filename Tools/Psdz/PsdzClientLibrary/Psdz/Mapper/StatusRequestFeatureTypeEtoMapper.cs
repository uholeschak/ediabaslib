using System.Collections.Generic;

namespace BMW.Rheingold.Psdz
{
    internal class StatusRequestFeatureTypeEtoMapper : MapperBase<PsdzStatusRequestFeatureTypeEtoEnum, StatusRequestFeatureTypeEto>
    {
        protected override IDictionary<PsdzStatusRequestFeatureTypeEtoEnum, StatusRequestFeatureTypeEto> CreateMap()
        {
            return new Dictionary<PsdzStatusRequestFeatureTypeEtoEnum, StatusRequestFeatureTypeEto>
            {
                {
                    PsdzStatusRequestFeatureTypeEtoEnum.ALL_FEATURES,
                    StatusRequestFeatureTypeEto.ALL_FEATURES
                },
                {
                    PsdzStatusRequestFeatureTypeEtoEnum.SYSTEM_FEATURES,
                    StatusRequestFeatureTypeEto.SYSTEM_FEATURES
                },
                {
                    PsdzStatusRequestFeatureTypeEtoEnum.APPLICATION_FEATURES,
                    StatusRequestFeatureTypeEto.APPLICATION_FEATURES
                }
            };
        }
    }
}