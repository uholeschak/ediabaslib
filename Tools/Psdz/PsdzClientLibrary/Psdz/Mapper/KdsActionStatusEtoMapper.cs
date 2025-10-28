using System.Collections.Generic;
using BMW.Rheingold.Psdz.Model.Kds;

namespace BMW.Rheingold.Psdz
{
    internal class KdsActionStatusEtoMapper : MapperBase<PsdzKdsActionStatusEto, KdsActionStatusEto>
    {
        protected override IDictionary<PsdzKdsActionStatusEto, KdsActionStatusEto> CreateMap()
        {
            return new Dictionary<PsdzKdsActionStatusEto, KdsActionStatusEto>
            {
                {
                    PsdzKdsActionStatusEto.ERROR,
                    KdsActionStatusEto.ERROR
                },
                {
                    PsdzKdsActionStatusEto.FORBIDDEN,
                    KdsActionStatusEto.FORBIDDEN
                },
                {
                    PsdzKdsActionStatusEto.IN_PROGRESS,
                    KdsActionStatusEto.IN_PROGRESS
                },
                {
                    PsdzKdsActionStatusEto.PARTIAL,
                    KdsActionStatusEto.PARTIAL
                },
                {
                    PsdzKdsActionStatusEto.SUCCESS,
                    KdsActionStatusEto.SUCCESS
                },
                {
                    PsdzKdsActionStatusEto.TIMEOUT,
                    KdsActionStatusEto.TIMEOUT
                }
            };
        }
    }
}