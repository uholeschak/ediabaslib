using System.Collections.Generic;
using BMW.Rheingold.Psdz.Model.SecureCoding;

namespace BMW.Rheingold.Psdz
{
    internal class BackendNcdCalculationEtoMapper : MapperBase<PsdzBackendNcdCalculationEtoEnum, BackendNcdCalculationEto>
    {
        protected override IDictionary<PsdzBackendNcdCalculationEtoEnum, BackendNcdCalculationEto> CreateMap()
        {
            return new Dictionary<PsdzBackendNcdCalculationEtoEnum, BackendNcdCalculationEto>
            {
                {
                    PsdzBackendNcdCalculationEtoEnum.ALLOW,
                    BackendNcdCalculationEto.ALLOW
                },
                {
                    PsdzBackendNcdCalculationEtoEnum.FORCE,
                    BackendNcdCalculationEto.FORCE
                },
                {
                    PsdzBackendNcdCalculationEtoEnum.MUST_NOT,
                    BackendNcdCalculationEto.MUST_NOT
                }
            };
        }
    }
}