using System.Collections.Generic;
using BMW.Rheingold.Psdz.Client;

namespace BMW.Rheingold.Psdz
{
    internal class KdsActionIdEtoMapper : MapperBase<PsdzKdsActionIdEto, KdsActionIdEto>
    {
        protected override IDictionary<PsdzKdsActionIdEto, KdsActionIdEto> CreateMap()
        {
            return new Dictionary<PsdzKdsActionIdEto, KdsActionIdEto>
            {
                {
                    PsdzKdsActionIdEto.CHECK_PARING_CONSISTENCY,
                    KdsActionIdEto.CHECK_PARING_CONSISTENCY
                },
                {
                    PsdzKdsActionIdEto.CUT_COMMUNICATION,
                    KdsActionIdEto.CUT_COMMUNICATION
                },
                {
                    PsdzKdsActionIdEto.LOCK_ECU,
                    KdsActionIdEto.LOCK_ECU
                },
                {
                    PsdzKdsActionIdEto.REPAIR_OR_CLEAR_DATA,
                    KdsActionIdEto.REPAIR_OR_CLEAR_DATA
                },
                {
                    PsdzKdsActionIdEto.SHOW_REACTION,
                    KdsActionIdEto.SHOW_REACTION
                },
                {
                    PsdzKdsActionIdEto.TEST_SIGNATURE,
                    KdsActionIdEto.TEST_SIGNATURE
                },
                {
                    PsdzKdsActionIdEto.TRIGGER_FREE_PAIRING,
                    KdsActionIdEto.TRIGGER_FREE_PAIRING
                },
                {
                    PsdzKdsActionIdEto.TRIGGER_INDIVIDUALIZATION,
                    KdsActionIdEto.TRIGGER_INDIVIDUALIZATION
                },
                {
                    PsdzKdsActionIdEto.TRIGGER_VERIFICATION,
                    KdsActionIdEto.TRIGGER_VERIFICATION
                }
            };
        }
    }
}