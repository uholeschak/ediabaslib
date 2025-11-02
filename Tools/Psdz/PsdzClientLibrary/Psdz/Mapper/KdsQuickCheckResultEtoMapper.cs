using System.Collections.Generic;
using BMW.Rheingold.Psdz.Model.Kds;

namespace BMW.Rheingold.Psdz
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
                },
                {
                    PsdzQuickCheckResultEto.MASTER_UNKNOWN_CLIENT_NOT_PAIRED,
                    KdsQuickCheckResultEto.MASTER_UNKNOWN_CLIENT_NOT_PAIRED
                },
                // [UH] added missing mappings
                {
                    PsdzQuickCheckResultEto.MASTER_NOT_PAIRED_CLIENT_UNKNOWN,
                    KdsQuickCheckResultEto.MASTER_NOT_PAIRED_CLIENT_UNKNOWN
                },
                {
                    PsdzQuickCheckResultEto.MASTER_UNKNOWN_CLIENT_TIMEOUT,
                    KdsQuickCheckResultEto.MASTER_UNKNOWN_CLIENT_TIMEOUT
                },
                {
                    PsdzQuickCheckResultEto.MASTER_ERROR_CLIENT_ERROR,
                    KdsQuickCheckResultEto.MASTER_ERROR_CLIENT_ERROR
                }
            };
        }
    }
}