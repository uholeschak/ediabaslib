using System.Collections.Generic;
using BMW.Rheingold.Psdz.Model.SecureCoding;

namespace BMW.Rheingold.Psdz
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