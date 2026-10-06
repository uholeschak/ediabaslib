using System.Collections.Generic;
using BMW.Rheingold.Psdz;
using BMW.Rheingold.Psdz.Model.Sfa;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Sfa;
using RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects;

namespace RheingoldPsdzWebApi.Adapter.Mapper
{
    internal class SecureEcuModeEtoMapper : MapperBase<PsdzSecureEcuModeEtoEnum, SecureEcuModeEto>
    {
        protected override IDictionary<PsdzSecureEcuModeEtoEnum, SecureEcuModeEto> CreateMap()
        {
            return new Dictionary<PsdzSecureEcuModeEtoEnum, SecureEcuModeEto>
            {
                {
                    PsdzSecureEcuModeEtoEnum.FIELD,
                    SecureEcuModeEto.FIELD
                },
                {
                    PsdzSecureEcuModeEtoEnum.PLANT,
                    SecureEcuModeEto.PLANT
                },
                {
                    PsdzSecureEcuModeEtoEnum.ENGINEERING,
                    SecureEcuModeEto.ENGINEERING
                }
            };
        }
    }
}