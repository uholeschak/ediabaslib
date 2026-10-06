using System.Collections.Generic;
using BMW.Rheingold.Psdz;
using BMW.Rheingold.Psdz.Model.SecureCoding;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.SecureCoding;
using RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects;
using RheingoldPsdzWebApi.Adapter.Mapper;

namespace RheingoldPsdzWebApi.Adapter.Mapper
{
    internal class NcdRecalculationEtoMapper : MapperBase<PsdzNcdRecalculationEtoEnum, NcdRecalculationEto>
    {
        protected override IDictionary<PsdzNcdRecalculationEtoEnum, NcdRecalculationEto> CreateMap()
        {
            return new Dictionary<PsdzNcdRecalculationEtoEnum, NcdRecalculationEto>
            {
                {
                    PsdzNcdRecalculationEtoEnum.ALLOW,
                    NcdRecalculationEto.ALLOW
                },
                {
                    PsdzNcdRecalculationEtoEnum.FORCE,
                    NcdRecalculationEto.FORCE
                }
            };
        }
    }
}