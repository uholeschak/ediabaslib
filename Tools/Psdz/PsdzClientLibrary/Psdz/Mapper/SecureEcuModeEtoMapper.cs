using System.Collections.Generic;
using BMW.Rheingold.Psdz.Model.Sfa;

namespace BMW.Rheingold.Psdz
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