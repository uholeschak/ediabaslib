using System.Collections.Generic;
using BMW.Rheingold.Psdz.Model.Sfa;

namespace BMW.Rheingold.Psdz
{
    internal class SfaLinkTypeEtoMapper : MapperBase<PsdzSfaLinkTypeEtoEnum, SfaLinkTypeEto>
    {
        protected override IDictionary<PsdzSfaLinkTypeEtoEnum, SfaLinkTypeEto> CreateMap()
        {
            return new Dictionary<PsdzSfaLinkTypeEtoEnum, SfaLinkTypeEto>
            {
                {
                    PsdzSfaLinkTypeEtoEnum.VIN_ECU_UID,
                    SfaLinkTypeEto.VIN_ECU_UID
                },
                {
                    PsdzSfaLinkTypeEtoEnum.ECU_UID,
                    SfaLinkTypeEto.ECU_UID
                },
                {
                    PsdzSfaLinkTypeEtoEnum.VIN,
                    SfaLinkTypeEto.VIN
                }
            };
        }
    }
}