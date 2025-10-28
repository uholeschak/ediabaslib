using System.Collections.Generic;
using BMW.Rheingold.Psdz.Model.Certificate;

namespace BMW.Rheingold.Psdz
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