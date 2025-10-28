using System.Collections.Generic;
using BMW.Rheingold.Psdz.Model.Swt;

namespace BMW.Rheingold.Psdz
{
    internal class SwtTypeEnumMapper : MapperBase<PsdzSwtType, SwtTypeModel>
    {
        protected override IDictionary<PsdzSwtType, SwtTypeModel> CreateMap()
        {
            return new Dictionary<PsdzSwtType, SwtTypeModel>
            {
                {
                    PsdzSwtType.Full,
                    SwtTypeModel.Full
                },
                {
                    PsdzSwtType.Light,
                    SwtTypeModel.Light
                },
                {
                    PsdzSwtType.PreEnabFull,
                    SwtTypeModel.PreEnabFull
                },
                {
                    PsdzSwtType.PreEnabLight,
                    SwtTypeModel.PreEnabLight
                },
                {
                    PsdzSwtType.Short,
                    SwtTypeModel.Short
                },
                {
                    PsdzSwtType.Unknown,
                    SwtTypeModel.Unknown
                }
            };
        }
    }
}