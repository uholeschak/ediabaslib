using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.Psdz.Model.Swt;

namespace PsdzClient.Programming
{
    class SwtTypeEnumMapper : ProgrammingEnumMapperBase<PsdzSwtType, SwtType>
    {
        protected override IDictionary<PsdzSwtType, SwtType> CreateMap()
        {
            return new Dictionary<PsdzSwtType, SwtType>
            {
                {
                    PsdzSwtType.Full,
                    SwtType.Full
                },
                {
                    PsdzSwtType.Light,
                    SwtType.Light
                },
                {
                    PsdzSwtType.PreEnabFull,
                    SwtType.PreEnabFull
                },
                {
                    PsdzSwtType.PreEnabLight,
                    SwtType.PreEnabLight
                },
                {
                    PsdzSwtType.Short,
                    SwtType.Short
                },
                {
                    PsdzSwtType.Unknown,
                    SwtType.Unknown
                }
            };
        }
    }
}
