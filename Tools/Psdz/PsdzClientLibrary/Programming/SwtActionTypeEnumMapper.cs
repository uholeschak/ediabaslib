using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.Psdz.Model.Swt;

namespace PsdzClient.Programming
{
    class SwtActionTypeEnumMapper : ProgrammingEnumMapperBase<PsdzSwtActionType, SwtActionType>
    {
        protected override IDictionary<PsdzSwtActionType, SwtActionType> CreateMap()
        {
            return new Dictionary<PsdzSwtActionType, SwtActionType>
            {
                {
                    PsdzSwtActionType.ActivateStore,
                    SwtActionType.ActivateStore
                },
                {
                    PsdzSwtActionType.ActivateUpdate,
                    SwtActionType.ActivateUpdate
                },
                {
                    PsdzSwtActionType.ActivateUpgrade,
                    SwtActionType.ActivateUpgrade
                },
                {
                    PsdzSwtActionType.Deactivate,
                    SwtActionType.Deactivate
                },
                {
                    PsdzSwtActionType.ReturnState,
                    SwtActionType.ReturnState
                },
                {
                    PsdzSwtActionType.WriteVin,
                    SwtActionType.WriteVin
                }
            };
        }
    }
}
