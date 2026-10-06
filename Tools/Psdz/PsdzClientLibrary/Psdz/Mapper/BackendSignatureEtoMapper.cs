using System.Collections.Generic;
using BMW.Rheingold.Psdz.Model.SecureCoding;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.SecureCoding;
using RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects;
using RheingoldPsdzWebApi.Adapter.Mapper;

namespace RheingoldPsdzWebApi.Adapter.Mapper
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