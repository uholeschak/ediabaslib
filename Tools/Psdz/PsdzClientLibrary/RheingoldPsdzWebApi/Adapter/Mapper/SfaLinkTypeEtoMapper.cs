using System.Collections.Generic;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Sfa;
using RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects;

namespace RheingoldPsdzWebApi.Adapter.Mapper
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