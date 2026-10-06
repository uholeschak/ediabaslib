using System.Collections.Generic;
using BMW.Rheingold.Psdz;
using BMW.Rheingold.Psdz.Model.Kds;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Kds;
using RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects;
using RheingoldPsdzWebApi.Adapter.Mapper;

namespace RheingoldPsdzWebApi.Adapter.Mapper
{
    internal class KdsQuickCheckResultEtoMapper : MapperBase<PsdzQuickCheckResultEto, KdsQuickCheckResultEto>
    {
        protected override IDictionary<PsdzQuickCheckResultEto, KdsQuickCheckResultEto> CreateMap()
        {
            return new Dictionary<PsdzQuickCheckResultEto, KdsQuickCheckResultEto>
            {
                {
                    PsdzQuickCheckResultEto.MASTER_INVALID_CLIENT_INVALID,
                    KdsQuickCheckResultEto.MASTER_INVALID_CLIENT_INVALID
                },
                {
                    PsdzQuickCheckResultEto.MASTER_INVALID_CLIENT_OK,
                    KdsQuickCheckResultEto.MASTER_INVALID_CLIENT_OK
                },
                {
                    PsdzQuickCheckResultEto.MASTER_OK_CLIENT_INVALID,
                    KdsQuickCheckResultEto.MASTER_OK_CLIENT_INVALID
                }
            };
        }
    }
}