using System.Collections.Generic;
using BMW.Rheingold.Psdz;
using BMW.Rheingold.Psdz.Model.SecureCoding;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.SecureCoding;
using RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects;
using RheingoldPsdzWebApi.Adapter.Mapper;

namespace RheingoldPsdzWebApi.Adapter.Mapper
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