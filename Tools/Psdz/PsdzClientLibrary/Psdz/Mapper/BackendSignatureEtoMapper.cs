using System.Collections.Generic;
using BMW.Rheingold.Psdz.Model.SecureCoding;

namespace BMW.Rheingold.Psdz
{
    internal class BackendSignatureEtoMapper : MapperBase<PsdzBackendSignatureEtoEnum, BackendSignatureEto>
    {
        protected override IDictionary<PsdzBackendSignatureEtoEnum, BackendSignatureEto> CreateMap()
        {
            return new Dictionary<PsdzBackendSignatureEtoEnum, BackendSignatureEto>
            {
                {
                    PsdzBackendSignatureEtoEnum.ALLOW,
                    BackendSignatureEto.ALLOW
                },
                {
                    PsdzBackendSignatureEtoEnum.FORCE,
                    BackendSignatureEto.FORCE
                },
                {
                    PsdzBackendSignatureEtoEnum.MUST_NOT,
                    BackendSignatureEto.MUST_NOT
                }
            };
        }
    }
}