using System.Collections.Generic;
using BMW.Rheingold.Psdz.Model.Swt;

namespace BMW.Rheingold.Psdz
{
    internal class SwtActionTypeMapper : NullableEnumMapper<PsdzSwtActionType, SwtActionTypeModel>
    {
        protected override IDictionary<PsdzSwtActionType?, SwtActionTypeModel?> CreateMap()
        {
            return new Dictionary<PsdzSwtActionType?, SwtActionTypeModel?>
            {
                {
                    PsdzSwtActionType.ActivateStore,
                    SwtActionTypeModel.ActivateStore
                },
                {
                    PsdzSwtActionType.ActivateUpdate,
                    SwtActionTypeModel.ActivateUpdate
                },
                {
                    PsdzSwtActionType.ActivateUpgrade,
                    SwtActionTypeModel.ActivateUpgrade
                },
                {
                    PsdzSwtActionType.Deactivate,
                    SwtActionTypeModel.Deactivate
                },
                {
                    PsdzSwtActionType.ReturnState,
                    SwtActionTypeModel.ReturnState
                },
                {
                    PsdzSwtActionType.WriteVin,
                    SwtActionTypeModel.WriteVin
                }
            };
        }
    }
}