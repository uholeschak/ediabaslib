using System.Collections.Generic;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Swt;
using RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects;

namespace RheingoldPsdzWebApi.Adapter.Mapper
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