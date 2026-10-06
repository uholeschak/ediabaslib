using System.Collections.Generic;
using BMW.Rheingold.Psdz;
using BMW.Rheingold.Psdz.Model.Certificate;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Certificate;
using RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects;

namespace RheingoldPsdzWebApi.Adapter.Mapper
{
    internal class SecurityMemoryObjectSourceEtoMapper : MapperBase<PsdzCertMemoryObjectSource, SecurityMemoryObjectSourceEto>
    {
        protected override IDictionary<PsdzCertMemoryObjectSource, SecurityMemoryObjectSourceEto> CreateMap()
        {
            return new Dictionary<PsdzCertMemoryObjectSource, SecurityMemoryObjectSourceEto>
            {
                {
                    PsdzCertMemoryObjectSource.CBB,
                    SecurityMemoryObjectSourceEto.CBB
                },
                {
                    PsdzCertMemoryObjectSource.VEHICLE,
                    SecurityMemoryObjectSourceEto.VEHICLE
                },
                {
                    PsdzCertMemoryObjectSource.UNKNOWN,
                    SecurityMemoryObjectSourceEto.UNKNOWN
                }
            };
        }
    }
}